using GitHubIssueTriageAgent.Models;
using Newtonsoft.Json;
using Octokit;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GitHubIssueTriageAgent.Agents;

public abstract class BaseAgent
{
    private readonly ChatClient _chatClient;

    public BaseAgent(string apiKey)
    {
        _chatClient = new ChatClient(model: "gpt-4.1-mini", apiKey);
    }

    protected async Task<string> AskAsync(string system, string user)
    {
        var response = await _chatClient.CompleteChatAsync(
        [
            new SystemChatMessage(system),
            new UserChatMessage(user)
        ]);

        return response.Value.Content[0].Text;
    }
}
