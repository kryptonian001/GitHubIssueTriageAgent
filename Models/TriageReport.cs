using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Models;

public sealed class TriageReport
{
    public required string Classification { get; init; }
    public required string Summary { get; init; }
    public required string Severity { get; init; }
    public List<string> SuggestedLabels { get; init; } = [];
    public List<string> ReproductionSteps { get; init; } = [];
    public List<string> MissingInformation { get; init; } = [];
    public required string DeveloperReadyTask { get; init; }
    public required string SuggestedResponseComment { get; init; }
    public required string Confidence { get; init; }
}
