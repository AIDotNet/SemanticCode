using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using System.Threading.Tasks;

namespace SemanticCode.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        // 设置全局异常处理器
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
        {
            Logger.LogInfo("应用程序启动");

            var result = BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

            Logger.LogInfo("应用程序正常退出");
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "应用程序启动时发生未处理异常");
            Console.WriteLine(e);
            return -1; // 返回错误代码而不是重新抛出异常
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            Logger.LogError(exception, "发生未处理的应用程序域异常");
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logger.LogError(e.Exception, "发生未观察到的任务异常");
        e.SetObserved(); // 标记异常已被观察，防止应用程序崩溃
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
#if !DEBUG
            .With(new Win32PlatformOptions()
            {
                RenderingMode =
                [
                    Win32RenderingMode.Software
                ]
            })
            .With(new X11PlatformOptions
            {
                RenderingMode = [X11RenderingMode.Software]
            })
#endif
            .UseReactiveUI()
#if DEBUG
            .LogToTrace();
#else
;
#endif
}