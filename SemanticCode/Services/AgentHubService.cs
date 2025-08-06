using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SemanticCode.Models;

namespace SemanticCode.Services;

public class AgentHubService
{
    private const string HubApiUrl =
        "https://raw.githubusercontent.com/AIDotNet/SemanticCode-Hubs/refs/heads/main/agents.json";

    private readonly HttpClient _httpClient;
    private readonly string _agentsDirectory;

    public AgentHubService()
    {
        _httpClient = new HttpClient();
        _agentsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude",
            "agents");
    }

    public async Task<AgentHubResponse?> FetchAgentHubDataAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(HubApiUrl);
            var hubData =
                JsonSerializer.Deserialize<AgentHubResponse>(response, AppSettingsContext.Default.Options);

            // Check which agents are already installed
            if (hubData?.Agents != null)
            {
                foreach (var agent in hubData.Agents)
                {
                    agent.IsInstalled = IsAgentInstalled(agent.Id);
                }
            }

            return hubData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching agent hub data: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DownloadAndInstallAgentAsync(AgentHubItem agent)
    {
        try
        {
            // Ensure agents directory exists
            if (!Directory.Exists(_agentsDirectory))
            {
                Directory.CreateDirectory(_agentsDirectory);
            }

            // Download agent content from promptUrl
            var agentContent = await _httpClient.GetStringAsync(agent.PromptUrl);

            agent.Id += ".md";

            // Save to agents directory
            var filePath = Path.Combine(_agentsDirectory, agent.Id);
            await File.WriteAllTextAsync(filePath, agentContent);

            // Mark as installed
            agent.IsInstalled = true;

            // Notify that an agent was installed
            AgentNotificationService.Instance.NotifyAgentInstalled();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading agent {agent.Name}: {ex.Message}");
            return false;
        }
    }

    private bool IsAgentInstalled(string agentId)
    {
        agentId += ".md";
        var filePath = Path.Combine(_agentsDirectory, agentId);
        return File.Exists(filePath);
    }

    public string GetAgentsDirectory()
    {
        return _agentsDirectory;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}