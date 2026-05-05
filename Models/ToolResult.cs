using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Models;

public sealed class ToolResult
{
    public required string ToolName { get; init; }
    public required string Json { get; init; }
}
