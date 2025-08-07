using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SemanticCode.ViewModels;

public class McpEditViewModel : ViewModelBase
{
    private string _projectName = "";
    private string _projectPath = "";
    private ProjectInfo? _currentProject;

    public ObservableCollection<SelectableMcpServerInfo> AvailableMcpServers { get; } = new();

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

    public Task Initialize(ProjectInfo project, IList<McpServerInfo> allMcpServers)
    {
        _currentProject = project;
        ProjectName = project.Name;
        ProjectPath = project.Path;

        AvailableMcpServers.Clear();
        
        foreach (var server in allMcpServers)
        {
            var isCurrentlyUsed = project.McpServers.Any(s => 
                s.Name == server.Name && 
                s.Type == server.Type && 
                s.Url == server.Url && 
                s.Command == server.Command);

            var selectableServer = new SelectableMcpServerInfo
            {
                Name = server.Name,
                Type = server.Type,
                Url = server.Url,
                Command = server.Command,
                IsSelected = isCurrentlyUsed,
                IsCurrentlyUsed = isCurrentlyUsed
            };

            AvailableMcpServers.Add(selectableServer);
        }

        return Task.CompletedTask;
    }

    public async Task SaveChanges()
    {
        if (_currentProject == null) return;

        try
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var claudeConfigPath = Path.Combine(userProfile, ".claude.json");

            if (!File.Exists(claudeConfigPath))
                return;

            var json = await File.ReadAllTextAsync(claudeConfigPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Create a mutable dictionary from the JSON
            var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (configDict == null) return;

            // Get projects dictionary
            if (!configDict.TryGetValue("projects", out var projectsObj) || 
                projectsObj is not JsonElement projectsElement)
                return;

            var projectsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(projectsElement.GetRawText());
            if (projectsDict == null) return;

            // Find the current project
            if (!projectsDict.TryGetValue(_currentProject.Path, out var projectObj) || 
                projectObj is not JsonElement projectElement)
                return;

            var projectDict = JsonSerializer.Deserialize<Dictionary<string, object>>(projectElement.GetRawText());
            if (projectDict == null) return;

            // Update MCP servers
            var selectedServers = AvailableMcpServers.Where(s => s.IsSelected).ToList();
            var mcpServersDict = new Dictionary<string, object>();

            foreach (var server in selectedServers)
            {
                var serverDict = new Dictionary<string, object>();
                
                if (!string.IsNullOrEmpty(server.Type))
                    serverDict["type"] = server.Type;
                if (!string.IsNullOrEmpty(server.Url))
                    serverDict["url"] = server.Url;
                if (!string.IsNullOrEmpty(server.Command))
                    serverDict["command"] = server.Command;

                mcpServersDict[server.Name] = serverDict;
            }

            projectDict["mcpServers"] = mcpServersDict;
            projectsDict[_currentProject.Path] = projectDict;
            configDict["projects"] = projectsDict;

            // Save back to file
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var updatedJson = JsonSerializer.Serialize(configDict, options);
            await File.WriteAllTextAsync(claudeConfigPath, updatedJson);

            // Update the current project's MCP servers in memory
            _currentProject.McpServers.Clear();
            foreach (var server in selectedServers)
            {
                _currentProject.McpServers.Add(new McpServerInfo
                {
                    Name = server.Name,
                    Type = server.Type,
                    Url = server.Url,
                    Command = server.Command
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving MCP changes: {ex.Message}");
        }
    }
}

public class SelectableMcpServerInfo : ReactiveObject
{
    private bool _isSelected;

    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Url { get; set; } = "";
    public string Command { get; set; } = "";
    public bool IsCurrentlyUsed { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }
}