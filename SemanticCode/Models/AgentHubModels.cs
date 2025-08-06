using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SemanticCode.Models;

public class AgentHubResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
    
    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;
    
    [JsonPropertyName("agents")]
    public List<AgentHubItem> Agents { get; set; } = new();
}

public class AgentHubItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
    
    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;
    
    [JsonPropertyName("promptUrl")]
    public string PromptUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("tools")]
    public List<string> Tools { get; set; } = new();
    
    // Helper property to check if agent is already installed
    public bool IsInstalled { get; set; }
}