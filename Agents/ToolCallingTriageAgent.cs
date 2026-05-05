using GitHubIssueTriageAgent.Helpers;
using GitHubIssueTriageAgent.Services;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.Json;

namespace GitHubIssueTriageAgent.Agents;

public sealed class ToolCallingTriageAgent
{
    private readonly ChatClient _chatClient;
    private readonly GitHubIssueService _github;
    private const int MaxIterations = 10;


    public ToolCallingTriageAgent(string openAiKey, GitHubIssueService github)
    {
        _chatClient = new ChatClient("gpt-4.1-mini", openAiKey);
        _github = github;
    }

    public async Task<string> RunAsync(string owner, string repo, int issueNumber)
    {
        var getIssueTool = ChatTool.CreateFunctionTool(
            functionName: ChatToolHelper.GetToolOptions.Name,
            functionDescription: ChatToolHelper.GetToolOptions.Description,
            functionParameters: ChatToolHelper.GetToolOptions.Parameters
            );

        var getRepoLabelsTool = ChatTool.CreateFunctionTool(
            functionName: ChatToolHelper.GetRepoLabelsOptions.Name,
            functionDescription: ChatToolHelper.GetRepoLabelsOptions.Description,
            functionParameters: ChatToolHelper.GetRepoLabelsOptions.Parameters
            );

        var options = new ChatCompletionOptions
        {
            Tools = { getIssueTool, getRepoLabelsTool }
        };

        List<ChatMessage> messages =
        [
            new SystemChatMessage(PromptHelper.TriageAgentPrompt),
            new UserChatMessage(PromptHelper.TriageChatMessagePrompt(owner, repo, issueNumber))
        ];

        bool getIssueWasCalled = false;
        int iterationCount = 0;


        while (true)
        {

            if (++iterationCount > MaxIterations)
            {
                throw new InvalidOperationException(
                    $"Triage agent exceeded maximum iterations ({MaxIterations}). " +
                    "The model may be stuck in a tool-calling loop.");
            }

            var completion = await _chatClient.CompleteChatAsync(messages, options);

            if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                messages.Add(new AssistantChatMessage(completion.Value));

                foreach (var toolCall in completion.Value.ToolCalls)
                {
                    if (toolCall.FunctionName == "get_issue")
                        getIssueWasCalled = true;


                    var json = await ExecuteToolAsync(toolCall.FunctionName, toolCall.FunctionArguments);

                    messages.Add(new ToolChatMessage(toolCall.Id, json));
                }

                continue;
            }

            if (!getIssueWasCalled)
                throw new InvalidOperationException(
                    "Cannot complete triage: get_issue tool was never called. " +
                    "Triage reports must be based on tool-provided data.");

            return completion.Value.Content[0].Text;
        }

    }

    private async Task<string> ExecuteToolAsync(string functionName, BinaryData arguments)
    {
        using var doc = JsonDocument.Parse(arguments);
        var root = doc.RootElement;



        return functionName switch
        {
        "get_issue" => await GetIssue(root),
        "get_repo_labels" => await GetRepoLabels(root),
            _ => throw new InvalidOperationException($"Unknown tool: {functionName}")
        };

    }

    private async Task<string> GetIssue(JsonElement root)
    {
        var owner = root.GetProperty("owner").GetString();
        var repo = root.GetProperty("repo").GetString();
        var issueNumber = root.GetProperty("issueNumber").GetInt32();

        if (IsValidParameters(owner, repo))
        {
            throw new UnauthorizedAccessException(
                $"Tool call parameters (owner={owner}, repo={repo}, issue={issueNumber}) " +
                $"do not match CLI target allowed Owner, Repo and/or IssueNumber). " +
                "Possible prompt injection attempt detected.");
        }

        return JsonConvert.SerializeObject(await _github.GetIssueAsync(owner, repo, issueNumber));
    }

    private async Task<string> GetRepoLabels(JsonElement root)
    {
        var owner = root.GetProperty("owner").GetString();
        var repo = root.GetProperty("repo").GetString();

        if (IsValidParameters(owner, repo))
        {
            throw new UnauthorizedAccessException(
                $"Tool call parameters (owner={owner}, repo={repo}) " +
                $"do not match CLI target allowed Owner, Repo). " +
                "Possible prompt injection attempt detected.");
        }

        return JsonConvert.SerializeObject( await _github.GetRepositoryLabelsAsync(owner, repo));
    }

    private bool IsValidParameters(string owner, string repo)
    {
        string[] validOwners = { "kryptonian001" };
        string[] validRepos = { "GitHubIssueTriageAgen" };

        return validOwners.Contains(owner) && validRepos.Contains(repo);
    }
}
