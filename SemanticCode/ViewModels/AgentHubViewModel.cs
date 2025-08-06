using System;
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

    public AgentHubResponse? HubData
    {
        get => _hubData;
        set => this.RaiseAndSetIfChanged(ref _hubData, value);
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

    public bool HasData => HubData != null && !IsLoading && !HasError;

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<AgentHubItem, Unit> InstallAgentCommand { get; }

    public AgentHubViewModel()
    {
        _agentHubService = new AgentHubService();
        AgentsDirectory = _agentHubService.GetAgentsDirectory();
        
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
        InstallAgentCommand = ReactiveCommand.CreateFromTask<AgentHubItem>(InstallAgentAsync);
        
        // Load data on startup
        _ = Task.Run(RefreshAsync);
    }

    private async Task RefreshAsync()
    {
        IsLoading = true;
        HasError = false;
        HubData = null;
        this.RaisePropertyChanged(nameof(HasData));

        try
        {
            var data = await _agentHubService.FetchAgentHubDataAsync();
            HubData = data;
            HasError = data == null;
        }
        catch (Exception ex)
        {
            HasError = true;
            Console.WriteLine($"Error refreshing agent hub: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            this.RaisePropertyChanged(nameof(HasData));
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
                await RefreshAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error installing agent {agent.Name}: {ex.Message}");
        }
    }
}