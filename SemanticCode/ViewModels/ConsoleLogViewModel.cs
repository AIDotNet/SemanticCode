using System;
using System.Reactive;
using ReactiveUI;

namespace SemanticCode.ViewModels;

public class ConsoleLogViewModel : ViewModelBase
{
    private string _logContent = "";
    private bool _isCompleted = false;
    private bool _hasErrors = false;
    private string _title = "安装日志";

    public string LogContent
    {
        get => _logContent;
        set => this.RaiseAndSetIfChanged(ref _logContent, value);
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
    }

    public bool HasErrors
    {
        get => _hasErrors;
        set => this.RaiseAndSetIfChanged(ref _hasErrors, value);
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public bool CanClose => IsCompleted || HasErrors;

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public event EventHandler? CloseRequested;

    public ConsoleLogViewModel()
    {
        CloseCommand = ReactiveCommand.Create(() =>
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        });

        // 监听属性变化以更新CanClose
        this.WhenAnyValue(x => x.IsCompleted, x => x.HasErrors)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CanClose)));
    }

    public void AppendLog(string message)
    {
        LogContent += message;
    }

    public void SetCompleted(bool success)
    {
        IsCompleted = true;
        HasErrors = !success;
        
        if (success)
        {
            Title = "安装完成";
        }
        else
        {
            Title = "安装失败";
        }
    }
}