using GitHubIssueTriageAgent.Models;
using Octokit;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GitHubIssueTriageAgent.Agents;

public sealed class IssueTriageAgent
{
    private readonly ChatClient _chatClient;

    public IssueTriageAgent(string apiKey)
    {
        _chatClient = new ChatClient(model: "gpt-4.1-mini", apiKey);
    }

    public async Task<TriageReport> TriageAsync(GitHubIssueContext issue)
    {
        var prompt = BuildPrompt(issue);

        var response = await _chatClient.CompleteChatAsync(
        [
            new SystemChatMessage("""
            You are a senior software engineering triage assistant.

            Rules:
            - Do not invent facts.
            - Only use the issue title, body, labels, and comments.
            - If information is missing, list it.
            - Return valid JSON only.
            """),
            new UserChatMessage(prompt)                
        ]);

        var json = response.Value.Content[0].Text;
;

        return JsonSerializer.Deserialize<TriageReport>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    private static string BuildPrompt(GitHubIssueContext issue)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Analyze this GitHub issue and return a JSON object matching this shape:");
        sb.AppendLine("""
        {
          "classification": "bug | feature | question | tech_debt | support",
          "summary": "...",
          "severity": "low | medium | high | critical",
          "suggestedLabels": ["..."],
          "reproductionSteps": ["..."],
          "missingInformation": ["..."],
          "developerReadyTask": "...",
          "suggestedResponseComment": "...",
          "confidence": "low | medium | high"
        }
        """);

        sb.AppendLine();
        sb.AppendLine($"Repository: {issue.RepositoryOwner}/{issue.RepositoryName}");
        sb.AppendLine($"Issue Number: {issue.IssueNumber}");
        sb.AppendLine($"Author: {issue.Author}");
        sb.AppendLine($"Title: {issue.Title}");
        sb.AppendLine();
        sb.AppendLine("Body:");
        sb.AppendLine(issue.Body);
        sb.AppendLine();
        sb.AppendLine("Existing Labels:");
        sb.AppendLine(string.Join(", ", issue.ExistingLabels));
        sb.AppendLine();
        sb.AppendLine("Comments:");
        foreach (var comment in issue.Comments)
        {
            sb.AppendLine("- " + comment);
        }

        return sb.ToString();
    }
}
