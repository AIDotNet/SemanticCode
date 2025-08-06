using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using System.Reactive;
using SemanticCode.Models;
using SemanticCode.Services;

namespace SemanticCode.ViewModels;

public class UpdateNotificationViewModel : ViewModelBase
{
    private readonly UpdateService _updateService;
    private readonly UpdateInfo _updateInfo;
    private bool _isDownloading;
    private float _downloadProgress;
    private string _updateButtonText = "立即更新";

    public UpdateNotificationViewModel() : this(new UpdateService(), new UpdateInfo()) { }
    
    public UpdateNotificationViewModel(UpdateService updateService, UpdateInfo updateInfo)
    {
        _updateService = updateService;
        _updateInfo = updateInfo;
        
        RemindLaterCommand = ReactiveCommand.Create(RemindLater);
        IgnoreVersionCommand = ReactiveCommand.Create(IgnoreVersion);
        UpdateCommand = ReactiveCommand.CreateFromTask(StartUpdate);
        
        IsWindowsPlatform = _updateService.IsWindowsPlatform();
        UpdateButtonText = IsWindowsPlatform ? "立即下载安装" : "前往下载页面";
    }
    
    public string CurrentVersion => GetCurrentVersion();
    public string NewVersion => _updateInfo.Version;
    public string PublishedDate => _updateInfo.PublishedAt.ToString("yyyy-MM-dd HH:mm");
    public string ReleaseNotes => _updateInfo.Description;
    public bool HasReleaseNotes => !string.IsNullOrWhiteSpace(_updateInfo.Description);
    public bool IsWindowsPlatform { get; }
    
    public bool IsDownloading
    {
        get => _isDownloading;
        set => this.RaiseAndSetIfChanged(ref _isDownloading, value);
    }
    
    public float DownloadProgress
    {
        get => _downloadProgress;
        set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
    }
    
    public string UpdateButtonText
    {
        get => _updateButtonText;
        set => this.RaiseAndSetIfChanged(ref _updateButtonText, value);
    }
    
    public ICommand RemindLaterCommand { get; }
    public ICommand IgnoreVersionCommand { get; }
    public ICommand UpdateCommand { get; }
    
    public event EventHandler? RemindLaterRequested;
    public event EventHandler? IgnoreVersionRequested;
    public event EventHandler? UpdateCompleted;
    
    private void RemindLater()
    {
        RemindLaterRequested?.Invoke(this, EventArgs.Empty);
    }
    
    private void IgnoreVersion()
    {
        IgnoreVersionRequested?.Invoke(this, EventArgs.Empty);
    }
    
    private async Task StartUpdate()
    {
        try
        {
            if (!IsWindowsPlatform)
            {
                // 打开浏览器到GitHub releases页面
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _updateInfo.ReleaseUrl,
                    UseShellExecute = true
                });
                UpdateCompleted?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            // Windows平台自动下载和安装
            IsDownloading = true;
            UpdateButtonText = "下载中...";
            
            var downloadUrl = _updateService.GetWindowsInstallerUrl(_updateInfo.Version);
            var progress = new Progress<float>(p =>
            {
                DownloadProgress = p * 100;
            });
            
            var installerPath = await _updateService.DownloadUpdateAsync(downloadUrl, progress);
            
            if (installerPath != null)
            {
                UpdateButtonText = "启动安装程序...";
                
                // 启动安装程序并关闭当前应用程序
                _updateService.StartUpdateInstaller(installerPath);
            }
            else
            {
                UpdateButtonText = "下载失败，请重试";
                IsDownloading = false;
                
                // 下载失败，打开浏览器
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _updateInfo.ReleaseUrl,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception)
        {
            UpdateButtonText = "更新失败，请重试";
            IsDownloading = false;
        }
    }
    
    private static string GetCurrentVersion()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v1.0.0";
    }
}