using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
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
    public ReactiveCommand<AgentModel, Unit> EditAgentInEditorCommand { get; }
    public ReactiveCommand<AgentModel, Unit> DeleteAgentCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    
    public AgentsManagementViewModel()
    {
        _directoryService = new AgentDirectoryService();
        
        AddAgentCommand = ReactiveCommand.Create(AddAgent);
        EditAgentCommand = ReactiveCommand.Create<AgentModel>(EditAgent);
        EditAgentInEditorCommand = ReactiveCommand.Create<AgentModel>(EditAgentInEditor);
        DeleteAgentCommand = ReactiveCommand.Create<AgentModel>(DeleteAgent);
        RefreshCommand = ReactiveCommand.Create(LoadAgents);
        
        // Subscribe to agent installation notifications
        AgentNotificationService.Instance.AgentInstalled += OnAgentInstalled;
        
        LoadAgents();
        SetupFileWatcher();
    }
    
    private void LoadAgents()
    {
        try
        {
            var agentInfos = _directoryService.LoadAllAgents();
            var agentModels = agentInfos.Select(AgentModel.FromAgentInfo).ToList();
            
            // Ensure UI updates happen on UI thread
            Dispatcher.UIThread.Invoke(() =>
            {
                Agents.Clear();
                foreach (var agent in agentModels)
                {
                    Agents.Add(agent);
                }
            });
            
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
    
    private void EditAgentInEditor(AgentModel agent)
    {
        if (agent != null && !string.IsNullOrEmpty(agent.FilePath) && File.Exists(agent.FilePath))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = agent.FilePath,
                    UseShellExecute = true
                });
                System.Diagnostics.Debug.WriteLine($"Opened agent file in editor: {agent.FilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open agent file in editor: {ex.Message}");
            }
        }
    }
    
    private async void DeleteAgent(AgentModel agent)
    {
        if (agent != null && !string.IsNullOrEmpty(agent.FileName))
        {
            // 显示确认对话框
            var result = await ShowDeleteConfirmationDialog(agent);
            if (result != true)
            {
                return;
            }
            
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
    
    private async Task<bool> ShowDeleteConfirmationDialog(AgentModel agent)
    {
        try
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
                
            if (mainWindow == null)
                return false;
                
            var dialog = new Window
            {
                Title = "确认删除",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };
            
            var panel = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 20
            };
            
            panel.Children.Add(new TextBlock
            {
                Text = $"确定要删除Agent \"{agent.Name}\" 吗？",
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap
            });
            
            panel.Children.Add(new TextBlock
            {
                Text = "此操作无法撤销。",
                FontSize = 12,
                Foreground = Brushes.Gray
            });
            
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10
            };
            
            var cancelButton = new Button
            {
                Content = "取消",
                Width = 80,
                Height = 32
            };
            
            var deleteButton = new Button
            {
                Content = "删除",
                Width = 80,
                Height = 32,
                Background = Brushes.Red,
                Foreground = Brushes.White
            };
            
            bool? result = null;
            
            cancelButton.Click += (s, e) =>
            {
                result = false;
                dialog.Close();
            };
            
            deleteButton.Click += (s, e) =>
            {
                result = true;
                dialog.Close();
            };
            
            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(deleteButton);
            panel.Children.Add(buttonPanel);
            
            dialog.Content = panel;
            
            await dialog.ShowDialog(mainWindow);
            return result == true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing delete confirmation dialog: {ex.Message}");
            return false;
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
            Dispatcher.UIThread.Post(LoadAgents);
        });
    }
    
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"File renamed: {e.OldName} -> {e.Name}");
        Task.Delay(500).ContinueWith(_ => 
        {
            Dispatcher.UIThread.Post(LoadAgents);
        });
    }
    
    private void OnAgentInstalled(object? sender, EventArgs e)
    {
        // Refresh the agents list when a new agent is installed
        LoadAgents();
    }
    
    public void Dispose()
    {
        // Unsubscribe from notifications
        AgentNotificationService.Instance.AgentInstalled -= OnAgentInstalled;
        
        if (_fileWatcher != null)
        {
            _fileWatcher.Changed -= OnFileChanged;
            _fileWatcher.Renamed -= OnFileRenamed;
            _fileWatcher.Dispose();
            _fileWatcher = null;
        }
    }
}