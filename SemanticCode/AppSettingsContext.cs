using System.Text.Json.Serialization;
using SemanticCode.Models;
using SemanticCode.Services;

namespace SemanticCode;

[JsonSerializable(typeof(ClaudeCodeSettings))]
[JsonSerializable(typeof(GitHubRelease))]
internal partial class AppSettingsContext : JsonSerializerContext
{
}