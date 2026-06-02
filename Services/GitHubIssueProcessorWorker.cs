using Azure.Messaging.ServiceBus;
using GitHubIssueTriageAgent.Agents;
using GithubIssueTriageShared.Models.Github;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Services;

public sealed class GitHubIssueProcessorWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ToolCallingTriageAgent _agent;
    private readonly IConfiguration _config;

    private ServiceBusProcessor? _processor;

    public GitHubIssueProcessorWorker(ServiceBusClient client, ToolCallingTriageAgent agent, IConfiguration config)
    {
        _client = client;
        _agent = agent;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = _config["ServiceBus:QueueName"]
            ?? throw new InvalidOperationException("Missing ServiceBus queue name.");

        _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
            PrefetchCount = 0
        });

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        await _processor.StartProcessingAsync();
    }

    private async Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        Console.WriteLine($"Error retrieving message: {arg.Exception.Message}");
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        Console.WriteLine($"Message recieved: {args.Message.Body}");

        var githubIssue = JsonConvert.DeserializeObject<GithubIssue>(args.Message.Body.ToString());

        if (githubIssue == null)
        {
            Console.WriteLine("GitHub issue was not found");
            return;
        }

        var repo = githubIssue.repository;
        var issue = githubIssue.issue;

        var report = await _agent.RunAsync(repo.owner.login, repo.name, issue.number);

        Console.WriteLine($"Triage report: {report}");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
