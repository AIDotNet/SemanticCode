using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SemanticCode.Models;

namespace SemanticCode.Services;

public class McpService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string GetClaudeDirectory()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, ".claude");
    }

    private static string GetMcpConfigFilePath()
    {
        return Path.Combine(GetClaudeDirectory(), "claude_desktop_config.json");
    }

    public static async Task<McpConfiguration> LoadMcpConfigurationAsync()
    {
        try
        {
            var configPath = GetMcpConfigFilePath();

            if (!File.Exists(configPath))
            {
                var defaultConfig = new McpConfiguration();
                await SaveMcpConfigurationAsync(defaultConfig);
                return defaultConfig;
            }

            var json = await File.ReadAllTextAsync(configPath);
            var config = JsonSerializer.Deserialize<McpConfiguration>(json, JsonOptions);

            return config ?? new McpConfiguration();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading MCP configuration: {ex.Message}");
            return new McpConfiguration();
        }
    }

    public static async Task SaveMcpConfigurationAsync(McpConfiguration config)
    {
        try
        {
            var claudeDir = GetClaudeDirectory();

            if (!Directory.Exists(claudeDir))
            {
                Directory.CreateDirectory(claudeDir);
            }

            var configPath = GetMcpConfigFilePath();
            var json = JsonSerializer.Serialize(config, JsonOptions);

            await File.WriteAllTextAsync(configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving MCP configuration: {ex.Message}");
            throw;
        }
    }

    public static async Task<List<McpServerStatus>> GetMcpServerStatusesAsync()
    {
        var statuses = new List<McpServerStatus>();

        // 首先尝试从Claude CLI获取MCP服务器列表
        if (await IsClaudeCliAvailableAsync())
        {
            try
            {
                var claudeList = await GetClaudeMcpListAsync();
                var parsedServers = ParseClaudeMcpList(claudeList);
                
                foreach (var server in parsedServers)
                {
                    var status = new McpServerStatus
                    {
                        Name = server.Key,
                        Configuration = server.Value,
                        IsEnabled = server.Value.Disabled != true,
                        IsRunning = false
                    };

                    try
                    {
                        status.IsRunning = await CheckMcpServerStatusAsync(server.Value);
                    }
                    catch (Exception ex)
                    {
                        status.ErrorMessage = ex.Message;
                    }

                    statuses.Add(status);
                }
                
                return statuses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get MCP servers from Claude CLI: {ex.Message}");
            }
        }

        // 备用方案：从本地配置文件加载
        var config = await LoadMcpConfigurationAsync();
        
        foreach (var kvp in config.McpServers)
        {
            var status = new McpServerStatus
            {
                Name = kvp.Key,
                Configuration = kvp.Value,
                IsEnabled = kvp.Value.Disabled != true,
                IsRunning = false
            };

            try
            {
                status.IsRunning = await CheckMcpServerStatusAsync(kvp.Value);
            }
            catch (Exception ex)
            {
                status.ErrorMessage = ex.Message;
            }

            statuses.Add(status);
        }

        return statuses;
    }

    private static async Task<bool> CheckMcpServerStatusAsync(McpServer server)
    {
        if (string.IsNullOrEmpty(server.Command))
            return false;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = server.Command,
                Arguments = string.Join(" ", server.Args ?? []),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(server.Command) ?? Environment.CurrentDirectory
            };

            if (server.Env != null)
            {
                foreach (var env in server.Env)
                {
                    startInfo.EnvironmentVariables[env.Key] = env.Value;
                }
            }

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var timeout = TimeSpan.FromSeconds(5);
            var completed = await Task.Run(() => process.WaitForExit((int)timeout.TotalMilliseconds));
            
            if (!completed)
            {
                process.Kill();
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static async Task AddMcpServerAsync(string name, McpServer server)
    {
        var config = await LoadMcpConfigurationAsync();
        config.McpServers[name] = server;
        await SaveMcpConfigurationAsync(config);
        
        // 同步到Claude CLI
        await SyncMcpToClaudeAsync("add", name, server);
    }

    public static async Task AddMcpServersAsync(Dictionary<string, McpServer> servers)
    {
        var config = await LoadMcpConfigurationAsync();
        
        // 添加所有服务器到本地配置
        foreach (var kvp in servers)
        {
            config.McpServers[kvp.Key] = kvp.Value;
        }
        
        await SaveMcpConfigurationAsync(config);
        
        // 逐个同步到Claude CLI
        foreach (var kvp in servers)
        {
            await SyncMcpToClaudeAsync("add", kvp.Key, kvp.Value);
        }
    }

    public static async Task RemoveMcpServerAsync(string name)
    {
        var config = await LoadMcpConfigurationAsync();
        config.McpServers.Remove(name);
        await SaveMcpConfigurationAsync(config);
        
        // 从Claude CLI中删除
        await SyncMcpToClaudeAsync("remove", name, null);
    }

    public static async Task UpdateMcpServerAsync(string name, McpServer server)
    {
        var config = await LoadMcpConfigurationAsync();
        if (config.McpServers.ContainsKey(name))
        {
            config.McpServers[name] = server;
            await SaveMcpConfigurationAsync(config);
        }
    }

    public static async Task EnableMcpServerAsync(string name, bool enabled)
    {
        var config = await LoadMcpConfigurationAsync();
        if (config.McpServers.TryGetValue(name, out var server))
        {
            server.Disabled = !enabled;
            await SaveMcpConfigurationAsync(config);
        }
    }

    public static McpValidationResult ValidateMcpServer(string name, McpServer server)
    {
        var result = new McpValidationResult();

        if (string.IsNullOrWhiteSpace(name))
        {
            result.AddError("MCP服务器名称不能为空");
        }

        if (string.IsNullOrWhiteSpace(server.Command))
        {
            result.AddError("命令不能为空");
        }
        else if (!File.Exists(server.Command) && !IsCommandInPath(server.Command))
        {
            result.AddWarning($"命令文件不存在或不在PATH中: {server.Command}");
        }

        if (server.Args != null && server.Args.Any(string.IsNullOrWhiteSpace))
        {
            result.AddWarning("参数列表包含空值");
        }

        return result;
    }

    private static bool IsCommandInPath(string command)
    {
        try
        {
            var pathValues = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
            
            foreach (var path in pathValues)
            {
                var fullPath = Path.Combine(path, command);
                if (File.Exists(fullPath) || File.Exists(fullPath + ".exe"))
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static List<McpServerTemplate> GetMcpServerTemplates()
    {
        return new List<McpServerTemplate>
        {
            new()
            {
                Name = "GitHub MCP Server",
                Description = "与GitHub仓库交互的MCP服务器",
                Server = new McpServer
                {
                    Command = "npx",
                    Args = ["-y", "@modelcontextprotocol/server-github"],
                    Env = new Dictionary<string, string>
                    {
                        ["GITHUB_PERSONAL_ACCESS_TOKEN"] = ""
                    }
                }
            },
            new()
            {
                Name = "Filesystem MCP Server", 
                Description = "文件系统操作MCP服务器",
                Server = new McpServer
                {
                    Command = "npx",
                    Args = ["-y", "@modelcontextprotocol/server-filesystem", "/path/to/allowed/files"]
                }
            },
            new()
            {
                Name = "SQLite MCP Server",
                Description = "SQLite数据库操作MCP服务器", 
                Server = new McpServer
                {
                    Command = "npx",
                    Args = ["-y", "@modelcontextprotocol/server-sqlite", "/path/to/database.db"]
                }
            },
            new()
            {
                Name = "PostgreSQL MCP Server",
                Description = "PostgreSQL数据库操作MCP服务器",
                Server = new McpServer
                {
                    Command = "npx",
                    Args = ["-y", "@modelcontextprotocol/server-postgres"],
                    Env = new Dictionary<string, string>
                    {
                        ["POSTGRES_CONNECTION_STRING"] = ""
                    }
                }
            },
            new()
            {
                Name = "Fetch MCP Server",
                Description = "HTTP请求MCP服务器",
                Server = new McpServer
                {
                    Command = "npx",
                    Args = ["-y", "@modelcontextprotocol/server-fetch"]
                }
            },
            new()
            {
                Name = "Brave Search MCP Server",
                Description = "Brave搜索引擎MCP服务器",
                Server = new McpServer
                {
                    Command = "npx",
                    Args = ["-y", "@modelcontextprotocol/server-brave-search"],
                    Env = new Dictionary<string, string>
                    {
                        ["BRAVE_API_KEY"] = ""
                    }
                }
            }
        };
    }

    private static async Task SyncMcpToClaudeAsync(string action, string name, McpServer? server)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            if (action == "add" && server != null)
            {
                // 使用 claude mcp add-json 命令
                processStartInfo.FileName = "claude";
                
                // 序列化单个服务器配置为JSON
                var jsonConfig = JsonSerializer.Serialize(server, JsonOptions);
                
                // 使用正确的命令格式: claude mcp add-json <name> <json>
                processStartInfo.Arguments = $"mcp add-json \"{name}\" '{jsonConfig}'";
            }
            else if (action == "remove")
            {
                // 使用 claude mcp remove 命令
                processStartInfo.FileName = "claude";
                processStartInfo.Arguments = $"mcp remove {name}";
            }
            else if (action == "list")
            {
                // 使用 claude mcp list 命令
                processStartInfo.FileName = "claude";
                processStartInfo.Arguments = "mcp list";
            }
            else
            {
                return; // 不支持的操作
            }

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Claude MCP {action} failed: {error}");
                throw new InvalidOperationException($"Claude MCP {action} failed: {error}");
            }
            else
            {
                Console.WriteLine($"Claude MCP {action} successful: {output}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error syncing to Claude CLI: {ex.Message}");
            // 不抛出异常，让本地配置继续工作
        }
    }

    public static async Task<string> GetClaudeMcpListAsync()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "claude",
                Arguments = "mcp list",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                return output;
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException($"Claude MCP list failed: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Claude MCP list: {ex.Message}");
            return string.Empty;
        }
    }

    public static async Task<bool> IsClaudeCliAvailableAsync()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "claude",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static async Task SyncAllMcpServersToClaudeAsync()
    {
        try
        {
            var config = await LoadMcpConfigurationAsync();
            
            foreach (var kvp in config.McpServers)
            {
                await SyncMcpToClaudeAsync("add", kvp.Key, kvp.Value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error syncing all MCP servers to Claude: {ex.Message}");
        }
    }

    private static Dictionary<string, McpServer> ParseClaudeMcpList(string claudeListOutput)
    {
        var servers = new Dictionary<string, McpServer>();

        if (string.IsNullOrWhiteSpace(claudeListOutput))
            return servers;

        try
        {
            // 尝试解析JSON格式的输出
            if (claudeListOutput.TrimStart().StartsWith("{") || claudeListOutput.TrimStart().StartsWith("["))
            {
                var config = JsonSerializer.Deserialize<McpConfiguration>(claudeListOutput, JsonOptions);
                if (config?.McpServers != null)
                {
                    return config.McpServers;
                }
            }

            // 如果不是JSON，尝试解析文本格式
            var lines = claudeListOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            McpServer? currentServer = null;
            string? currentServerName = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // 检测服务器名称（通常是不缩进的行）
                if (!line.StartsWith(" ") && !line.StartsWith("\t") && trimmedLine.Contains(":"))
                {
                    // 保存前一个服务器
                    if (currentServer != null && !string.IsNullOrEmpty(currentServerName))
                    {
                        servers[currentServerName] = currentServer;
                    }

                    // 开始新的服务器
                    currentServerName = trimmedLine.Split(':')[0].Trim();
                    currentServer = new McpServer();
                }
                else if (currentServer != null && trimmedLine.Contains(":"))
                {
                    // 解析服务器属性
                    var parts = trimmedLine.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim().ToLower();
                        var value = parts[1].Trim();

                        switch (key)
                        {
                            case "command":
                                currentServer.Command = value;
                                break;
                            case "args":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    currentServer.Args = value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                                }
                                break;
                            case "disabled":
                                if (bool.TryParse(value, out var disabled))
                                {
                                    currentServer.Disabled = disabled;
                                }
                                break;
                            case "env":
                                if (currentServer.Env == null)
                                    currentServer.Env = new Dictionary<string, string>();
                                
                                if (value.Contains("="))
                                {
                                    var envParts = value.Split('=', 2);
                                    if (envParts.Length == 2)
                                    {
                                        currentServer.Env[envParts[0].Trim()] = envParts[1].Trim();
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            // 保存最后一个服务器
            if (currentServer != null && !string.IsNullOrEmpty(currentServerName))
            {
                servers[currentServerName] = currentServer;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing Claude MCP list: {ex.Message}");
        }

        return servers;
    }
}

public class McpServerTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public McpServer Server { get; set; } = new();
}