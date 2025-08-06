using System;
using System.Net.Http;
using System.Text.Json;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
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
    
    public bool IsWindowsPlatform()
    {
        return OperatingSystem.IsWindows();
    }
    
    public string GetWindowsInstallerUrl(string version)
    {
        return $"https://github.com/AIDotNet/SemanticCode/releases/download/v{version}/SemanticCode-Setup-{version}-win-x64.exe";
    }
    
    public async Task<string?> DownloadUpdateAsync(string downloadUrl, IProgress<float>? progress = null)
    {
        try
        {
            var fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);
            
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var downloadedBytes = 0L;
            
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var buffer = new byte[8192];
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                downloadedBytes += bytesRead;
                
                if (totalBytes > 0 && progress != null)
                {
                    var progressPercentage = (float)downloadedBytes / totalBytes;
                    progress.Report(progressPercentage);
                }
            }
            
            return tempPath;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    public void StartUpdateInstaller(string installerPath)
    {
        if (!File.Exists(installerPath))
            return;
            
        try
        {
            // 启动安装程序
            Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true
            });
            
            // 关闭当前应用程序
            Environment.Exit(0);
        }
        catch (Exception)
        {
            // 忽略错误，让用户手动运行安装程序
        }
    }
    
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}