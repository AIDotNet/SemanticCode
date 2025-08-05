using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using SemanticCode.Services;
using SemanticCode.Models;

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
    
    private readonly UpdateService _updateService;
    
    public SystemSettingsViewModel()
    {
        _updateService = new UpdateService();
        CheckUpdateCommand = ReactiveCommand.CreateFromTask(CheckForUpdatesAsync);
        
        // 当进入系统设置页面时自动检查更新
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000); // 延迟1秒后自动检查
            await CheckForUpdatesAsync();
        });
    }
    
    public string UpdateLabel { get; } = "更新检查";
    public ReactiveCommand<Unit, Unit> CheckUpdateCommand { get; }
    
    private bool _isCheckingUpdate = false;
    public bool IsCheckingUpdate
    {
        get => _isCheckingUpdate;
        set => this.RaiseAndSetIfChanged(ref _isCheckingUpdate, value);
    }
    
    private string _updateStatus = "点击检查更新";
    public string UpdateStatus
    {
        get => _updateStatus;
        set => this.RaiseAndSetIfChanged(ref _updateStatus, value);
    }
    
    private UpdateInfo? _latestUpdate;
    public UpdateInfo? LatestUpdate
    {
        get => _latestUpdate;
        set => this.RaiseAndSetIfChanged(ref _latestUpdate, value);
    }
    
    private async Task CheckForUpdatesAsync()
    {
        IsCheckingUpdate = true;
        UpdateStatus = "正在检查更新...";
        
        try
        {
            var updateInfo = await _updateService.CheckForUpdatesAsync();
            
            if (updateInfo == null)
            {
                UpdateStatus = "检查更新失败";
                return;
            }
            
            LatestUpdate = updateInfo;
            
            if (updateInfo.IsNewerVersion)
            {
                UpdateStatus = $"发现新版本 v{updateInfo.Version}";
            }
            else
            {
                UpdateStatus = "已是最新版本";
            }
        }
        catch (Exception)
        {
            UpdateStatus = "检查更新失败";
        }
        finally
        {
            IsCheckingUpdate = false;
        }
    }
}