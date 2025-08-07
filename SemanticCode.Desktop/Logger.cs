using System;
using System.IO;
using System.Text;

namespace SemanticCode.Desktop;

public static class Logger
{
    private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    private static readonly object LockObject = new object();

    static Logger()
    {
        // 确保日志目录存在
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }
    }

    public static void LogError(Exception exception, string? additionalMessage = null)
    {
        try
        {
            var logFileName = $"error_{DateTime.Now:yyyy-MM-dd}.log";
            var logFilePath = Path.Combine(LogDirectory, logFileName);
            
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR");
            
            if (!string.IsNullOrEmpty(additionalMessage))
            {
                logEntry.AppendLine($"Message: {additionalMessage}");
            }
            
            logEntry.AppendLine($"Exception: {exception.GetType().Name}");
            logEntry.AppendLine($"Message: {exception.Message}");
            logEntry.AppendLine($"StackTrace: {exception.StackTrace}");
            
            if (exception.InnerException != null)
            {
                logEntry.AppendLine($"InnerException: {exception.InnerException.GetType().Name}");
                logEntry.AppendLine($"InnerException Message: {exception.InnerException.Message}");
            }
            
            logEntry.AppendLine(new string('-', 80));
            logEntry.AppendLine();

            // 使用锁确保线程安全写入
            lock (LockObject)
            {
                File.AppendAllText(logFilePath, logEntry.ToString(), Encoding.UTF8);
            }
        }
        catch
        {
            // 忽略日志写入失败，避免递归异常
        }
    }

    public static void LogInfo(string message)
    {
        try
        {
            var logFileName = $"info_{DateTime.Now:yyyy-MM-dd}.log";
            var logFilePath = Path.Combine(LogDirectory, logFileName);
            
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}{Environment.NewLine}";

            lock (LockObject)
            {
                File.AppendAllText(logFilePath, logEntry, Encoding.UTF8);
            }
        }
        catch
        {
            // 忽略日志写入失败
        }
    }
}
