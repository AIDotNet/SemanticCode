using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SemanticCode.Models;

public class ClaudeCodeSettings
{
    [JsonPropertyName("env")]
    public EnvironmentSettings Env { get; set; } = new();
    
    [JsonPropertyName("editor")]
    public EditorSettings? Editor { get; set; }
    
    [JsonPropertyName("tools")]
    public ToolsSettings? Tools { get; set; }
    
    [JsonPropertyName("memory")]
    public MemorySettings? Memory { get; set; }
    
    [JsonPropertyName("ui")]
    public UISettings? UI { get; set; }
}

public class EnvironmentSettings
{
    [JsonPropertyName("ANTHROPIC_AUTH_TOKEN")]
    public string? AnthropicAuthToken { get; set; }
    
    [JsonPropertyName("ANTHROPIC_BASE_URL")]
    public string? AnthropicBaseUrl { get; set; }
    
    [JsonPropertyName("ANTHROPIC_MODEL")]
    public string? AnthropicModel { get; set; }
    
    [JsonPropertyName("ANTHROPIC_SMALL_FAST_MODEL")]
    public string? AnthropicSmallFastModel { get; set; }
    
    [JsonPropertyName("ANTHROPIC_MAX_TOKENS")]
    public int? AnthropicMaxTokens { get; set; }
    
    [JsonPropertyName("ANTHROPIC_TEMPERATURE")]
    public double? AnthropicTemperature { get; set; }
    
    [JsonPropertyName("CLAUDE_CODE_DEBUG")]
    public bool? ClaudeCodeDebug { get; set; }
    
    [JsonPropertyName("CLAUDE_CODE_MAX_CONTEXT_TOKENS")]
    public int? ClaudeCodeMaxContextTokens { get; set; }
    
    [JsonPropertyName("CLAUDE_CODE_MEMORY_PATH")]
    public string? ClaudeCodeMemoryPath { get; set; }
    
    [JsonPropertyName("CLAUDE_CODE_TOOLS_DISABLED")]
    public string? ClaudeCodeToolsDisabled { get; set; }
}

public class EditorSettings
{
    [JsonPropertyName("auto_save")]
    public bool? AutoSave { get; set; }
    
    [JsonPropertyName("word_wrap")]
    public bool? WordWrap { get; set; }
    
    [JsonPropertyName("font_size")]
    public int? FontSize { get; set; }
    
    [JsonPropertyName("theme")]
    public string? Theme { get; set; }
}

public class ToolsSettings
{
    [JsonPropertyName("enabled")]
    public List<string>? Enabled { get; set; }
    
    [JsonPropertyName("disabled")]
    public List<string>? Disabled { get; set; }
    
    [JsonPropertyName("bash_timeout")]
    public int? BashTimeout { get; set; }
    
    [JsonPropertyName("max_file_size")]
    public int? MaxFileSize { get; set; }
}

public class MemorySettings
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    
    [JsonPropertyName("max_entries")]
    public int? MaxEntries { get; set; }
    
    [JsonPropertyName("auto_cleanup")]
    public bool? AutoCleanup { get; set; }
}

public class UISettings
{
    [JsonPropertyName("language")]
    public string? Language { get; set; }
    
    [JsonPropertyName("theme")]
    public string? Theme { get; set; }
    
    [JsonPropertyName("auto_start")]
    public bool? AutoStart { get; set; }
    
    [JsonPropertyName("notifications")]
    public bool? Notifications { get; set; }
}