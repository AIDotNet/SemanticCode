using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SemanticCode.Models;
using SemanticCode.Services;

namespace SemanticCode.ViewModels;

public class AgentHubViewModel : ViewModelBase
{
    private readonly AgentHubService _agentHubService;

    private AgentHubResponse? _hubData;
    private bool _isLoading;
    private bool _hasError;
    private string _agentsDirectory = string.Empty;
    private string _cacheStatus = string.Empty;
    private string _searchText = string.Empty;
    private List<AgentHubItem> _filteredAgents = new();

    public AgentHubResponse? HubData
    {
        get => _hubData;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _hubData, value);
            UpdateFilteredAgents();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    public string AgentsDirectory
    {
        get => _agentsDirectory;
        set => this.RaiseAndSetIfChanged(ref _agentsDirectory, value);
    }

    public string CacheStatus
    {
        get => _cacheStatus;
        set => this.RaiseAndSetIfChanged(ref _cacheStatus, value);
    }

    public string SearchText
    {
        get => _searchText;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _searchText, value);
            UpdateFilteredAgents();
        }
    }

    public List<AgentHubItem> FilteredAgents
    {
        get => _filteredAgents;
        set => this.RaiseAndSetIfChanged(ref _filteredAgents, value);
    }

    public int TotalCount => HubData?.Agents?.Count ?? 0;
    public int FilteredCount => FilteredAgents?.Count ?? 0;
    public string CountDisplay => string.IsNullOrEmpty(SearchText) ? 
        $"共 {TotalCount} 个Agent" : 
        $"找到 {FilteredCount} 个Agent（共 {TotalCount} 个）";

    public bool HasData => HubData != null && !IsLoading && !HasError;

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearCacheCommand { get; }
    public ReactiveCommand<AgentHubItem, Unit> InstallAgentCommand { get; }

    public AgentHubViewModel()
    {
        _agentHubService = new AgentHubService();
        AgentsDirectory = _agentHubService.GetAgentsDirectory();
        
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
        ClearCacheCommand = ReactiveCommand.CreateFromTask(ClearCacheAsync);
        InstallAgentCommand = ReactiveCommand.CreateFromTask<AgentHubItem>(InstallAgentAsync);
        
        // Load data on startup (use cache if available)
        _ = Task.Run(() => LoadDataAsync(false));
    }

    private void UpdateFilteredAgents()
    {
        if (HubData?.Agents == null)
        {
            FilteredAgents = new List<AgentHubItem>();
        }
        else if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredAgents = HubData.Agents.ToList();
        }
        else
        {
            var searchTerm = SearchText.ToLower();
            FilteredAgents = HubData.Agents
                .Where(agent => 
                    agent.Name.ToLower().Contains(searchTerm) ||
                    agent.Description.ToLower().Contains(searchTerm))
                .ToList();
        }
        
        // 通知UI更新计数显示
        this.RaisePropertyChanged(nameof(TotalCount));
        this.RaisePropertyChanged(nameof(FilteredCount));
        this.RaisePropertyChanged(nameof(CountDisplay));
    }

    private async Task LoadDataAsync(bool forceRefresh)
    {
        IsLoading = true;
        HasError = false;
        
        // 如果不是强制刷新且已有数据，跳过loading状态
        if (!forceRefresh && HubData != null)
        {
            IsLoading = false;
        }
        else
        {
            HubData = null;
            this.RaisePropertyChanged(nameof(HasData));
        }

        try
        {
            var data = await _agentHubService.FetchAgentHubDataAsync(forceRefresh);
            HubData = data;
            HasError = data == null;
            
            // 更新缓存状态
            if (data != null)
            {
                CacheStatus = forceRefresh ? "数据已更新" : "使用缓存数据";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            Console.WriteLine($"Error loading agent hub data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            this.RaisePropertyChanged(nameof(HasData));
        }
    }

    private async Task RefreshAsync()
    {
        // 强制刷新数据
        await LoadDataAsync(true);
    }

    private async Task ClearCacheAsync()
    {
        try
        {
            _agentHubService.ClearCache();
            // 清除缓存后重新加载数据
            await LoadDataAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing cache: {ex.Message}");
        }
    }

    private async Task InstallAgentAsync(AgentHubItem? agent)
    {
        if (agent == null || agent.IsInstalled)
            return;

        try
        {
            var success = await _agentHubService.DownloadAndInstallAgentAsync(agent);
            if (success)
            {
                // 只更新安装状态，不重新加载整个列表
                agent.IsInstalled = true;
                
                // 通知UI更新
                this.RaisePropertyChanged(nameof(HubData));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error installing agent {agent.Name}: {ex.Message}");
        }
    }
}