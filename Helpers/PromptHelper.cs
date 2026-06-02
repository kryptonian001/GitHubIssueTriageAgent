using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Helpers;

internal static class PromptHelper
{
    public const string TriageAgentPrompt = @"
        # Issue Triage Report

        ## Triage Summary

        | Field | Value |
        |---|---|
        | Classification | invalid |
        | Severity | low |
        | Priority | low |
        | Developer Ready | no |
        | Recommended Route | support_triage |
        | Recommended Support Action | request_info |
        | Confidence | medium |

        ## Triage Decision

        The issue is not actionable as currently written. It appears to reference a Service Bus test, but it does not describe a specific problem, question, feature request, or expected outcome.

        ## Evidence

        - The issue references ""New SB test"".
        - The issue references ""Service bus"".
        - No clear problem statement was provided.
        - No expected or actual behavior was provided.
        - No reproduction steps were provided.

        ## Reproduction Steps

        NONE

        ## Missing Information

        - [ ] Clear description of the problem, question, or request
        - [ ] Expected behavior
        - [ ] Actual behavior, if reporting a bug
        - [ ] Steps to reproduce, if reporting a bug
        - [ ] Relevant Service Bus configuration, logs, or error messages

        ## Developer-Ready Task

        Not developer-ready. The issue lacks enough detail to create an actionable development task.

        ## Routing Recommendation

        Route to support triage to request clarification from the author.

        ## Suggested Labels

        - type: invalid
        - status: needs-info
        - severity: low

        ## Suggested Response Comment

        Thank you for opening this issue. We need a little more information before we can investigate. Could you clarify what you mean by ""New SB test"" and what you need regarding Service Bus? Please include the expected behavior, actual behavior if something is failing, and any relevant steps, logs, or configuration details.

        ## Recommended Support Action

        request_info

        ## Automation Payload

        Rules:
        - Use ONLY the allowed enum values listed below.
        - Do not create synonyms for enum values.
        - Do not use values like dev, investigate, medium priority, needs-investigation, or priority labels unless explicitly listed.
        - The Automation Payload must use the exact same values shown in the Triage Summary.
        - If the issue is developer-ready, Recommended Support Action should usually be route_to_engineering.
        - If the issue is not developer-ready, Recommended Support Action should usually be request_info.

        Allowed priority values:
        - low
        - normal
        - high
        - urgent

        Allowed route values:
        - support_triage
        - engineering
        - product
        - documentation
        - security
        - no_routing_needed

        Allowed support action values:
        - request_info
        - route_to_engineering
        - route_to_product
        - route_to_documentation
        - close_invalid
        - close_duplicate
        - escalate_security
        - answer_question
        - monitor

        Allowed status labels:
        - status: needs-info
        - status: developer-ready
        - status: triaged
        - status: blocked
        - status: stale

        ```json
        {
          ""classification"": ""invalid"",
          ""severity"": ""low"",
          ""priority"": ""low"",
          ""developerReady"": ""no"",
          ""recommendedRoute"": ""support_triage"",
          ""recommendedSupportAction"": ""request_info"",
          ""recommendedLabels"": [""type: invalid"", ""status: needs-info"", ""severity: low""],
          ""confidence"": ""medium""
        }
            ";

    public static string TriageChatMessagePrompt(string owner, string repo, int issueNumber)
    {
        return @$"
            Triage issue #{issueNumber} in {owner}/{repo}.

            Inspect the issue using available tools.

            Produce the required Markdown triage report using the configured layout.
            ";
    }
}
