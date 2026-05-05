using GitHubIssueTriageAgent.Helpers;
using GitHubIssueTriageAgent.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Agents;

public sealed class ResponseAgent : BaseAgent
{
    public ResponseAgent(string key) : base(key) { }

    public async Task Run(TriageState state)
    {
        var result = await AskAsync(PromptHelper.ResponsePrompt,
        state.Issue.Body);

        state.ResponseComment = result;
    }
}
