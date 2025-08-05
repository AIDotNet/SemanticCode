using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using SemanticCode.Pages;
using SemanticCode.ViewModels;
using System.Diagnostics;

namespace SemanticCode.Views;

public partial class MainView : UserControl
{
    private bool _isDragging;
    private Point _dragStartPoint;

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
            case "AgentsManagement":
                viewModel = new AgentsManagementViewModel();
                page = new AgentsManagementView();
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


    private void OnDragAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // 只有在点击的是空白区域或标题栏时才开始拖拽
        if (IsDragArea(e.Source))
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);

            // 获取顶层窗口
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window window)
            {
                window.BeginMoveDrag(e);
            }

            e.Handled = true;
        }
        // 如果不是拖拽区域，不处理事件，让其传递给子控件
    }

    private void OnDragAreaPointerMoved(object? sender, PointerEventArgs e)
    {
        // 这个方法主要用于更新鼠标光标样式
        if (_isDragging)
        {
            _dragStartPoint = e.GetPosition(this);
            e.Handled = true;
        }
    }

    private void OnDragAreaPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            e.Handled = true;
        }
    }

    private bool IsDragArea(object? source)
    {
        // 检查点击的源是否是可拖拽区域
        // 如果点击的是 NavigationViewItem 或其子元素，则不应该拖拽
        var element = source as Control;

        if (element is TextBlock)
            return false;

        while (element != null)
        {
            // 如果是 NavigationViewItem，则不是拖拽区域
            if (element is NavigationViewItem)
                return false;

            // 如果是菜单面板，则不是拖拽区域
            if (element.Classes.Contains("NavigationViewMenuItems") ||
                element.Classes.Contains("NavigationViewFooterMenuItems") ||
                element.Classes.Contains("NavigationViewHeader") ||
                element.Classes.Contains("NavigationViewTitle") ||
                element.Classes.Contains("SelectionIndicator"))
                return false;

            element = element.Parent as Control;
        }

        return true;
    }
}