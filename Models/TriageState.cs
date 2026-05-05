using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Models;

public sealed class TriageState
{
    public required GitHubIssueContext Issue { get; init; }

    public string? Classification { get; set; }
    public string? Summary { get; set; }
    public string? Severity { get; set; }

    public List<string> SuggestedLabels { get; set; } = [];
    public List<string> ReproductionSteps { get; set; } = [];
    public List<string> MissingInformation { get; set; } = [];

    public string? DeveloperTask { get; set; }
    public string? ResponseComment { get; set; }
    public string? Confidence { get; set; }
}
