using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Reactive;
using System.Reflection;

namespace SemanticCode.ViewModels;

public class HomeViewModel : ViewModelBase
{
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
    
    public HomeViewModel()
    {
        NavigateCommand = ReactiveCommand.Create<string>(Navigate);
        OpenProjectCommand = ReactiveCommand.Create<string>(OpenProject);
        InitializeFeatures();
        InitializeQuickActions();
        InitializeRecentProjects();
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