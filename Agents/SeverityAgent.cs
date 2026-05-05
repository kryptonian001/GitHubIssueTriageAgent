using GitHubIssueTriageAgent.Helpers;
using GitHubIssueTriageAgent.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Agents;

internal class SeverityAgent : BaseAgent
{
    public SeverityAgent(string key) : base(key) { }

    public async Task Run(TriageState state)
    {
        var result = await AskAsync(PromptHelper.SeverityPrompt,
        state.Issue.Body);

        state.Severity = result.Trim();
    }
}
