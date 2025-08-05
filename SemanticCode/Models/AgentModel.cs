using System;
using System.Collections.Generic;

namespace SemanticCode.Models;

public class AgentModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "default";
    public string Status { get; set; } = "活跃";
    public string Type { get; set; } = "通用";
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, string> FrontMatter { get; set; } = new();
    public Dictionary<string, string> Configuration { get; set; } = new();

    // 从 AgentFileParser.AgentInfo 转换
    public static AgentModel FromAgentInfo(Services.AgentFileParser.AgentInfo agentInfo)
    {
        return new AgentModel
        {
            Name = agentInfo.Name,
            Description = agentInfo.Description,
            Color = agentInfo.Color,
            FileName = agentInfo.FileName,
            FilePath = agentInfo.FilePath,
            Content = agentInfo.Content,
            FrontMatter = agentInfo.FrontMatter,
            Status = "活跃",
            Type = "通用",
            IsEnabled = true
        };
    }

    // 转换为 AgentFileParser.AgentInfo
    public SemanticCode.Services.AgentFileParser.AgentInfo ToAgentInfo()
    {
        return new SemanticCode.Services.AgentFileParser.AgentInfo
        {
            Name = Name,
            Description = Description,
            Color = Color,
            FileName = FileName,
            FilePath = FilePath,
            Content = Content,
            FrontMatter = FrontMatter
        };
    }
}