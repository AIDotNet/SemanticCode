using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using SemanticCode.ViewModels;
using SemanticCode.Views;

namespace SemanticCode;

public partial class App : Application
{
    private TrayIcon? _trayIcon;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            desktop.MainWindow = mainWindow;

            CreateTrayIcon();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void CreateTrayIcon()
    {
        try
        {
            var menu = new NativeMenu();

            var showMenuItem = new NativeMenuItem("显示主窗口");
            showMenuItem.Click += ShowMainWindow;
            menu.Add(showMenuItem);

            var openClaudeMenuItem = new NativeMenuItem("打开 .claude 文件夹");
            openClaudeMenuItem.Click += OpenClaudeFolder;
            menu.Add(openClaudeMenuItem);

            menu.Add(new NativeMenuItemSeparator());

            var exitMenuItem = new NativeMenuItem("退出");
            exitMenuItem.Click += ExitApplication;
            menu.Add(exitMenuItem);

            _trayIcon = new TrayIcon
            {
                Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://SemanticCode/Assets/favicon.ico"))),
                ToolTipText = "SemanticCode",
                Menu = menu
            };

            _trayIcon.Clicked += ShowMainWindow;
            TrayIcon.SetIcons(this, [_trayIcon]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to create tray icon: {ex.Message}");
        }
    }

    private void OpenClaudeFolder(object? sender, EventArgs eventArgs)
    {
        try
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var claudeFolder = Path.Combine(userProfile, ".claude");
            
            if (!Directory.Exists(claudeFolder))
            {
                Directory.CreateDirectory(claudeFolder);
            }

            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = claudeFolder,
                    UseShellExecute = true
                });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = claudeFolder,
                    UseShellExecute = true
                });
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = claudeFolder,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            // Log error or show message
            Debug.WriteLine($"Failed to open .claude folder: {ex.Message}");
        }
    }

    private void ExitApplication(object? sender, EventArgs eventArgs)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
    private void ShowMainWindow(object? sender, EventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow != null)
            {
                desktop.MainWindow.Show();
                desktop.MainWindow.WindowState = WindowState.Normal;
                desktop.MainWindow.Activate();
            }
        }
    }
}