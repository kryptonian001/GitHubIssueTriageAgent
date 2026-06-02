using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Configuration;

public class AgentConfig
{
    public string GitHubToken { get; set; }
    public string OpenAIApiKey { get; set; }
}
