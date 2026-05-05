using GitHubIssueTriageAgent.Helpers;
using GitHubIssueTriageAgent.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Agents;

public sealed class ReproductionAgent : BaseAgent
{
    public ReproductionAgent(string key) : base(key) { }

    public async Task Run(TriageState state)
    {
        var result = await AskAsync(PromptHelper.ReproductionPrompt,
        state.Issue.Body);

        if (result.Contains("NONE"))
        {
            state.MissingInformation.Add("Reproduction steps not provided");
        }
        else
        {
            state.ReproductionSteps = result
                .Split('\n')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }
    }
}
