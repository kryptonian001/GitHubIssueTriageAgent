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

    public async Task<GitHubIssueContext> GetInssueAsync(string owner, string repo, int issueNumber)
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
            Body = issue.Body,
            Author = issue.User.Login,
            ExistingLabels = issue.Labels.Select(x => x.Name).ToList(),
            Comments = comments.Select(x => x.Body ?? "").ToList()
        };
    }
}
