using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SemanticCode.Models;

namespace SemanticCode.Services;

public class AgentHubService
{
    private const string HubApiUrl =
        "https://raw.githubusercontent.com/AIDotNet/SemanticCode-Hubs/refs/heads/main/agents.json";
    
    private const int CacheExpirationMinutes = 30; // 缓存30分钟
    
    private readonly HttpClient _httpClient;
    private readonly string _agentsDirectory;
    private readonly string _cacheFilePath;
    
    private static AgentHubResponse? _cachedData;
    private static DateTime _cacheTimestamp = DateTime.MinValue;
    private static readonly object _cacheLock = new object();

    public AgentHubService()
    {
        _httpClient = new HttpClient();
        _agentsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude",
            "agents");
        _cacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude",
            "agent_hub_cache.json");
    }

    public async Task<AgentHubResponse?> FetchAgentHubDataAsync(bool forceRefresh = false)
    {
        lock (_cacheLock)
        {
            // 检查内存缓存是否有效
            if (!forceRefresh && _cachedData != null && IsCacheValid(_cacheTimestamp))
            {
                // 更新安装状态并返回缓存数据
                UpdateInstallationStatus(_cachedData);
                return _cachedData;
            }
        }

        try
        {
            // 如果网络请求失败，尝试从磁盘缓存加载
            AgentHubResponse? hubData = null;
            
            try
            {
                var response = await _httpClient.GetStringAsync(HubApiUrl);
                hubData = JsonSerializer.Deserialize<AgentHubResponse>(response, AppSettingsContext.Default.Options);
            }
            catch (Exception netEx)
            {
                Console.WriteLine($"Network error, trying disk cache: {netEx.Message}");
                hubData = await LoadFromDiskCacheAsync();
            }

            if (hubData != null)
            {
                // 更新安装状态
                UpdateInstallationStatus(hubData);
                
                // 更新缓存
                lock (_cacheLock)
                {
                    _cachedData = hubData;
                    _cacheTimestamp = DateTime.Now;
                }
                
                // 保存到磁盘缓存
                await SaveToDiskCacheAsync(hubData);
            }

            return hubData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching agent hub data: {ex.Message}");
            
            // 尝试返回磁盘缓存
            var diskCache = await LoadFromDiskCacheAsync();
            if (diskCache != null)
            {
                UpdateInstallationStatus(diskCache);
            }
            return diskCache;
        }
    }

    private bool IsCacheValid(DateTime cacheTime)
    {
        return DateTime.Now - cacheTime < TimeSpan.FromMinutes(CacheExpirationMinutes);
    }

    private void UpdateInstallationStatus(AgentHubResponse? hubData)
    {
        if (hubData?.Agents != null)
        {
            foreach (var agent in hubData.Agents)
            {
                agent.IsInstalled = IsAgentInstalled(agent.Id);
            }
        }
    }

    private async Task SaveToDiskCacheAsync(AgentHubResponse hubData)
    {
        try
        {
            // 确保目录存在
            var cacheDir = Path.GetDirectoryName(_cacheFilePath);
            if (!string.IsNullOrEmpty(cacheDir) && !Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            var cacheData = new
            {
                Timestamp = DateTime.Now,
                Data = hubData
            };

            var json = JsonSerializer.Serialize(cacheData, AppSettingsContext.Default.Options);
            await File.WriteAllTextAsync(_cacheFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving cache to disk: {ex.Message}");
        }
    }

    private async Task<AgentHubResponse?> LoadFromDiskCacheAsync()
    {
        try
        {
            if (!File.Exists(_cacheFilePath))
                return null;

            var json = await File.ReadAllTextAsync(_cacheFilePath);
            var cacheData = JsonSerializer.Deserialize<JsonElement>(json);

            if (cacheData.TryGetProperty("Timestamp", out var timestampElement) &&
                cacheData.TryGetProperty("Data", out var dataElement))
            {
                var timestamp = timestampElement.GetDateTime();
                
                // 检查磁盘缓存是否过期（更长的过期时间，比如24小时）
                if (DateTime.Now - timestamp < TimeSpan.FromHours(24))
                {
                    return JsonSerializer.Deserialize<AgentHubResponse>(dataElement.GetRawText(), AppSettingsContext.Default.Options);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cache from disk: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DownloadAndInstallAgentAsync(AgentHubItem agent)
    {
        try
        {
            // Ensure agents directory exists
            if (!Directory.Exists(_agentsDirectory))
            {
                Directory.CreateDirectory(_agentsDirectory);
            }

            // Download agent content from promptUrl
            var agentContent = await _httpClient.GetStringAsync(agent.PromptUrl);

            agent.Id += ".md";

            // Save to agents directory
            var filePath = Path.Combine(_agentsDirectory, agent.Id);
            await File.WriteAllTextAsync(filePath, agentContent);

            // Mark as installed
            agent.IsInstalled = true;

            // Notify that an agent was installed
            AgentNotificationService.Instance.NotifyAgentInstalled();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading agent {agent.Name}: {ex.Message}");
            return false;
        }
    }

    private bool IsAgentInstalled(string agentId)
    {
        agentId += ".md";
        var filePath = Path.Combine(_agentsDirectory, agentId);
        return File.Exists(filePath);
    }

    public string GetAgentsDirectory()
    {
        return _agentsDirectory;
    }

    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _cachedData = null;
            _cacheTimestamp = DateTime.MinValue;
        }

        try
        {
            if (File.Exists(_cacheFilePath))
            {
                File.Delete(_cacheFilePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing disk cache: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}