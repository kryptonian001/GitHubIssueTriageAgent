using GitHubIssueTriageAgent.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Services;

public static class ReportBuilder
{
    public static string Build(TriageState s)
    {
        return $"""
        # GitHub Issue Triage Report

        ## Classification
        {s.Classification}

        ## Severity
        {s.Severity}

        ## Suggested Labels
        {string.Join(", ", s.SuggestedLabels)}

        ## Reproduction Steps
        {string.Join("\n", s.ReproductionSteps)}

        ## Missing Information
        {string.Join("\n", s.MissingInformation)}

        ## Suggested Response
        {s.ResponseComment}

        ## Confidence
        {s.Confidence}
        """;
    }
}
