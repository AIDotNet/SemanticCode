using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SemanticCode.ViewModels;

public class SessionHistoryViewModel : ViewModelBase
{
    private string _projectName = "";
    private string _projectPath = "";
    private bool _isLoading;

    public ObservableCollection<SessionRecord> SessionRecords { get; } = new();

    public string ProjectName
    {
        get => _projectName;
        set => this.RaiseAndSetIfChanged(ref _projectName, value);
    }

    public string ProjectPath
    {
        get => _projectPath;
        set => this.RaiseAndSetIfChanged(ref _projectPath, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public ICommand RefreshCommand { get; }
    public SessionHistoryViewModel()
    {
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadSessionHistory);
    }

    public async Task Initialize(string projectName, string projectPath)
    {
        ProjectName = projectName;
        ProjectPath = projectPath;
        await LoadSessionHistory();
    }

    private async Task LoadSessionHistory()
    {
        IsLoading = true;
        SessionRecords.Clear();

        try
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Convert project path: replace / with --
            var convertedPath = ProjectPath.Replace("/", "-").Replace("\\", "-").Replace(":", "-").Replace(".", "-");
            if (convertedPath.StartsWith("-"))
                convertedPath = convertedPath.Substring(1);

            var projectDirectory = Path.Combine(userProfile, ".claude", "projects", convertedPath);

            if (!Directory.Exists(projectDirectory))
            {
                IsLoading = false;
                return;
            }

            // Get all session files in the project directory
            var sessionFiles = Directory.GetFiles(projectDirectory, "*.jsonl")
                .OrderByDescending(File.GetLastWriteTime)
                .ToList();

            int index = 1;
            foreach (var sessionFile in sessionFiles)
            {
                var fileName = Path.GetFileName(sessionFile);
                var sessionId = Path.GetFileNameWithoutExtension(fileName);

                try
                {
                    var lines = await File.ReadAllLinesAsync(sessionFile);
                    var userMessages = new List<SessionMessage>();

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            using var doc = JsonDocument.Parse(line);
                            var root = doc.RootElement;

                            if (root.TryGetProperty("type", out var typeElement))
                            {
                                var messageType = typeElement.GetString();

                                // Handle user messages
                                if (messageType == "user" && root.TryGetProperty("message", out var messageElement))
                                {
                                    var content = ExtractMessageContent(messageElement);
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        DateTime timestamp = DateTime.MinValue;
                                        if (root.TryGetProperty("timestamp", out var timestampElement))
                                        {
                                            DateTime.TryParse(timestampElement.GetString(), out timestamp);
                                        }

                                        userMessages.Add(new SessionMessage
                                        {
                                            Content = content,
                                            Timestamp = timestamp,
                                            Type = "user"
                                        });
                                    }
                                }
                                // Handle assistant messages
                                else if (messageType == "assistant" && root.TryGetProperty("message", out var assistantMessageElement))
                                {
                                    var content = ExtractAssistantMessageContent(assistantMessageElement);
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        DateTime timestamp = DateTime.MinValue;
                                        if (root.TryGetProperty("timestamp", out var timestampElement))
                                        {
                                            DateTime.TryParse(timestampElement.GetString(), out timestamp);
                                        }

                                        userMessages.Add(new SessionMessage
                                        {
                                            Content = content,
                                            Timestamp = timestamp,
                                            Type = "assistant"
                                        });
                                    }
                                }
                                // Handle system messages
                                else if (messageType == "system" && root.TryGetProperty("message", out var systemMessageElement))
                                {
                                    var content = ExtractMessageContent(systemMessageElement);
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        DateTime timestamp = DateTime.MinValue;
                                        if (root.TryGetProperty("timestamp", out var timestampElement))
                                        {
                                            DateTime.TryParse(timestampElement.GetString(), out timestamp);
                                        }

                                        userMessages.Add(new SessionMessage
                                        {
                                            Content = content,
                                            Timestamp = timestamp,
                                            Type = "system"
                                        });
                                    }
                                }
                            }
                        }
                        catch (JsonException)
                        {
                            // Skip invalid JSON lines
                            continue;
                        }
                    }

                    // Create session records from user messages
                    foreach (var message in userMessages.OrderBy(m => m.Timestamp))
                    {
                        var sessionRecord = new SessionRecord
                        {
                            Index = index++,
                            Display = FormatDisplayText(message.Content, message.Type, 100),
                            Timestamp = message.Timestamp == DateTime.MinValue
                                ? File.GetLastWriteTime(sessionFile)
                                : message.Timestamp,
                            HasPastedContent = DetectPastedContent(message.Content),
                            SessionId = sessionId,
                            MessageType = message.Type
                        };

                        SessionRecords.Add(sessionRecord);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading session file {sessionFile}: {ex.Message}");
                    continue;
                }
            }

            // Sort by timestamp descending (most recent first)
            var sortedRecords = SessionRecords.OrderByDescending(x => x.Timestamp).ToList();
            SessionRecords.Clear();

            // Re-index after sorting
            index = 1;
            foreach (var record in sortedRecords)
            {
                record.Index = index++;
                SessionRecords.Add(record);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading session history: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }

    private static string ExtractMessageContent(JsonElement messageElement)
    {
        try
        {
            // Handle string content directly
            if (messageElement.ValueKind == JsonValueKind.String)
            {
                return messageElement.GetString() ?? "";
            }

            // Handle content property (simple text)
            if (messageElement.TryGetProperty("content", out var contentElement))
            {
                if (contentElement.ValueKind == JsonValueKind.String)
                {
                    return contentElement.GetString() ?? "";
                }
                
                // Handle array of content items
                if (contentElement.ValueKind == JsonValueKind.Array)
                {
                    var textContent = "";
                    foreach (var item in contentElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("type", out var typeElement) && 
                            typeElement.GetString() == "text" &&
                            item.TryGetProperty("text", out var textElement))
                        {
                            textContent += textElement.GetString() + " ";
                        }
                    }
                    return textContent.Trim();
                }
            }

            // Handle role-based message structure
            if (messageElement.TryGetProperty("role", out var roleElement))
            {
                if (messageElement.TryGetProperty("content", out var roleContentElement))
                {
                    return ExtractMessageContent(roleContentElement);
                }
            }

            return "";
        }
        catch (Exception)
        {
            return "";
        }
    }

    private static string ExtractAssistantMessageContent(JsonElement messageElement)
    {
        try
        {
            // Handle assistant message format with content array
            if (messageElement.TryGetProperty("content", out var contentElement))
            {
                if (contentElement.ValueKind == JsonValueKind.Array)
                {
                    var textContent = "";
                    foreach (var item in contentElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("type", out var typeElement))
                        {
                            var type = typeElement.GetString();
                            
                            // Handle text content
                            if (type == "text" && item.TryGetProperty("text", out var textElement))
                            {
                                textContent += textElement.GetString() + " ";
                            }
                            // Handle tool use
                            else if (type == "tool_use" && item.TryGetProperty("name", out var nameElement))
                            {
                                textContent += $"[Tool: {nameElement.GetString()}] ";
                            }
                        }
                    }
                    return textContent.Trim();
                }
                else if (contentElement.ValueKind == JsonValueKind.String)
                {
                    return contentElement.GetString() ?? "";
                }
            }

            return ExtractMessageContent(messageElement);
        }
        catch (Exception)
        {
            return "";
        }
    }

    private static string FormatDisplayText(string content, string messageType, int maxLength)
    {
        var prefix = messageType switch
        {
            "user" => "[User] ",
            "assistant" => "[Assistant] ",
            "system" => "[System] ",
            _ => ""
        };

        var text = content;
        if (prefix.Length + text.Length <= maxLength)
            return prefix + text;

        var availableLength = maxLength - prefix.Length - 3; // 3 for "..."
        if (availableLength <= 0)
            return prefix + "...";

        return prefix + text.Substring(0, availableLength) + "...";
    }

    private static bool DetectPastedContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        // Heuristics for detecting pasted content:
        // 1. Very long content (more than 500 characters)
        // 2. Contains multiple newlines
        // 3. Contains code patterns (function definitions, imports, etc.)
        // 4. Contains file paths
        
        if (content.Length > 500)
            return true;

        var newlineCount = content.Count(c => c == '\n');
        if (newlineCount > 5)
            return true;

        // Check for common code patterns
        var codePatterns = new[]
        {
            "function ", "def ", "class ", "import ", "require(", "using ",
            "public class", "private ", "protected ", "#!/", "#include",
            "<?php", "function(", "const ", "let ", "var ", "export "
        };

        if (codePatterns.Any(pattern => content.Contains(pattern)))
            return true;

        // Check for file paths
        if (content.Contains("\\") && content.Contains(".") && 
            (content.Contains("C:") || content.Contains("/")))
            return true;

        return false;
    }

}

public class SessionRecord : ReactiveObject
{
    public int Index { get; set; }
    public string Display { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public bool HasPastedContent { get; set; }
    public string SessionId { get; set; } = "";
    public string MessageType { get; set; } = "";
}

public class SessionMessage
{
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = "";
}