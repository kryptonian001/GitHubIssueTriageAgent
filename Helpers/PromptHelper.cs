using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Helpers;

internal static class PromptHelper
{
    public const string ClassificationPrompt = @"
        Classify the issue into:
        bug, feature, question, tech_debt, support

        Return ONLY the classification.";

    public const string ReproductionPrompt = @"
        Extract reproduction steps as a numbered list.
        If none exist, say: NONE";

    public const string LabelPrompt = @"
        Suggest GitHub labels based on the issue.
        Return a comma-separated list.";

    public const string SeverityPrompt = @"
        Classify severity:
        low, medium, high, critical

        Consider impact and scope.
        Return ONLY one word.";

    public const string ResponsePrompt = @"
        Write a professional GitHub response.
        Ask for missing info if needed.";


    public const string TriageAgentPrompt = @"
        You are a GitHub issue triage agent.

        You may call tools to inspect GitHub issue data.
        Do not invent facts.
        Use only tool-provided data.
        Produce a Markdown triage report.";

    public static string TriageChatMessagePrompt(string owner, string repo, int issueNumber)
    {
        return @$"            
            Triage issue #{issueNumber} in {owner}/{repo}.

            Include:
            - classification
            - severity
            - suggested labels
            - reproduction steps
            - missing information
            - developer-ready task
            - suggested response comment
            - confidence";
    }
}
