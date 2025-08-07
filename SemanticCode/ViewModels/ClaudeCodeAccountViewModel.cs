using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using SemanticCode.Views;

namespace SemanticCode.ViewModels;

public class ClaudeCodeAccountViewModel : ViewModelBase
{
    private int _selectedTabIndex;
    private string _userEmail = "";
    private string _organizationName = "";
    private decimal _totalCost;
    private int _totalInputTokens;
    private int _totalOutputTokens;
    private int _totalCacheCreationTokens;
    private int _totalCacheReadTokens;
    private int _totalApiDuration;
    private int _projectCount;
    private int _totalSessions;
    private int _totalMessages;
    private int _averageSessionLength;
    private DateTime _lastActiveDate;
    private string _mostUsedModel = "";
    private decimal _averageCostPerSession;
    private int _errorCount;
    private double _errorRate;
    private int _mcpServerCount;
    private string _mostActiveProject = "";

    public ObservableCollection<ProjectInfo> Projects { get; } = new();
    public ObservableCollection<McpServerInfo> AllMcpServers { get; } = new();
    public ObservableCollection<ModelUsageInfo> ModelUsageStats { get; } = new();
    public ObservableCollection<SessionAnalyticsInfo> SessionAnalytics { get; } = new();
    public ObservableCollection<ProjectActivityInfo> ProjectActivity { get; } = new();

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
    }

    public string UserEmail
    {
        get => _userEmail;
        set => this.RaiseAndSetIfChanged(ref _userEmail, value);
    }

    public string OrganizationName
    {
        get => _organizationName;
        set => this.RaiseAndSetIfChanged(ref _organizationName, value);
    }

    public decimal TotalCost
    {
        get => _totalCost;
        set => this.RaiseAndSetIfChanged(ref _totalCost, value);
    }

    public int TotalInputTokens
    {
        get => _totalInputTokens;
        set
        {
            this.RaiseAndSetIfChanged(ref _totalInputTokens, value);
            this.RaisePropertyChanged(nameof(TotalInputTokensDisplay));
        }
    }

    public int TotalOutputTokens
    {
        get => _totalOutputTokens;
        set
        {
            this.RaiseAndSetIfChanged(ref _totalOutputTokens, value);
            this.RaisePropertyChanged(nameof(TotalOutputTokensDisplay));
        }
    }

    public int TotalCacheCreationTokens
    {
        get => _totalCacheCreationTokens;
        set
        {
            this.RaiseAndSetIfChanged(ref _totalCacheCreationTokens, value);
            this.RaisePropertyChanged(nameof(TotalCacheCreationTokensDisplay));
        }
    }

    public int TotalCacheReadTokens
    {
        get => _totalCacheReadTokens;
        set
        {
            this.RaiseAndSetIfChanged(ref _totalCacheReadTokens, value);
            this.RaisePropertyChanged(nameof(TotalCacheReadTokensDisplay));
        }
    }

    public int TotalApiDuration
    {
        get => _totalApiDuration;
        set
        {
            this.RaiseAndSetIfChanged(ref _totalApiDuration, value);
            this.RaisePropertyChanged(nameof(TotalApiDurationDisplay));
        }
    }

    public int ProjectCount
    {
        get => _projectCount;
        set => this.RaiseAndSetIfChanged(ref _projectCount, value);
    }

    public int TotalSessions
    {
        get => _totalSessions;
        set => this.RaiseAndSetIfChanged(ref _totalSessions, value);
    }

    public int TotalMessages
    {
        get => _totalMessages;
        set => this.RaiseAndSetIfChanged(ref _totalMessages, value);
    }

    public int AverageSessionLength
    {
        get => _averageSessionLength;
        set => this.RaiseAndSetIfChanged(ref _averageSessionLength, value);
    }

    public DateTime LastActiveDate
    {
        get => _lastActiveDate;
        set => this.RaiseAndSetIfChanged(ref _lastActiveDate, value);
    }

    public string MostUsedModel
    {
        get => _mostUsedModel;
        set => this.RaiseAndSetIfChanged(ref _mostUsedModel, value);
    }

    public decimal AverageCostPerSession
    {
        get => _averageCostPerSession;
        set => this.RaiseAndSetIfChanged(ref _averageCostPerSession, value);
    }

    public int ErrorCount
    {
        get => _errorCount;
        set => this.RaiseAndSetIfChanged(ref _errorCount, value);
    }

    public double ErrorRate
    {
        get => _errorRate;
        set => this.RaiseAndSetIfChanged(ref _errorRate, value);
    }

    public int McpServerCount
    {
        get => _mcpServerCount;
        set => this.RaiseAndSetIfChanged(ref _mcpServerCount, value);
    }

    public string MostActiveProject
    {
        get => _mostActiveProject;
        set => this.RaiseAndSetIfChanged(ref _mostActiveProject, value);
    }

    public string TotalInputTokensDisplay => FormatTokenCount(TotalInputTokens);
    public string TotalOutputTokensDisplay => FormatTokenCount(TotalOutputTokens);
    public string TotalCacheCreationTokensDisplay => FormatTokenCount(TotalCacheCreationTokens);
    public string TotalCacheReadTokensDisplay => FormatTokenCount(TotalCacheReadTokens);
    public string TotalApiDurationDisplay => FormatDuration(TotalApiDuration);

    public string LastActiveDateDisplay =>
        LastActiveDate == DateTime.MinValue ? "未知" : LastActiveDate.ToString("yyyy-MM-dd HH:mm");

    public string ErrorRateDisplay => $"{ErrorRate:F1}%";

    public ICommand RefreshDataCommand { get; }
    public ICommand EditMcpCommand { get; }
    public ICommand ViewSessionHistoryCommand { get; }

    public ICommand OpenClaudeCommand { get; }

    public ClaudeCodeAccountViewModel()
    {
        RefreshDataCommand = ReactiveCommand.CreateFromTask(RefreshData);
        EditMcpCommand = ReactiveCommand.Create<ProjectInfo>(EditMcp);
        ViewSessionHistoryCommand = ReactiveCommand.Create<ProjectInfo>(ViewSessionHistory);
        OpenClaudeCommand = ReactiveCommand.Create<ProjectInfo>(OpenClaudeConsole);

        _ = RefreshData();
    }

    private void OpenClaudeConsole(ProjectInfo project)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c start cmd /k claude",
                UseShellExecute = false,
                WorkingDirectory = project.Path,
                CreateNoWindow = true
            };

            Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening Claude console: {ex.Message}");
        }
    }

    private async Task RefreshData()
    {
        try
        {
            await LoadClaudeData();
            await LoadProjectsData();
            CollectAllMcpServers();
            await AnalyzeSessionData();
            AnalyzeModelUsage();
            AnalyzeProjectActivity();
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error loading data: {ex.Message}");
        }
    }

    private async Task LoadClaudeData()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var claudeConfigPath = Path.Combine(userProfile, ".claude.json");

        if (!File.Exists(claudeConfigPath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(claudeConfigPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("oauthAccount", out var oauthAccount))
            {
                if (oauthAccount.TryGetProperty("emailAddress", out var email))
                    UserEmail = email.GetString() ?? "";

                if (oauthAccount.TryGetProperty("organizationName", out var orgName))
                    OrganizationName = orgName.GetString() ?? "";
            }

            decimal totalCost = 0;
            int totalInputTokens = 0;
            int totalOutputTokens = 0;
            int totalCacheCreationTokens = 0;
            int totalCacheReadTokens = 0;
            int totalApiDuration = 0;

            if (root.TryGetProperty("projects", out var projects))
            {
                ProjectCount = projects.EnumerateObject().Count();

                foreach (var project in projects.EnumerateObject())
                {
                    var projectData = project.Value;
                    if (projectData.TryGetProperty("lastCost", out var cost))
                        totalCost += (decimal)cost.GetDouble();

                    if (projectData.TryGetProperty("lastTotalInputTokens", out var inputTokens))
                        totalInputTokens += inputTokens.GetInt32();

                    if (projectData.TryGetProperty("lastTotalOutputTokens", out var outputTokens))
                        totalOutputTokens += outputTokens.GetInt32();

                    if (projectData.TryGetProperty("lastTotalCacheCreationInputTokens", out var cacheCreationTokens))
                        totalCacheCreationTokens += cacheCreationTokens.GetInt32();

                    if (projectData.TryGetProperty("lastTotalCacheReadInputTokens", out var cacheReadTokens))
                        totalCacheReadTokens += cacheReadTokens.GetInt32();

                    if (projectData.TryGetProperty("lastAPIDuration", out var apiDuration))
                        totalApiDuration += apiDuration.GetInt32();
                }
            }

            TotalCost = totalCost;
            TotalInputTokens = totalInputTokens;
            TotalOutputTokens = totalOutputTokens;
            TotalCacheCreationTokens = totalCacheCreationTokens;
            TotalCacheReadTokens = totalCacheReadTokens;
            TotalApiDuration = totalApiDuration;

            // Calculate derived metrics
            if (TotalSessions > 0)
            {
                AverageCostPerSession = TotalCost / TotalSessions;
                AverageSessionLength = TotalMessages / TotalSessions;
            }

            if (TotalMessages > 0)
            {
                ErrorRate = (double)ErrorCount / TotalMessages * 100;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing claude config: {ex.Message}");
        }
    }

    private async Task LoadProjectsData()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var claudeConfigPath = Path.Combine(userProfile, ".claude.json");

        if (!File.Exists(claudeConfigPath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(claudeConfigPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Projects.Clear();

            if (root.TryGetProperty("projects", out var projects))
            {
                foreach (var project in projects.EnumerateObject())
                {
                    var projectName = project.Name;
                    var projectData = project.Value;

                    var projectInfo = new ProjectInfo
                    {
                        Name = Path.GetFileName(projectName) ?? projectName,
                        Path = projectName,
                        Cost = projectData.TryGetProperty("lastCost", out var cost) ? (decimal)cost.GetDouble() : 0,
                        InputTokens = projectData.TryGetProperty("lastTotalInputTokens", out var inputTokens)
                            ? inputTokens.GetInt32()
                            : 0,
                        OutputTokens = projectData.TryGetProperty("lastTotalOutputTokens", out var outputTokens)
                            ? outputTokens.GetInt32()
                            : 0,
                        CacheCreationTokens =
                            projectData.TryGetProperty("lastTotalCacheCreationInputTokens", out var cacheCreation)
                                ? cacheCreation.GetInt32()
                                : 0,
                        CacheReadTokens = projectData.TryGetProperty("lastTotalCacheReadInputTokens", out var cacheRead)
                            ? cacheRead.GetInt32()
                            : 0,
                        ApiDuration = projectData.TryGetProperty("lastAPIDuration", out var duration)
                            ? duration.GetInt32()
                            : 0,
                        ApiDurationRaw = projectData.TryGetProperty("lastAPIDuration", out var durationRaw)
                            ? durationRaw.GetInt32()
                            : 0,
                        McpServers = new ObservableCollection<McpServerInfo>()
                    };

                    if (projectData.TryGetProperty("mcpServers", out var mcpServers))
                    {
                        foreach (var mcpServer in mcpServers.EnumerateObject())
                        {
                            var serverInfo = new McpServerInfo
                            {
                                Name = mcpServer.Name,
                                Type = mcpServer.Value.TryGetProperty("type", out var type)
                                    ? type.GetString() ?? ""
                                    : "",
                                Url = mcpServer.Value.TryGetProperty("url", out var url) ? url.GetString() ?? "" : "",
                                Command = mcpServer.Value.TryGetProperty("command", out var command)
                                    ? command.GetString() ?? ""
                                    : ""
                            };
                            projectInfo.McpServers.Add(serverInfo);
                        }
                    }

                    Projects.Add(projectInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading projects: {ex.Message}");
        }
    }

    private async Task AnalyzeSessionData()
    {
        try
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var projectsDirectory = Path.Combine(userProfile, ".claude", "projects");

            if (!Directory.Exists(projectsDirectory))
                return;

            int totalSessions = 0;
            int totalMessages = 0;
            int errorCount = 0;
            DateTime lastActive = DateTime.MinValue;
            var modelUsage = new Dictionary<string, int>();

            SessionAnalytics.Clear();

            foreach (var project in Projects)
            {
                var convertedPath = project.Path.Replace("/", "-").Replace("\\", "-").Replace(":", "-")
                    .Replace(".", "-");
                if (convertedPath.StartsWith("-"))
                    convertedPath = convertedPath.Substring(1);

                var projectDirectory = Path.Combine(projectsDirectory, convertedPath);

                if (!Directory.Exists(projectDirectory))
                    continue;

                var sessionFiles = Directory.GetFiles(projectDirectory, "*.jsonl");
                var projectSessions = 0;
                var projectMessages = 0;
                var projectErrors = 0;
                DateTime projectLastActive = DateTime.MinValue;

                foreach (var sessionFile in sessionFiles)
                {
                    var fileLastWrite = File.GetLastWriteTime(sessionFile);
                    if (fileLastWrite > lastActive)
                        lastActive = fileLastWrite;
                    if (fileLastWrite > projectLastActive)
                        projectLastActive = fileLastWrite;

                    var lines = await File.ReadAllLinesAsync(sessionFile);
                    bool hasUserMessage = false;
                    int sessionMessages = 0;

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            using var doc = JsonDocument.Parse(line);
                            var root = doc.RootElement;

                            if (root.TryGetProperty("type", out var typeElement))
                            {
                                var messageType = typeElement.GetString();

                                if (messageType == "user")
                                {
                                    hasUserMessage = true;
                                    sessionMessages++;
                                    totalMessages++;
                                    projectMessages++;
                                }
                                else if (messageType == "assistant" && root.TryGetProperty("message", out var message))
                                {
                                    sessionMessages++;
                                    totalMessages++;
                                    projectMessages++;

                                    // Track model usage
                                    if (message.TryGetProperty("model", out var modelElement))
                                    {
                                        var model = modelElement.GetString() ?? "unknown";
                                        modelUsage[model] = modelUsage.GetValueOrDefault(model, 0) + 1;
                                    }
                                }
                                else if (messageType == "error")
                                {
                                    errorCount++;
                                    projectErrors++;
                                }
                            }
                        }
                        catch (JsonException)
                        {
                            // Skip invalid JSON lines
                            continue;
                        }
                    }

                    if (hasUserMessage)
                    {
                        totalSessions++;
                        projectSessions++;
                    }
                }

                // Create project session analytics
                if (projectSessions > 0)
                {
                    SessionAnalytics.Add(new SessionAnalyticsInfo
                    {
                        ProjectName = project.Name,
                        SessionCount = projectSessions,
                        MessageCount = projectMessages,
                        ErrorCount = projectErrors,
                        LastActiveDate = projectLastActive,
                        AverageMessagesPerSession = projectMessages / projectSessions,
                        ErrorRate = projectMessages > 0 ? (double)projectErrors / projectMessages * 100 : 0
                    });
                }
            }

            TotalSessions = totalSessions;
            TotalMessages = totalMessages;
            ErrorCount = errorCount;
            LastActiveDate = lastActive;

            // Find most used model
            if (modelUsage.Any())
            {
                MostUsedModel = modelUsage.OrderByDescending(x => x.Value).First().Key;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing session data: {ex.Message}");
        }
    }

    private void AnalyzeModelUsage()
    {
        try
        {
            ModelUsageStats.Clear();

            // Group projects by model usage patterns
            var modelGroups = Projects
                .GroupBy(p => "claude-sonnet") // Simplified - in real scenario, extract from session data
                .Select(g => new ModelUsageInfo
                {
                    ModelName = g.Key,
                    UsageCount = g.Count(),
                    TotalTokens = g.Sum(p => p.InputTokens + p.OutputTokens),
                    TotalCost = g.Sum(p => p.Cost),
                    ProjectsUsing = g.Count()
                })
                .OrderByDescending(x => x.UsageCount)
                .ToList();

            foreach (var model in modelGroups)
            {
                ModelUsageStats.Add(model);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing model usage: {ex.Message}");
        }
    }

    private void AnalyzeProjectActivity()
    {
        try
        {
            ProjectActivity.Clear();

            var sortedProjects = Projects
                .OrderByDescending(p => p.Cost + p.InputTokens + p.OutputTokens)
                .Select((p, index) => new ProjectActivityInfo
                {
                    ProjectName = p.Name,
                    Rank = index + 1,
                    ActivityScore = (int)(p.Cost * 100 + (decimal)((p.InputTokens + p.OutputTokens) * 0.001)),
                    TotalTokens = p.InputTokens + p.OutputTokens,
                    TotalCost = p.Cost,
                    McpServerCount = p.McpServers.Count,
                    LastActivity = "最近活动" // Could be enhanced with real data
                })
                .ToList();

            foreach (var activity in sortedProjects.Take(10)) // Top 10 most active projects
            {
                ProjectActivity.Add(activity);
            }

            if (sortedProjects.Any())
            {
                MostActiveProject = sortedProjects.First().ProjectName;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing project activity: {ex.Message}");
        }
    }

    private void CollectAllMcpServers()
    {
        AllMcpServers.Clear();
        var uniqueServers = new HashSet<string>();

        foreach (var project in Projects)
        {
            foreach (var server in project.McpServers)
            {
                var serverKey = $"{server.Name}:{server.Type}:{server.Url}:{server.Command}";
                if (uniqueServers.Add(serverKey))
                {
                    AllMcpServers.Add(new McpServerInfo
                    {
                        Name = server.Name,
                        Type = server.Type,
                        Url = server.Url,
                        Command = server.Command
                    });
                }
            }
        }

        McpServerCount = AllMcpServers.Count;
    }

    private async void EditMcp(ProjectInfo project)
    {
        try
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow != null)
            {
                var result = await McpEditDialog.ShowDialog(mainWindow, project, AllMcpServers.ToList());
                if (result)
                {
                    // Refresh the data after saving changes
                    await RefreshData();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error editing MCP: {ex.Message}");
        }
    }

    private async void ViewSessionHistory(ProjectInfo project)
    {
        try
        {
            // 获取主窗口
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow != null)
            {
                await SessionHistoryWindow.ShowDialog(mainWindow, project.Name, project.Path);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error viewing session history: {ex.Message}");
        }
    }

    private static string FormatTokenCount(int tokens)
    {
        if (tokens >= 1_000_000)
            return $"{tokens / 1_000_000.0:F1}M";
        if (tokens >= 1_000)
            return $"{tokens / 1_000.0:F1}K";
        return tokens.ToString();
    }

    private static string FormatDuration(int milliseconds)
    {
        if (milliseconds >= 60_000)
        {
            var minutes = milliseconds / 60_000.0;
            return $"{minutes:F1}分钟";
        }

        if (milliseconds >= 1_000)
        {
            var seconds = milliseconds / 1_000.0;
            return $"{seconds:F1}秒";
        }

        return $"{milliseconds}ms";
    }
}

public class ProjectInfo : ReactiveObject
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public decimal Cost { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int CacheCreationTokens { get; set; }
    public int CacheReadTokens { get; set; }
    public int ApiDuration { get; set; }
    public int ApiDurationRaw { get; set; }
    public ObservableCollection<McpServerInfo> McpServers { get; set; } = new();

    public string InputTokensDisplay => FormatTokenCount(InputTokens);
    public string OutputTokensDisplay => FormatTokenCount(OutputTokens);
    public string ApiDurationDisplay => FormatDuration(ApiDuration);

    private static string FormatTokenCount(int tokens)
    {
        if (tokens >= 1_000_000)
            return $"{tokens / 1_000_000.0:F1}M";
        if (tokens >= 1_000)
            return $"{tokens / 1_000.0:F1}K";
        return tokens.ToString();
    }

    private static string FormatDuration(int milliseconds)
    {
        if (milliseconds >= 60_000)
        {
            var minutes = milliseconds / 60_000.0;
            return $"{minutes:F1}分钟";
        }

        if (milliseconds >= 1_000)
        {
            var seconds = milliseconds / 1_000.0;
            return $"{seconds:F1}秒";
        }

        return $"{milliseconds}ms";
    }
}

public class McpServerInfo : ReactiveObject
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Url { get; set; } = "";
    public string Command { get; set; } = "";
}

public class ModelUsageInfo : ReactiveObject
{
    public string ModelName { get; set; } = "";
    public int UsageCount { get; set; }
    public int TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
    public int ProjectsUsing { get; set; }

    public string TotalTokensDisplay => FormatTokenCount(TotalTokens);
    public string TotalCostDisplay => $"${TotalCost:F2}";

    private static string FormatTokenCount(int tokens)
    {
        if (tokens >= 1_000_000)
            return $"{tokens / 1_000_000.0:F1}M";
        if (tokens >= 1_000)
            return $"{tokens / 1_000.0:F1}K";
        return tokens.ToString();
    }
}

public class SessionAnalyticsInfo : ReactiveObject
{
    public string ProjectName { get; set; } = "";
    public int SessionCount { get; set; }
    public int MessageCount { get; set; }
    public int ErrorCount { get; set; }
    public DateTime LastActiveDate { get; set; }
    public int AverageMessagesPerSession { get; set; }
    public double ErrorRate { get; set; }

    public string LastActiveDateDisplay =>
        LastActiveDate == DateTime.MinValue ? "未知" : LastActiveDate.ToString("yyyy-MM-dd");

    public string ErrorRateDisplay => $"{ErrorRate:F1}%";
}

public class ProjectActivityInfo : ReactiveObject
{
    public string ProjectName { get; set; } = "";
    public int Rank { get; set; }
    public int ActivityScore { get; set; }
    public int TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
    public int McpServerCount { get; set; }
    public string LastActivity { get; set; } = "";

    public string TotalTokensDisplay => FormatTokenCount(TotalTokens);
    public string TotalCostDisplay => $"${TotalCost:F2}";

    private static string FormatTokenCount(int tokens)
    {
        if (tokens >= 1_000_000)
            return $"{tokens / 1_000_000.0:F1}M";
        if (tokens >= 1_000)
            return $"{tokens / 1_000.0:F1}K";
        return tokens.ToString();
    }
}