using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SemanticCode.Models;

namespace SemanticCode.Services;

public class ClaudeCodeSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(AppSettingsContext.Default.Options)
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string GetSettingsDirectory()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, ".claude");
    }

    private static string GetSettingsFilePath()
    {
        return Path.Combine(GetSettingsDirectory(), "settings.json");
    }

    public static async Task<ClaudeCodeSettings> LoadSettingsAsync()
    {
        try
        {
            var settingsPath = GetSettingsFilePath();

            if (!File.Exists(settingsPath))
            {
                var defaultSettings = CreateDefaultSettings();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(settingsPath);
            var settings = JsonSerializer.Deserialize<ClaudeCodeSettings>(json, JsonOptions);

            return settings ?? CreateDefaultSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
            return CreateDefaultSettings();
        }
    }

    public static async Task SaveSettingsAsync(ClaudeCodeSettings settings)
    {
        try
        {
            var settingsDir = GetSettingsDirectory();

            if (!Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }

            var settingsPath = GetSettingsFilePath();
            var json = JsonSerializer.Serialize(settings, JsonOptions);

            await File.WriteAllTextAsync(settingsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
            throw;
        }
    }

    public static ClaudeCodeSettings CreateDefaultSettings()
    {
        return new ClaudeCodeSettings
        {
            Env = new EnvironmentSettings
            {
                AnthropicBaseUrl = "https://api.anthropic.com",
                AnthropicModel = "claude-sonnet-4-20250514",
                AnthropicSmallFastModel = "claude-3-5-haiku-20241022",
                AnthropicMaxTokens = null,
                AnthropicTemperature = null,
                ClaudeCodeDebug = null,
                ClaudeCodeMaxContextTokens = null
            }
        };
    }

    public static ValidationResult ValidateSettings(ClaudeCodeSettings settings)
    {
        var result = new ValidationResult();

        if (settings?.Env == null)
        {
            result.AddError("配置对象为空");
            return result;
        }

        // 验证 API 密钥
        if (string.IsNullOrWhiteSpace(settings.Env.AnthropicAuthToken))
        {
            result.AddError("API 密钥不能为空");
        }
        else if (!settings.Env.AnthropicAuthToken.StartsWith("sk-"))
        {
            result.AddWarning("API 密钥格式可能不正确，通常以 'sk-' 开头");
        }

        // 验证模型名称
        if (string.IsNullOrWhiteSpace(settings.Env.AnthropicModel))
        {
            result.AddError("主要模型名称不能为空");
        }

        // 验证后台小模型名称
        if (string.IsNullOrWhiteSpace(settings.Env.AnthropicSmallFastModel))
        {
            result.AddWarning("建议设置后台快速模型以提升性能");
        }

        // 验证 token 数量
        if (settings.Env.AnthropicMaxTokens <= 0)
        {
            result.AddError("最大 token 数必须大于 0");
        }
        else if (settings.Env.AnthropicMaxTokens > 200000)
        {
            result.AddWarning("最大 token 数过大，可能导致 API 调用失败");
        }

        // 验证温度值
        if (settings.Env.AnthropicTemperature < 0 || settings.Env.AnthropicTemperature > 2)
        {
            result.AddError("温度值必须在 0.0 到 2.0 之间");
        }

        // 验证 URL 格式
        if (!string.IsNullOrWhiteSpace(settings.Env.AnthropicBaseUrl))
        {
            if (!IsValidUrl(settings.Env.AnthropicBaseUrl))
            {
                result.AddError("API 基础 URL 格式不正确");
            }
        }

        // 验证上下文 token 数量
        if (settings.Env.ClaudeCodeMaxContextTokens <= 0)
        {
            result.AddError("最大上下文 token 数必须大于 0");
        }

        // 验证路径
        if (!string.IsNullOrWhiteSpace(settings.Env.ClaudeCodeMemoryPath))
        {
            try
            {
                var directory = Path.GetDirectoryName(settings.Env.ClaudeCodeMemoryPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    result.AddWarning("记忆文件路径的目录不存在");
                }
            }
            catch
            {
                result.AddError("记忆文件路径格式不正确");
            }
        }

        return result;
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    public static List<string> GetAvailableModels()
    {
        return
        [
            "claude-sonnet-4-20250514",
            "claude-3-5-sonnet-20241022",
            "claude-3-5-haiku-20241022",
            "claude-3-opus-20240229",
            "claude-3-sonnet-20240229",
            "claude-3-haiku-20240307",
            "kimi-k2-0711-preview"
        ];
    }

    public static List<string> GetAvailableSmallFastModels()
    {
        return new List<string>
        {
            "claude-3-5-haiku-20241022",
            "claude-3-haiku-20240307"
        };
    }

}