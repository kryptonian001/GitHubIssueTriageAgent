using Azure.Messaging.ServiceBus;
using GitHubIssueTriageAgent.Agents;
using GitHubIssueTriageAgent.Helpers;
using GitHubIssueTriageAgent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octokit;

public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddUserSecrets<Program>()
              .AddEnvironmentVariables()
              .Build();

        builder.Services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var connectionString = config["ServiceBus:ConnectionString"]
        ?? throw new InvalidOperationException("Missing ServiceBus connection string.");

            return new ServiceBusClient(connectionString);
        });

        var githubToken = builder.Configuration["GitHub:Token"]
            ?? throw new InvalidOperationException("Missing GitHub token.");

        var openAiKey = builder.Configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("Missing OpenAI API key.");


        builder.Services.AddScoped((sp) => new GitHubIssueService(githubToken));

        builder.Services.AddSingleton((sp) =>
        {
            var githubService = sp.GetRequiredService<GitHubIssueService>();
            return new ToolCallingTriageAgent(openAiKey, githubService);
        });


        builder.Services.AddHostedService<GitHubIssueProcessorWorker>();

        if (args.TryGetArgument("-d", out string value))
        {
            if (int.TryParse(value, out int number))
            {
                Console.WriteLine($"Pausing {number}s to allow servicebus to complete loading");
                await Task.Delay(TimeSpan.FromSeconds(number));
            }
        }

        await builder.Build().RunAsync();
    }
}

