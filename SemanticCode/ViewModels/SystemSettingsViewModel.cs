using System.Collections.Generic;
using ReactiveUI;

namespace SemanticCode.ViewModels;

public class SystemSettingsViewModel : ViewModelBase
{
    public string Title { get; } = "系统设置";
    public string ThemeLabel { get; } = "主题";
    public string LanguageLabel { get; } = "语言";
    public string AutoStartLabel { get; } = "开机自启动";
    public string NotificationLabel { get; } = "通知";
    
    private string _selectedTheme = "跟随系统";
    public string SelectedTheme
    {
        get => _selectedTheme;
        set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
    }
    
    private string _selectedLanguage = "中文";
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
    }
    
    private bool _autoStart = false;
    public bool AutoStart
    {
        get => _autoStart;
        set => this.RaiseAndSetIfChanged(ref _autoStart, value);
    }
    
    private bool _enableNotifications = true;
    public bool EnableNotifications
    {
        get => _enableNotifications;
        set => this.RaiseAndSetIfChanged(ref _enableNotifications, value);
    }
    
    public List<string> AvailableThemes { get; } = new()
    {
        "跟随系统",
        "浅色主题",
        "深色主题"
    };
    
    public List<string> AvailableLanguages { get; } = new()
    {
        "中文",
        "English",
        "日本語"
    };
}