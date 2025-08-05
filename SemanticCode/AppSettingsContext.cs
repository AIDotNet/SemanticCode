using System.Text.Json.Serialization;
using SemanticCode.Models;

namespace SemanticCode;

[JsonSerializable(typeof(ClaudeCodeSettings))]
internal partial class AppSettingsContext : JsonSerializerContext
{
}