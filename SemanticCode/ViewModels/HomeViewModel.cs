using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Reactive;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Avalonia.Media;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using SemanticCode.Services;
using SemanticCode.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace SemanticCode.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private string _claudeStatus = "检查中...";
    private string _nodeStatus = "检查中...";
    private string _gitStatus = "检查中...";
    private string _envVarStatus = "检查中...";
    private bool _isClaudeInstalled;
    private bool _isInstalling;
    private string _consoleOutput = "";
    private IBrush _claudeStatusColor = Brushes.Orange;
    private IBrush _nodeStatusColor = Brushes.Orange;
    private IBrush _gitStatusColor = Brushes.Orange;
    private IBrush _envVarStatusColor = Brushes.Orange;
    private readonly UpdateService _updateService;
    private readonly UpdateConfigService _updateConfigService;
    private bool _hasCheckedForUpdatesThisSession = false;
    private ConsoleLogWindow? _consoleWindow;
    private ConsoleLogViewModel? _consoleViewModel;

    public string Title { get; } = "Semantic Code";
    public string WelcomeMessage { get; } = "欢迎使用 Semantic Code 一款Claude Code工具。";
    public string Description { get; } = "一个功能强大的Claude Code管理工具，轻松帮你管理Claude Code项目，提供智能代码分析、项目管理和Claude Code集成等功能。";
    
    public string Version { get; } = GetApplicationVersion();
    public string BuildDate { get; } = DateTime.Now.ToString("yyyy-MM-dd");
    
    public ObservableCollection<FeatureCardViewModel> Features { get; } = new();
    public ObservableCollection<QuickActionViewModel> QuickActions { get; } = new();
    public ObservableCollection<RecentProjectViewModel> RecentProjects { get; } = new();
    
    public ReactiveCommand<string, Unit> NavigateCommand { get; }
    public ReactiveCommand<string, Unit> OpenProjectCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshStatusCommand { get; }
    public ReactiveCommand<Unit, Unit> InstallAllCommand { get; }

    public string ClaudeStatus
    {
        get => _claudeStatus;
        set => this.RaiseAndSetIfChanged(ref _claudeStatus, value);
    }

    public string NodeStatus
    {
        get => _nodeStatus;
        set => this.RaiseAndSetIfChanged(ref _nodeStatus, value);
    }

    public string GitStatus
    {
        get => _gitStatus;
        set => this.RaiseAndSetIfChanged(ref _gitStatus, value);
    }

    public string EnvVarStatus
    {
        get => _envVarStatus;
        set => this.RaiseAndSetIfChanged(ref _envVarStatus, value);
    }

    public bool IsClaudeInstalled
    {
        get => _isClaudeInstalled;
        set => this.RaiseAndSetIfChanged(ref _isClaudeInstalled, value);
    }

    public bool IsInstalling
    {
        get => _isInstalling;
        set => this.RaiseAndSetIfChanged(ref _isInstalling, value);
    }

    public string ConsoleOutput
    {
        get => _consoleOutput;
        set => this.RaiseAndSetIfChanged(ref _consoleOutput, value);
    }

    public IBrush ClaudeStatusColor
    {
        get => _claudeStatusColor;
        set => this.RaiseAndSetIfChanged(ref _claudeStatusColor, value);
    }

    public IBrush NodeStatusColor
    {
        get => _nodeStatusColor;
        set => this.RaiseAndSetIfChanged(ref _nodeStatusColor, value);
    }

    public IBrush GitStatusColor
    {
        get => _gitStatusColor;
        set => this.RaiseAndSetIfChanged(ref _gitStatusColor, value);
    }

    public IBrush EnvVarStatusColor
    {
        get => _envVarStatusColor;
        set => this.RaiseAndSetIfChanged(ref _envVarStatusColor, value);
    }

    public bool IsAllInstalled => IsClaudeInstalled && 
                                  NodeStatus.Contains("v") && !NodeStatus.Contains("需要") && 
                                  GitStatus == "已安装" && 
                                  (EnvVarStatus == "已设置" || EnvVarStatus == "Git Bash可用" || EnvVarStatus == "支持");
    
    public HomeViewModel()
    {
        _updateService = new UpdateService();
        _updateConfigService = new UpdateConfigService();
        
        NavigateCommand = ReactiveCommand.Create<string>(Navigate);
        OpenProjectCommand = ReactiveCommand.Create<string>(OpenProject);
        RefreshStatusCommand = ReactiveCommand.CreateFromTask(RefreshStatusAsync);
        InstallAllCommand = ReactiveCommand.CreateFromTask(InstallAllAsync);
        
        InitializeFeatures();
        InitializeQuickActions();
        InitializeRecentProjects();
        
        // 启动时检查状态和版本更新（只检查一次）
        Task.Run(async () => 
        {
            await RefreshStatusAsync();
            await CheckForUpdatesAsync();
        });
    }
    
    private void InitializeFeatures()
    {
        Features.Clear();
        Features.Add(new FeatureCardViewModel
        {
            Title = "智能代码分析",
            Description = "使用AI技术分析代码结构和语义",
            Icon = "🔍",
            IsEnabled = true
        });
        Features.Add(new FeatureCardViewModel
        {
            Title = "项目管理",
            Description = "统一管理多个代码项目",
            Icon = "📁",
            IsEnabled = true
        });
        Features.Add(new FeatureCardViewModel
        {
            Title = "Claude Code集成",
            Description = "与Claude Code无缝集成",
            Icon = "🤖",
            IsEnabled = true
        });
        Features.Add(new FeatureCardViewModel
        {
            Title = "自定义配置",
            Description = "灵活的配置选项和个性化设置",
            Icon = "⚙️",
            IsEnabled = true
        });
    }
    
    private void InitializeQuickActions()
    {
        QuickActions.Clear();
        QuickActions.Add(new QuickActionViewModel
        {
            Title = "新建项目",
            Description = "创建新的代码项目",
            Icon = "➕",
            Command = "NewProject"
        });
        QuickActions.Add(new QuickActionViewModel
        {
            Title = "打开项目",
            Description = "打开现有项目",
            Icon = "📂",
            Command = "OpenProject"
        });
        QuickActions.Add(new QuickActionViewModel
        {
            Title = "Claude设置",
            Description = "配置Claude Code",
            Icon = "🔧",
            Command = "ClaudeCodeSettings"
        });
        QuickActions.Add(new QuickActionViewModel
        {
            Title = "系统设置",
            Description = "应用程序设置",
            Icon = "⚙️",
            Command = "SystemSettings"
        });
    }
    
    private void InitializeRecentProjects()
    {
        RecentProjects.Clear();
        // 示例项目数据
        RecentProjects.Add(new RecentProjectViewModel
        {
            Name = "SemanticCode",
            Path = @"D:\code\SemanticCode",
            LastOpened = DateTime.Now.AddDays(-1),
            Language = "C#"
        });
        RecentProjects.Add(new RecentProjectViewModel
        {
            Name = "WebApp",
            Path = @"D:\projects\WebApp",
            LastOpened = DateTime.Now.AddDays(-3),
            Language = "JavaScript"
        });
        RecentProjects.Add(new RecentProjectViewModel
        {
            Name = "DataAnalysis",
            Path = @"D:\work\DataAnalysis",
            LastOpened = DateTime.Now.AddDays(-7),
            Language = "Python"
        });
    }
    
    private void Navigate(string destination)
    {
        // 导航逻辑将在主视图中处理
    }
    
    private void OpenProject(string projectPath)
    {
        // 打开项目逻辑
    }
    
    private static string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v1.0.0";
    }

    private async Task RefreshStatusAsync()
    {
        await CheckClaudeStatusAsync();
        await CheckNodeStatusAsync();
        await CheckGitStatusAsync();
        await CheckEnvironmentVariableAsync();
        
        // 触发IsAllInstalled属性更新
        this.RaisePropertyChanged(nameof(IsAllInstalled));
    }

    private async Task CheckClaudeStatusAsync()
    {
        try
        {
            var result = await RunCommandAsync("cmd", "/c claude --version", 5000);
            if (result.Success && !result.Output.Contains("不是内部或外部命令"))
            {
                ClaudeStatus = "已安装";
                ClaudeStatusColor = Brushes.Green;
                IsClaudeInstalled = true;
                this.RaisePropertyChanged(nameof(IsAllInstalled));
            }
            else
            {
                ClaudeStatus = "未安装";
                ClaudeStatusColor = Brushes.Red;
                IsClaudeInstalled = false;
                this.RaisePropertyChanged(nameof(IsAllInstalled));
            }
        }
        catch
        {
            ClaudeStatus = "未安装";
            ClaudeStatusColor = Brushes.Red;
            IsClaudeInstalled = false;
            this.RaisePropertyChanged(nameof(IsAllInstalled));
        }
    }

    private async Task CheckNodeStatusAsync()
    {
        try
        {
            var result = await RunCommandAsync("cmd", "/c node --version", 5000);
            if (result.Success && !string.IsNullOrEmpty(result.Output) && !result.Output.Contains("不是内部或外部命令"))
            {
                var versionMatch = Regex.Match(result.Output, @"v(\d+)\.(\d+)\.(\d+)");
                if (versionMatch.Success && int.TryParse(versionMatch.Groups[1].Value, out int major))
                {
                    if (major >= 18)
                    {
                        NodeStatus = $"v{major}.x.x";
                        NodeStatusColor = Brushes.Green;
                    }
                    else
                    {
                        NodeStatus = $"v{major}.x.x (需要>=18)";
                        NodeStatusColor = Brushes.Orange;
                    }
                }
                else
                {
                    NodeStatus = "版本未知";
                    NodeStatusColor = Brushes.Orange;
                }
            }
            else
            {
                NodeStatus = "未安装";
                NodeStatusColor = Brushes.Red;
            }
        }
        catch
        {
            NodeStatus = "未安装";
            NodeStatusColor = Brushes.Red;
        }
    }

    private async Task CheckGitStatusAsync()
    {
        try
        {
            var result = await RunCommandAsync("cmd", "/c git --version", 5000);
            if (result.Success && !result.Output.Contains("不是内部或外部命令"))
            {
                GitStatus = "已安装";
                GitStatusColor = Brushes.Green;
            }
            else
            {
                GitStatus = "未安装";
                GitStatusColor = Brushes.Red;
            }
        }
        catch
        {
            GitStatus = "未安装";
            GitStatusColor = Brushes.Red;
        }
    }

    private string? GetGitInstallPath()
    {
        try
        {
            // 只在Windows平台上检查
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            // 尝试从注册表获取Git安装路径
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GitForWindows");
            if (key?.GetValue("InstallPath") is string installPath && Directory.Exists(installPath))
            {
                return installPath;
            }

            // 尝试常见的安装路径
            var commonPaths = new[]
            {
                @"C:\Program Files\Git",
                @"C:\Program Files (x86)\Git",
                @"D:\Program Files\Git",
                @"D:\Program Files (x86)\Git"
            };

            foreach (var path in commonPaths)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private async Task CheckEnvironmentVariableAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    var gitPath = GetGitInstallPath();
                    if (!string.IsNullOrEmpty(gitPath))
                    {
                        var bashPath = Path.Combine(gitPath, "bin", "bash.exe");
                        if (File.Exists(bashPath))
                        {
                            EnvVarStatus = "Git Bash可用";
                            EnvVarStatusColor = Brushes.Green;
                            return;
                        }
                    }

                    // 检查环境变量是否设置
                    var envVar = Environment.GetEnvironmentVariable("CLAUDE_CODE_GIT_BASH_PATH", EnvironmentVariableTarget.Machine);
                    if (!string.IsNullOrEmpty(envVar) && File.Exists(envVar))
                    {
                        EnvVarStatus = "已设置";
                        EnvVarStatusColor = Brushes.Green;
                    }
                    else
                    {
                        EnvVarStatus = "未检测到";
                        EnvVarStatusColor = Brushes.Orange;
                    }
                }
                else
                {
                    // Linux/macOS本身支持bash，无需检查
                    EnvVarStatus = "支持";
                    EnvVarStatusColor = Brushes.Green;
                }
            }
            catch
            {
                EnvVarStatus = "检查失败";
                EnvVarStatusColor = Brushes.Red;
            }
        });
    }


    private async Task InstallAllAsync()
    {
        if (IsInstalling) return;
        
        IsInstalling = true;
        
        // 创建并显示控制台窗口
        await ShowConsoleWindowAsync();
        
        bool installationSuccessful = false;
        
        try
        {
            AddConsoleOutput("开始检查并安装所需组件...\n");
            
            // 检查并安装 Node.js
            if (NodeStatus == "未安装" || NodeStatus.Contains("需要"))
            {
                await InstallNodeJSAsync();
            }
            else
            {
                AddConsoleOutput("Node.js 已经满足要求，跳过安装\n");
            }
            
            // 检查并安装 Git
            if (GitStatus != "已安装")
            {
                await InstallGitAsync();
            }
            else
            {
                AddConsoleOutput("Git 已经安装，跳过安装\n");
            }
            
            // 设置环境变量（只在Windows且未检测到时需要）
            if (OperatingSystem.IsWindows() && EnvVarStatus == "未检测到")
            {
                await SetEnvironmentVariableAsync();
            }
            else
            {
                AddConsoleOutput("Bash环境已可用，跳过环境变量设置\n");
            }
            
            // 安装 Claude Code
            if (!IsClaudeInstalled)
            {
                await InstallClaudeCodeAsync();
            }
            else
            {
                AddConsoleOutput("Claude Code 已安装，跳过安装\n");
            }
            
            AddConsoleOutput("\n所有组件安装完成!\n");
            await RefreshStatusAsync();
            installationSuccessful = true;
        }
        catch (Exception ex)
        {
            AddConsoleOutput($"\n安装过程中出现错误: {ex.Message}\n");
            installationSuccessful = false;
        }
        finally
        {
            IsInstalling = false;
            
            // 更新控制台窗口状态
            if (_consoleViewModel != null)
            {
                _consoleViewModel.SetCompleted(installationSuccessful);
                
                // 如果安装成功，延迟3秒后自动关闭窗口
                if (installationSuccessful)
                {
                    await Task.Delay(3000);
                    CloseConsoleWindow();
                }
            }
        }
    }

    private async Task InstallNodeJSAsync()
    {
        AddConsoleOutput("正在下载最新版 Node.js...\n");
        
        try
        {
            using var client = new HttpClient();
            var downloadUrl = "https://nodejs.org/dist/v20.11.0/node-v20.11.0-x64.msi";
            var tempPath = Path.Combine(Path.GetTempPath(), "nodejs-installer.msi");
            
            AddConsoleOutput($"下载地址: {downloadUrl}\n");
            var response = await client.GetAsync(downloadUrl);
            response.EnsureSuccessStatusCode();
            
            await File.WriteAllBytesAsync(tempPath, await response.Content.ReadAsByteArrayAsync());
            AddConsoleOutput("下载完成，开始安装...\n");
            
            var result = await RunCommandWithRealtimeOutputAsync("msiexec", $"/i \"{tempPath}\" /quiet /qn", 300000);
            
            if (result.Success)
            {
                AddConsoleOutput("Node.js 安装完成\n");
            }
            else
            {
                AddConsoleOutput($"Node.js 安装失败: {result.Error}\n");
                throw new Exception("Node.js 安装失败");
            }
        }
        catch (Exception ex)
        {
            AddConsoleOutput($"Node.js 安装异常: {ex.Message}\n");
            throw;
        }
    }

    private async Task InstallGitAsync()
    {
        AddConsoleOutput("正在下载最新版 Git...\n");
        
        try
        {
            using var client = new HttpClient();
            var downloadUrl = "https://github.com/git-for-windows/git/releases/download/v2.43.0.windows.1/Git-2.43.0-64-bit.exe";
            var tempPath = Path.Combine(Path.GetTempPath(), "git-installer.exe");
            
            AddConsoleOutput($"下载地址: {downloadUrl}\n");
            var response = await client.GetAsync(downloadUrl);
            response.EnsureSuccessStatusCode();
            
            await File.WriteAllBytesAsync(tempPath, await response.Content.ReadAsByteArrayAsync());
            AddConsoleOutput("下载完成，开始安装...\n");
            
            var result = await RunCommandWithRealtimeOutputAsync(tempPath, "/VERYSILENT /NORESTART", 300000);
            
            if (result.Success || result.ExitCode == 0)
            {
                AddConsoleOutput("Git 安装完成\n");
            }
            else
            {
                AddConsoleOutput($"Git 安装失败: {result.Error}\n");
                throw new Exception("Git 安装失败");
            }
        }
        catch (Exception ex)
        {
            AddConsoleOutput($"Git 安装异常: {ex.Message}\n");
            throw;
        }
    }

    private async Task SetEnvironmentVariableAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    AddConsoleOutput("非Windows平台，跳过环境变量设置\n");
                    return;
                }

                AddConsoleOutput("设置环境变量 CLAUDE_CODE_GIT_BASH_PATH...\n");
                
                var gitPath = GetGitInstallPath();
                if (!string.IsNullOrEmpty(gitPath))
                {
                    var gitBashPath = Path.Combine(gitPath, "bin", "bash.exe");
                    if (File.Exists(gitBashPath))
                    {
                        Environment.SetEnvironmentVariable("CLAUDE_CODE_GIT_BASH_PATH", gitBashPath, EnvironmentVariableTarget.Machine);
                        AddConsoleOutput($"环境变量设置完成: {gitBashPath}\n");
                        return;
                    }
                }
                
                throw new Exception("无法找到 Git Bash 可执行文件");
            }
            catch (Exception ex)
            {
                AddConsoleOutput($"环境变量设置失败: {ex.Message}\n");
                throw;
            }
        });
    }

    private async Task InstallClaudeCodeAsync()
    {
        AddConsoleOutput("正在安装 Claude Code...\n");
        
        try
        {
            var result = await RunCommandWithRealtimeOutputAsync("cmd", "/c npm install -g @anthropic-ai/claude-code", 120000);
            
            if (result.Success)
            {
                AddConsoleOutput("Claude Code 安装完成\n");
            }
            else
            {
                AddConsoleOutput($"Claude Code 安装失败: {result.Error}\n");
                throw new Exception("Claude Code 安装失败");
            }
        }
        catch (Exception ex)
        {
            AddConsoleOutput($"Claude Code 安装异常: {ex.Message}\n");
            
            await Task.Delay(5000); // 延迟1秒以便用户看到错误信息
            throw;
        }
    }

    private void AddConsoleOutput(string text)
    {
        var formattedText = $"[{DateTime.Now:HH:mm:ss}] {text}";
        ConsoleOutput += formattedText;
        
        // 同时更新控制台窗口的日志
        if (_consoleViewModel != null)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _consoleViewModel.AppendLog(formattedText);
            });
        }
    }

    private async Task ShowConsoleWindowAsync()
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow != null)
            {
                _consoleViewModel = new ConsoleLogViewModel();
                _consoleWindow = new ConsoleLogWindow(_consoleViewModel);
                
                // 订阅关闭事件
                _consoleViewModel.CloseRequested += (s, e) => CloseConsoleWindow();
                
                _consoleWindow.Show(mainWindow);
            }
        });
    }

    private void CloseConsoleWindow()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_consoleWindow != null)
            {
                _consoleWindow.Close();
                _consoleWindow = null;
            }
            
            if (_consoleViewModel != null)
            {
                _consoleViewModel.CloseRequested -= (s, e) => CloseConsoleWindow();
                _consoleViewModel = null;
            }
        });
    }

    private async Task<CommandResult> RunCommandAsync(string command, string arguments, int timeoutMs = 30000)
    {
        return await Task.Run(() =>
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    return new CommandResult { Success = false, Error = "无法启动进程" };
                }

                var output = new StringBuilder();
                var error = new StringBuilder();

                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        output.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        error.AppendLine(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var finished = process.WaitForExit(timeoutMs);
                
                if (!finished)
                {
                    process.Kill();
                    return new CommandResult { Success = false, Error = "命令执行超时" };
                }

                return new CommandResult
                {
                    Success = process.ExitCode == 0,
                    ExitCode = process.ExitCode,
                    Output = output.ToString(),
                    Error = error.ToString()
                };
            }
            catch (Exception ex)
            {
                return new CommandResult { Success = false, Error = ex.Message };
            }
        });
    }

    private async Task<CommandResult> RunCommandWithRealtimeOutputAsync(string command, string arguments, int timeoutMs = 30000)
    {
        return await Task.Run(() =>
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    return new CommandResult { Success = false, Error = "无法启动进程" };
                }

                var output = new StringBuilder();
                var error = new StringBuilder();

                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                        // 实时输出到控制台
                        AddConsoleOutput($"{e.Data}\n");
                    }
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        error.AppendLine(e.Data);
                        // 实时输出错误到控制台
                        AddConsoleOutput($"ERROR: {e.Data}\n");
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var finished = process.WaitForExit(timeoutMs);
                
                if (!finished)
                {
                    process.Kill();
                    return new CommandResult { Success = false, Error = "命令执行超时" };
                }

                return new CommandResult
                {
                    Success = process.ExitCode == 0,
                    ExitCode = process.ExitCode,
                    Output = output.ToString(),
                    Error = error.ToString()
                };
            }
            catch (Exception ex)
            {
                return new CommandResult { Success = false, Error = ex.Message };
            }
        });
    }
    
    private async Task CheckForUpdatesAsync()
    {
        // 确保只在首次加载时检查更新，避免重复检查
        if (_hasCheckedForUpdatesThisSession)
            return;
            
        _hasCheckedForUpdatesThisSession = true;
        
        try
        {
            var updateInfo = await _updateService.CheckForUpdatesAsync();
            
            if (updateInfo != null && updateInfo.IsNewerVersion)
            {
                // 检查是否被用户忽略
                if (!_updateConfigService.IsVersionIgnored(updateInfo.Version))
                {
                    // 在UI线程中显示更新对话框
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await ShowUpdateNotification(updateInfo);
                    });
                }
            }
        }
        catch (Exception)
        {
            // 静默处理更新检查失败
        }
    }
    
    private async Task ShowUpdateNotification(Models.UpdateInfo updateInfo)
    {
        var viewModel = new UpdateNotificationViewModel(_updateService, updateInfo);
        var dialog = new UpdateNotificationDialog(viewModel);
        
        // 设置对话框事件处理
        viewModel.RemindLaterRequested += (s, e) => dialog.Close();
        viewModel.IgnoreVersionRequested += (s, e) => 
        {
            _updateConfigService.IgnoreVersion(updateInfo.Version);
            dialog.Close();
        };
        viewModel.UpdateCompleted += (s, e) => dialog.Close();
        
        // 显示对话框
        var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
            
        if (mainWindow != null)
        {
            await dialog.ShowDialog(mainWindow);
        }
    }
}

public class CommandResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string Output { get; set; } = "";
    public string Error { get; set; } = "";
}

public class FeatureCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class QuickActionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}

public class RecentProjectViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime LastOpened { get; set; }
    public string Language { get; set; } = string.Empty;
    public string DisplayDate => LastOpened.ToString("yyyy-MM-dd HH:mm");
}