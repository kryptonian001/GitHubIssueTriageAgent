using Azure.Messaging.ServiceBus;
using GitHubIssueTriageAgent.Agents;
using GitHubIssueTriageAgent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octokit;

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


builder.Services.AddHostedService<GitHubIssueProcessorWorker>();


await builder.Build().RunAsync();


//var repoArg = args.FirstOrDefault(x => x.StartsWith("--repo="))?
//              .Replace("--repo=", "");

//var issueArg = args.FirstOrDefault(x => x.StartsWith("--issue="))?
//    .Replace("--issue=", "");

//if (string.IsNullOrWhiteSpace(repoArg) || string.IsNullOrWhiteSpace(issueArg))
//{
//    Console.WriteLine("Usage: dotnet run -- --repo=owner/repo --issue=42");
//    return;
//}

//var repoParts = repoArg.Split('/');

//if (repoParts.Length != 2)
//{
//    //dotnet run -- --repo=DeeProgrammer/8020-Nutrition-2020 --issue=116
//    Console.WriteLine("Repo must be in owner/repo format.");
//    return;
//}

//var owner = repoParts[0];
//var repo = repoParts[1];
//var issueNumber = int.Parse(issueArg);

//var github = new GitHubIssueService(githubToken);

//var agent = new ToolCallingTriageAgent(openAiKey, github);


//var report = await agent.RunAsync(owner, repo, issueNumber);

//Console.WriteLine(report);

