using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SemanticCode.Models;

public class McpConfiguration
{
    [JsonPropertyName("mcpServers")]
    public Dictionary<string, McpServer> McpServers { get; set; } = new();
}

public class McpServer
{
    [JsonPropertyName("command")]
    public string? Command { get; set; }

    [JsonPropertyName("args")]
    public List<string>? Args { get; set; }

    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; set; }

    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }

    [JsonPropertyName("alwaysAllow")]
    public List<string>? AlwaysAllow { get; set; }
}

public class McpServerStatus
{
    public string Name { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public bool IsEnabled { get; set; }
    public string? ErrorMessage { get; set; }
    public McpServer Configuration { get; set; } = new();
}

public class McpValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public void AddError(string error)
    {
        IsValid = false;
        Errors.Add(error);
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}