using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;

namespace SemanticCode.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .WithInterFont()
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
            .UseReactiveUI()
            // 仅在 Debug 模式下启用日志追踪
#if DEBUG
            .LogToTrace();
#else
;
#endif
}