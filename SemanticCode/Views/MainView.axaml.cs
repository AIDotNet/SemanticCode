using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using SemanticCode.ViewModels;
using SemanticCode.Pages;
using System.Diagnostics;

namespace SemanticCode.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        // 默认导航到首页
        Navigate("Home");
    }

    private void OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            if (tag == "GitHub")
            {
                OpenGitHub();
                return;
            }
            
            if (tag == "设置")
            {
                tag = "SystemSettings";
            }
            Navigate(tag);
        }
    }

    private void Navigate(string tag)
    {
        UserControl? page = null;
        ViewModelBase? viewModel = null;

        switch (tag)
        {
            case "Home":
                viewModel = new HomeViewModel();
                page = new HomeView();
                break;
            case "ClaudeCodeSettings":
                viewModel = new ClaudeCodeSettingsViewModel();
                page = new ClaudeCodeSettingsView();
                break;
            case "SystemSettings":
                viewModel = new SystemSettingsViewModel();
                page = new SystemSettingsView();
                break;
        }

        if (page != null && viewModel != null)
        {
            page.DataContext = viewModel;
            FrameView.Content = page;
        }
    }
    
    private void OpenGitHub()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/AIDotNet/SemanticCode",
                UseShellExecute = true
            });
        }
        catch
        {
            // Handle error silently
        }
    }
    
    private void OnUpdateAvailableClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/AIDotNet/SemanticCode/releases/latest",
                UseShellExecute = true
            });
        }
        catch
        {
            // Handle error silently
        }
    }
}