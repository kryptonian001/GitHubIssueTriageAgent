using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Services;

public sealed class GitHubIssueProcessorWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IConfiguration _config;

    private ServiceBusProcessor? _processor;

    public GitHubIssueProcessorWorker(ServiceBusClient client, IConfiguration config)
    {
        _client = client;
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
