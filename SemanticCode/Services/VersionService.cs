using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SemanticCode.Services;

public class VersionService
{
    private readonly HttpClient _httpClient;
    private const string GITHUB_API_URL = "https://api.github.com/repos/AIDotNet/SemanticCode/releases/latest";

    public VersionService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SemanticCode-App");
    }

    public async Task<VersionInfo> CheckForUpdatesAsync()
    {
        try
        {
            var currentVersion = GetCurrentVersion();
            var latestVersion = await GetLatestVersionFromGitHubAsync();
            
            return new VersionInfo
            {
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                HasUpdate = IsNewerVersion(latestVersion, currentVersion),
                IsCheckSuccessful = true
            };
        }
        catch (Exception ex)
        {
            // 记录异常但不抛出，防止程序崩溃
            System.Diagnostics.Debug.WriteLine($"版本检查失败: {ex.Message}");
            
            return new VersionInfo
            {
                CurrentVersion = GetCurrentVersion(),
                LatestVersion = "未知",
                HasUpdate = false,
                IsCheckSuccessful = false,
                ErrorMessage = "无法连接到服务器检查更新"
            };
        }
    }

    private string GetCurrentVersion()
    {
        return typeof(VersionService).Assembly.GetName().Version?.ToString() ?? "1.0.0.0";
    }

    private async Task<string> GetLatestVersionFromGitHubAsync()
    {
        using var response = await _httpClient.GetAsync(GITHUB_API_URL);
        response.EnsureSuccessStatusCode();
        
        var jsonContent = await response.Content.ReadAsStringAsync();
        var releaseInfo = JsonSerializer.Deserialize<GitHubRelease>(jsonContent,AppSettingsContext.Default.Options);
        
        return releaseInfo?.TagName?.TrimStart('v') ?? "0.0.0.0";
    }

    private bool IsNewerVersion(string latestVersion, string currentVersion)
    {
        try
        {
            var latest = new Version(latestVersion);
            var current = new Version(currentVersion);
            return latest > current;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public class VersionInfo
{
    public string CurrentVersion { get; set; } = "";
    public string LatestVersion { get; set; } = "";
    public bool HasUpdate { get; set; }
    public bool IsCheckSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = "";
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = "";
}