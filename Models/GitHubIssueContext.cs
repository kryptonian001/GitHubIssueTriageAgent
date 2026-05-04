using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Models;

public sealed class GitHubIssueContext
{
    public required string RepositoryOwner { get; init; }
    public required string RepositoryName { get; init; }
    public required int IssueNumber { get; init; }

    public required string Title { get; init; }
    public required string Body { get; init; }
    public required string Author { get; init; }

    public List<string> ExistingLabels { get; init; } = [];
    public List<string> Comments { get; init; } = [];
}
