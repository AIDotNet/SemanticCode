using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SemanticCode.Models;

public class ClaudeCodeProfile
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; } = false;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("settings")]
    public ClaudeCodeSettings Settings { get; set; } = new();
}

public class ClaudeCodeProfileInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; } = false;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ProfileManager
{
    [JsonPropertyName("currentProfile")]
    public string CurrentProfile { get; set; } = "default";
    
    [JsonPropertyName("profiles")]
    public List<ClaudeCodeProfileInfo> Profiles { get; set; } = new();
}