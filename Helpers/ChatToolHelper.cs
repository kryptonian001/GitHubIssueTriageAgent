using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Helpers;

internal static class ChatToolHelper
{
    internal static IssueToolParameters GetToolOptions => new IssueToolParameters();
    internal static RepoLabelToolParameters GetRepoLabelsOptions => new RepoLabelToolParameters();
}


internal class IssueToolParameters
{
    internal string Name => "get_issue";
    internal string Description => "Gets the GitHub issue title, body, labels, author, and comments.";
    internal BinaryData Parameters => BinaryData.FromObjectAsJson<object>(new
    {
        type = "object",
        properties = new
        {
            owner = new { type = "string" },
            repo = new { type = "string" },
            issueNumber = new { type = "integer" }
        },
        required = new[] { "owner", "repo", "issueNumber" }
    });
}


internal class RepoLabelToolParameters
{
    internal string Name => "get_repo_labels";
    internal string Description => "Gets the available labels for the repository.";
    internal BinaryData Parameters => BinaryData.FromObjectAsJson<object>(new
    {
        type = "object",
        properties = new
        {
            owner = new { type = "string" },
            repo = new { type = "string" }
        },
        required = new[] { "owner", "repo" }
    });
}
