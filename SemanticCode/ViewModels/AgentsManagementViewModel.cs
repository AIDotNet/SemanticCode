using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SemanticCode.Models;
using SemanticCode.Services;

namespace SemanticCode.ViewModels;

public class AgentsManagementViewModel : ViewModelBase, IDisposable
{
    private AgentModel? _selectedAgent;
    private readonly AgentDirectoryService _directoryService;
    private FileSystemWatcher? _fileWatcher;
    
    public ObservableCollection<AgentModel> Agents { get; } = new();
    
    public AgentModel? SelectedAgent
    {
        get => _selectedAgent;
        set => this.RaiseAndSetIfChanged(ref _selectedAgent, value);
    }
    
    public string AgentsDirectoryPath => _directoryService.GetAgentsDirectoryPath();
    
    public ReactiveCommand<Unit, Unit> AddAgentCommand { get; }
    public ReactiveCommand<AgentModel, Unit> EditAgentCommand { get; }
    public ReactiveCommand<AgentModel, Unit> DeleteAgentCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    
    public AgentsManagementViewModel()
    {
        _directoryService = new AgentDirectoryService();
        
        AddAgentCommand = ReactiveCommand.Create(AddAgent);
        EditAgentCommand = ReactiveCommand.Create<AgentModel>(EditAgent);
        DeleteAgentCommand = ReactiveCommand.Create<AgentModel>(DeleteAgent);
        RefreshCommand = ReactiveCommand.Create(LoadAgents);
        
        LoadAgents();
        SetupFileWatcher();
    }
    
    private void LoadAgents()
    {
        try
        {
            var agentInfos = _directoryService.LoadAllAgents();
            var agentModels = agentInfos.Select(AgentModel.FromAgentInfo).ToList();
            
            Agents.Clear();
            foreach (var agent in agentModels)
            {
                Agents.Add(agent);
            }
            
            System.Diagnostics.Debug.WriteLine($"Loaded {Agents.Count} agents from {AgentsDirectoryPath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading agents: {ex.Message}");
        }
    }
    
    private void AddAgent()
    {
        var newAgent = new AgentModel
        {
            Name = "新建Agent",
            Description = "请编辑此Agent的配置信息",
            Status = "活跃",
            Type = "通用",
            Color = "default",
            IsEnabled = true
        };
        
        // 保存到文件
        var agentInfo = newAgent.ToAgentInfo();
        if (_directoryService.SaveAgent(agentInfo))
        {
            // 更新文件路径和文件名
            newAgent.FilePath = agentInfo.FilePath;
            newAgent.FileName = agentInfo.FileName;
            
            Agents.Add(newAgent);
            SelectedAgent = newAgent;
            
            System.Diagnostics.Debug.WriteLine($"Created new agent: {newAgent.Name}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Failed to create new agent: {newAgent.Name}");
        }
    }
    
    private void EditAgent(AgentModel agent)
    {
        if (agent != null)
        {
            agent.UpdatedAt = DateTime.Now;
            
            // 保存到文件
            var agentInfo = agent.ToAgentInfo();
            if (_directoryService.SaveAgent(agentInfo))
            {
                System.Diagnostics.Debug.WriteLine($"Saved agent: {agent.Name}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save agent: {agent.Name}");
            }
        }
    }
    
    private void DeleteAgent(AgentModel agent)
    {
        if (agent != null && !string.IsNullOrEmpty(agent.FileName))
        {
            if (_directoryService.DeleteAgent(agent.FileName))
            {
                Agents.Remove(agent);
                if (SelectedAgent == agent)
                {
                    SelectedAgent = null;
                }
                
                System.Diagnostics.Debug.WriteLine($"Deleted agent: {agent.Name}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete agent: {agent.Name}");
            }
        }
    }
    
    private void SetupFileWatcher()
    {
        try
        {
            var agentsDirectory = AgentsDirectoryPath;
            if (!Directory.Exists(agentsDirectory))
            {
                return;
            }
            
            _fileWatcher = new FileSystemWatcher(agentsDirectory, "*.md")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };
            
            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.Created += OnFileCreated;
            _fileWatcher.Deleted += OnFileDeleted;
            _fileWatcher.Renamed += OnFileRenamed;
            
            System.Diagnostics.Debug.WriteLine($"File watcher setup for: {agentsDirectory}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting up file watcher: {ex.Message}");
        }
    }
    
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"File changed: {e.Name}");
        // 延迟加载以避免文件锁定问题
        Task.Delay(500).ContinueWith(_ => 
        {
            LoadAgents();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    
    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"File created: {e.Name}");
        Task.Delay(500).ContinueWith(_ => 
        {
            LoadAgents();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    
    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"File deleted: {e.Name}");
        Task.Delay(500).ContinueWith(_ => 
        {
            LoadAgents();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"File renamed: {e.OldName} -> {e.Name}");
        Task.Delay(500).ContinueWith(_ => 
        {
            LoadAgents();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    
    public void Dispose()
    {
        if (_fileWatcher != null)
        {
            _fileWatcher.Changed -= OnFileChanged;
            _fileWatcher.Created -= OnFileCreated;
            _fileWatcher.Deleted -= OnFileDeleted;
            _fileWatcher.Renamed -= OnFileRenamed;
            _fileWatcher.Dispose();
            _fileWatcher = null;
        }
    }
}