using GitHubIssueTriageAgent.Helpers;
using GitHubIssueTriageAgent.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Agents;

public sealed class LabelAgent : BaseAgent
{
    public LabelAgent(string key) : base(key) { }

    public async Task Run(TriageState state)
    {
        var result = await AskAsync(PromptHelper.LabelPrompt,
        state.Issue.Body);

        state.SuggestedLabels = result
            .Split(',')
            .Select(x => x.Trim())
            .ToList();
    }
}
