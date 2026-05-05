using GitHubIssueTriageAgent.Helpers;
using GitHubIssueTriageAgent.Services;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GitHubIssueTriageAgent.Agents;

public sealed class ToolCallingTriageAgent
{
    private readonly ChatClient _chatClient;
    private readonly GitHubIssueService _github;

    public ToolCallingTriageAgent(string openAiKey, GitHubIssueService github)
    {
        _chatClient = new ChatClient("gpt-4.1-mini", openAiKey);
        _github = github;
    }

    public async Task<string> RunAsync(string owner, string repo , int issueNumber)
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


        while (true)
        {
            var completion = await _chatClient.CompleteChatAsync(messages, options);

            if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                messages.Add(new AssistantChatMessage(completion.Value));

                foreach(var toolCall in completion.Value.ToolCalls)
                {
                    var json = await ExecuteToolAsync(toolCall.FunctionName, toolCall.FunctionArguments);

                    messages.Add(new ToolChatMessage(toolCall.Id, json));
                }

                continue;
            }

            return completion.Value.Content[0].Text;
        }

    }

    private async Task<string> ExecuteToolAsync(string functionName, BinaryData arguments)
    {
        using var doc = JsonDocument.Parse(arguments);
        var root = doc.RootElement;

        return functionName switch
        {
            "get_issue" => JsonConvert.SerializeObject(
                await _github.GetIssueAsync(
                    root.GetProperty("owner").GetString()!,
                    root.GetProperty("repo").GetString()!,
                    root.GetProperty("issueNumber").GetInt32()!
                 )
                ),
            "get_repo_labels" => JsonConvert.SerializeObject(
                await _github.GetRepositoryLabelsAsync(
                    root.GetProperty("owner").GetString()!,
                    root.GetProperty("repo").GetString()!
                 )
                ),

            _ => throw new InvalidOperationException($"Unknown tool: {functionName}")
        };

    }
}
