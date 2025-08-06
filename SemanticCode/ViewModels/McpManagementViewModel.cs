using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using SemanticCode.Models;
using SemanticCode.Services;

namespace SemanticCode.ViewModels;

public class McpManagementViewModel : ViewModelBase
{
    private ObservableCollection<McpServerStatus> _mcpServers = new();
    private McpServerStatus? _selectedServer;
    private string _newServerJson = string.Empty;
    private bool _isAddingServer;
    private bool _isLoading;
    private string _statusMessage = string.Empty;
    private bool _isClaudeCliAvailable;

    public ObservableCollection<McpServerStatus> McpServers
    {
        get => _mcpServers;
        set => this.RaiseAndSetIfChanged(ref _mcpServers, value);
    }

    public McpServerStatus? SelectedServer
    {
        get => _selectedServer;
        set => this.RaiseAndSetIfChanged(ref _selectedServer, value);
    }

    public string NewServerJson
    {
        get => _newServerJson;
        set => this.RaiseAndSetIfChanged(ref _newServerJson, value);
    }

    public bool IsAddingServer
    {
        get => _isAddingServer;
        set => this.RaiseAndSetIfChanged(ref _isAddingServer, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool IsClaudeCliAvailable
    {
        get => _isClaudeCliAvailable;
        set => this.RaiseAndSetIfChanged(ref _isClaudeCliAvailable, value);
    }

    public ICommand LoadServersCommand { get; }
    public ICommand RemoveServerCommand { get; }
    public ICommand EnableServerCommand { get; }
    public ICommand DisableServerCommand { get; }
    public ICommand RefreshStatusCommand { get; }
    public ICommand ShowAddServerCommand { get; }
    public ICommand CancelAddServerCommand { get; }
    public ICommand SaveNewServerCommand { get; }
    public ICommand CheckClaudeCliCommand { get; }
    public ICommand SyncAllToClaudeCommand { get; }

    public McpManagementViewModel()
    {
        LoadServersCommand = ReactiveCommand.CreateFromTask(LoadServersAsync);
        RemoveServerCommand = ReactiveCommand.CreateFromTask<McpServerStatus>(RemoveServerAsync);
        EnableServerCommand = ReactiveCommand.CreateFromTask<McpServerStatus>(EnableServerAsync);
        DisableServerCommand = ReactiveCommand.CreateFromTask<McpServerStatus>(DisableServerAsync);
        RefreshStatusCommand = ReactiveCommand.CreateFromTask(RefreshStatusAsync);
        ShowAddServerCommand = ReactiveCommand.Create(ShowAddServer);
        CancelAddServerCommand = ReactiveCommand.Create(CancelAddServer);
        SaveNewServerCommand = ReactiveCommand.CreateFromTask(SaveNewServerAsync);
        CheckClaudeCliCommand = ReactiveCommand.CreateFromTask(CheckClaudeCliAsync);
        SyncAllToClaudeCommand = ReactiveCommand.CreateFromTask(SyncAllToClaudeAsync);

        _ = CheckClaudeCliAsync();
        _ = LoadServersAsync();
    }

    private async Task LoadServersAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "加载MCP服务器...";

            var servers = await McpService.GetMcpServerStatusesAsync();
            
            McpServers.Clear();
            foreach (var server in servers)
            {
                McpServers.Add(server);
            }

            StatusMessage = $"已加载 {servers.Count} 个MCP服务器";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RemoveServerAsync(McpServerStatus server)
    {
        try
        {
            await McpService.RemoveMcpServerAsync(server.Name);
            StatusMessage = $"已删除MCP服务器: {server.Name}";
            await LoadServersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败: {ex.Message}";
        }
    }

    private async Task EnableServerAsync(McpServerStatus server)
    {
        try
        {
            await McpService.EnableMcpServerAsync(server.Name, true);
            StatusMessage = $"已启用MCP服务器: {server.Name}";
            await LoadServersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"启用失败: {ex.Message}";
        }
    }

    private async Task DisableServerAsync(McpServerStatus server)
    {
        try
        {
            await McpService.EnableMcpServerAsync(server.Name, false);
            StatusMessage = $"已禁用MCP服务器: {server.Name}";
            await LoadServersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"禁用失败: {ex.Message}";
        }
    }

    private async Task RefreshStatusAsync()
    {
        await LoadServersAsync();
    }

    private void ShowAddServer()
    {
        IsAddingServer = true;
        NewServerJson = string.Empty;
    }

    private void CancelAddServer()
    {
        IsAddingServer = false;
    }

    private async Task SaveNewServerAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewServerJson))
            {
                StatusMessage = "JSON配置不能为空";
                return;
            }

            // 验证JSON格式
            Dictionary<string, McpServer>? serverConfigs;
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                
                serverConfigs = JsonSerializer.Deserialize<Dictionary<string, McpServer>>(NewServerJson, jsonOptions);
                if (serverConfigs == null || serverConfigs.Count == 0)
                {
                    StatusMessage = "JSON配置无效或为空";
                    return;
                }
            }
            catch (JsonException ex)
            {
                StatusMessage = $"JSON格式错误: {ex.Message}";
                return;
            }

            // 检查是否有重复的服务器名称
            var duplicateNames = serverConfigs.Keys.Where(name => McpServers.Any(s => s.Name == name)).ToList();
            if (duplicateNames.Any())
            {
                StatusMessage = $"服务器名称已存在: {string.Join(", ", duplicateNames)}";
                return;
            }

            // 验证每个服务器配置
            var validationErrors = new List<string>();
            foreach (var kvp in serverConfigs)
            {
                var validation = McpService.ValidateMcpServer(kvp.Key, kvp.Value);
                if (!validation.IsValid)
                {
                    validationErrors.AddRange(validation.Errors.Select(e => $"{kvp.Key}: {e}"));
                }
            }

            if (validationErrors.Any())
            {
                StatusMessage = $"验证失败: {string.Join("; ", validationErrors)}";
                return;
            }

            // 使用批量添加方法
            await McpService.AddMcpServersAsync(serverConfigs);
            
            var addedCount = serverConfigs.Count;
            var addedNames = serverConfigs.Keys.ToList();

            StatusMessage = $"已添加 {addedCount} 个MCP服务器: {string.Join(", ", addedNames)}";
            IsAddingServer = false;
            await LoadServersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"添加失败: {ex.Message}";
        }
    }

    private async Task CheckClaudeCliAsync()
    {
        try
        {
            IsClaudeCliAvailable = await McpService.IsClaudeCliAvailableAsync();
            
            if (IsClaudeCliAvailable)
            {
                StatusMessage = "Claude CLI 可用，MCP服务器配置将自动同步到Claude";
            }
            else
            {
                StatusMessage = "Claude CLI 不可用，仅支持本地配置管理";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"检查Claude CLI状态失败: {ex.Message}";
            IsClaudeCliAvailable = false;
        }
    }

    private async Task SyncAllToClaudeAsync()
    {
        if (!IsClaudeCliAvailable)
        {
            StatusMessage = "Claude CLI 不可用，无法同步";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "同步所有MCP服务器到Claude CLI...";

            await McpService.SyncAllMcpServersToClaudeAsync();
            StatusMessage = "所有MCP服务器已同步到Claude CLI";
        }
        catch (Exception ex)
        {
            StatusMessage = $"同步失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}