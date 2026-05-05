using GitHubIssueTriageAgent.Helpers;
using GitHubIssueTriageAgent.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Agents;

public sealed class ClassificationAgent : BaseAgent
{
    public ClassificationAgent(string key) : base(key) { }

    public async Task Run(TriageState state)
    {
        var result = await AskAsync(PromptHelper.ClassificationPrompt,
            state.Issue.Title + "\n" + state.Issue.Body);

        state.Classification = result.Trim();
    }
}
