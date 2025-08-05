using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SemanticCode.Services;

namespace SemanticCode.Services;

public class AgentDirectoryService
{
    private readonly AgentFileParser _parser;
    private readonly string _agentsDirectory;

    public AgentDirectoryService()
    {
        _parser = new AgentFileParser();
        _agentsDirectory = GetAgentsDirectory();
        
        // 确保目录存在
        if (!Directory.Exists(_agentsDirectory))
        {
            try
            {
                Directory.CreateDirectory(_agentsDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating agents directory: {ex.Message}");
            }
        }
    }

    private string GetAgentsDirectory()
    {
        // 获取当前用户目录
        var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userHome, ".claude", "agents");
    }

    public List<AgentFileParser.AgentInfo> LoadAllAgents()
    {
        var agents = new List<AgentFileParser.AgentInfo>();

        try
        {
            if (!Directory.Exists(_agentsDirectory))
            {
                return agents;
            }

            var mdFiles = Directory.GetFiles(_agentsDirectory, "*.md", SearchOption.TopDirectoryOnly);
            
            foreach (var file in mdFiles)
            {
                var agentInfo = _parser.ParseAgentFile(file);
                if (agentInfo != null)
                {
                    agents.Add(agentInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading agents: {ex.Message}");
        }

        return agents;
    }

    public AgentFileParser.AgentInfo? LoadAgent(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_agentsDirectory, fileName);
            return _parser.ParseAgentFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading agent {fileName}: {ex.Message}");
            return null;
        }
    }

    public bool SaveAgent(AgentFileParser.AgentInfo agentInfo)
    {
        try
        {
            if (string.IsNullOrEmpty(agentInfo.FileName))
            {
                // 生成文件名
                var safeName = string.Join("_", agentInfo.Name.Split(Path.GetInvalidFileNameChars()));
                agentInfo.FileName = $"{safeName}.md";
            }
            
            agentInfo.FilePath = Path.Combine(_agentsDirectory, agentInfo.FileName);
            return _parser.SaveAgentFile(agentInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving agent: {ex.Message}");
            return false;
        }
    }

    public bool DeleteAgent(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_agentsDirectory, fileName);
            return _parser.DeleteAgentFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting agent: {ex.Message}");
            return false;
        }
    }

    public string GetAgentsDirectoryPath()
    {
        return _agentsDirectory;
    }

    public bool AgentExists(string fileName)
    {
        var filePath = Path.Combine(_agentsDirectory, fileName);
        return File.Exists(filePath);
    }

    public List<string> GetAgentFileNames()
    {
        try
        {
            if (!Directory.Exists(_agentsDirectory))
            {
                return new List<string>();
            }

            return Directory.GetFiles(_agentsDirectory, "*.md", SearchOption.TopDirectoryOnly)
                          .Select(Path.GetFileName)
                          .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting agent file names: {ex.Message}");
            return new List<string>();
        }
    }
}