using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.LogicalTree;
using ReactiveUI;
using SemanticCode.ViewModels;

namespace SemanticCode.Views;

public partial class ConsoleLogWindow : Window
{
    private ConsoleLogViewModel? _viewModel;

    public ConsoleLogWindow()
    {
        InitializeComponent();
    }

    public ConsoleLogWindow(ConsoleLogViewModel viewModel) : this()
    {
        DataContext = viewModel;
        _viewModel = viewModel;
        
        // 订阅关闭请求事件
        _viewModel.CloseRequested += OnCloseRequested;
        
        // 订阅日志内容变化，自动滚动到底部
        _viewModel.WhenAnyValue(x => x.LogContent)
            .Subscribe(_ => ScrollToBottom());
    }

    private void OnCloseRequested(object? sender, System.EventArgs e)
    {
        Close();
    }

    private void ScrollToBottom()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.FindControl<TextBox>("LogTextBox") is TextBox textBox)
            {
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
            }
        }, DispatcherPriority.Background);
    }

    protected override void OnClosed(System.EventArgs e)
    {
        base.OnClosed(e);
        
        // 取消订阅事件
        if (_viewModel != null)
        {
            _viewModel.CloseRequested -= OnCloseRequested;
        }
    }
}