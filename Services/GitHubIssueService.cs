using GitHubIssueTriageAgent.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Services;

public sealed class GitHubIssueService
{
    private readonly GitHubClient _client;
    public GitHubIssueService(string token)
    {
        _client = new GitHubClient(new ProductHeaderValue("GitHubIssueTriageAgent"))
        {
            Credentials = new Credentials(token)
        };       
    }

    public async Task<GitHubIssueContext> GetIssueAsync(string owner, string repo, int issueNumber)
    {
        var issue = await _client.Issue.Get(owner, repo, issueNumber);

        if (issue.PullRequest is not null)
            throw new InvalidCastException("This item is a pull request, not a standard issue");

        var comments = await _client.Issue.Comment.GetAllForIssue(owner, repo, issueNumber);

        return new GitHubIssueContext
        {
            RepositoryOwner = owner,
            RepositoryName = repo,
            IssueNumber = issueNumber,
            Title = issue.Title,
            Body = issue.Body,
            Author = issue.User.Login,
            ExistingLabels = issue.Labels.Select(x => x.Name).ToList(),
            Comments = comments.Select(x => x.Body ?? "").ToList()
        };
    }

    public async Task<string[]> GetRepositoryLabelsAsync(string owner, string repo)
    {
        var labels = await _client.Issue.Labels.GetAllForRepository(owner, repo);

        return labels
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToArray();
    }
}
