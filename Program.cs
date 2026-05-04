using GitHubIssueTriageAgent.Agents;
using GitHubIssueTriageAgent.Services;
using Microsoft.Extensions.Configuration;
using Octokit;


var config = new ConfigurationBuilder()
      .AddUserSecrets<Program>()
      .AddEnvironmentVariables()
      .Build();

var githubToken = config["GitHub:Token"]
    ?? throw new InvalidOperationException("Missing GitHub token.");

var openAiKey = config["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("Missing OpenAI API key.");

var repoArg = args.FirstOrDefault(x => x.StartsWith("--repo="))?
    .Replace("--repo=", "");

var issueArg = args.FirstOrDefault(x => x.StartsWith("--issue="))?
    .Replace("--issue=", "");

if (string.IsNullOrWhiteSpace(repoArg) || string.IsNullOrWhiteSpace(issueArg))
{
    Console.WriteLine("Usage: dotnet run -- --repo=owner/repo --issue=42");
    return;
}

var repoParts = repoArg.Split('/');

if (repoParts.Length != 2)
{
    Console.WriteLine("Repo must be in owner/repo format.");
    return;
}

var owner = repoParts[0];
var repo = repoParts[1];
var issueNumber = int.Parse(issueArg);

var github = new GitHubIssueService(githubToken);
var agent = new IssueTriageAgent(openAiKey);

var issue = await github.GetInssueAsync(owner, repo, issueNumber);
var report = await agent.TriageAsync(issue);

Console.WriteLine($$"""
    # GitHub Issue Triage Report

    ## Classification
    {report.Classification}

    ## Summary
    {report.Summary}

    ## Severity
    {report.Severity}

    ## Suggested Labels
    {string.Join(", ", report.SuggestedLabels)}

    ## Reproduction Steps
    {string.Join(Environment.NewLine, report.ReproductionSteps.Select((x, i) => $"{i + 1}. {x}"))}

    ## Missing Information
    {string.Join(Environment.NewLine, report.MissingInformation.Select(x => $"- {x}"))}

    ## Developer-Ready Task
    {report.DeveloperReadyTask}

    ## Suggested Response Comment
    {report.SuggestedResponseComment}

    ## Confidence
    {report.Confidence}
    """);

