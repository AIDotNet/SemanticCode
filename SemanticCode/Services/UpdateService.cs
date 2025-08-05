using System;
using System.Net.Http;
using System.Text.Json;
using System.Reflection;
using System.Threading.Tasks;
using SemanticCode.Models;

namespace SemanticCode.Services;

public class UpdateService
{
    private readonly HttpClient _httpClient;
    private const string GitHubApiUrl = "https://api.github.com/repos/AIDotNet/SemanticCode/releases/latest";
    
    public UpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SemanticCode-UpdateChecker");
    }
    
    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(GitHubApiUrl);
            var releaseData = JsonSerializer.Deserialize<JsonElement>(response);
            
            var latestVersion = releaseData.GetProperty("tag_name").GetString()?.TrimStart('v');
            var currentVersion = GetCurrentVersion();
            
            if (string.IsNullOrEmpty(latestVersion) || string.IsNullOrEmpty(currentVersion))
                return null;
                
            var updateInfo = new UpdateInfo
            {
                Version = latestVersion,
                ReleaseUrl = releaseData.GetProperty("html_url").GetString() ?? "",
                Description = releaseData.GetProperty("body").GetString() ?? "",
                PublishedAt = releaseData.GetProperty("published_at").GetDateTime(),
                IsNewerVersion = IsNewerVersion(currentVersion, latestVersion)
            };
            
            if (releaseData.TryGetProperty("assets", out var assetsElement))
            {
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    updateInfo.Assets.Add(new UpdateAsset
                    {
                        Name = asset.GetProperty("name").GetString() ?? "",
                        DownloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "",
                        Size = asset.GetProperty("size").GetInt64()
                    });
                }
            }
            
            return updateInfo;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    private string GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "0.0.0.0";
    }
    
    private bool IsNewerVersion(string currentVersion, string latestVersion)
    {
        if (Version.TryParse(currentVersion, out var current) && 
            Version.TryParse(latestVersion, out var latest))
        {
            return latest > current;
        }
        
        return false;
    }
    
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}