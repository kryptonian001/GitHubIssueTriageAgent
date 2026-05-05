using GitHubIssueTriageAgent.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubIssueTriageAgent.Agents;

public sealed class CoordinatorAgent
{
    private readonly ClassificationAgent _classification;
    private readonly ReproductionAgent _repro;
    private readonly LabelAgent _labels;
    private readonly SeverityAgent _severity;
    private readonly ResponseAgent _response;

    public CoordinatorAgent(string key)
    {
        _classification = new ClassificationAgent(key);
        _repro = new ReproductionAgent(key);
        _labels = new LabelAgent(key);
        _severity = new SeverityAgent(key);
        _response = new ResponseAgent(key);
    }

    public async Task<TriageState> Run(GitHubIssueContext issue)
    {
        var state = new TriageState { Issue = issue };

        await _classification.Run(state);
        await _repro.Run(state);
        await _labels.Run(state);
        await _severity.Run(state);
        await _response.Run(state);

        state.Confidence = state.MissingInformation.Count > 2 ? "low" : "high";

        return state;
    }
}
