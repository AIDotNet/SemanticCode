using System.Text.Json;
using System.Text.Json.Serialization;
using SemanticCode.Models;
using SemanticCode.Services;

namespace SemanticCode;

[JsonSerializable(typeof(ClaudeCodeSettings))]
[JsonSerializable(typeof(ClaudeCodeProfile))]
[JsonSerializable(typeof(ClaudeCodeProfileInfo))]
[JsonSerializable(typeof(ProfileManager))]
[JsonSerializable(typeof(GitHubRelease))]
[JsonSerializable(typeof(AgentHubResponse))]
internal partial class AppSettingsContext : JsonSerializerContext
{
    public static JsonSerializerOptions DefaultOptions => Default.Options;
}