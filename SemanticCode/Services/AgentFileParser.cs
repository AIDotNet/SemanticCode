using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SemanticCode.Services;

public class AgentFileParser
{
    public class AgentInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "default";
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PreContent { get; set; } = string.Empty; // 前置内容（第一个---之前）
        public string MainContent { get; set; } = string.Empty; // 主要内容（第二个---之后）
        public Dictionary<string, string> FrontMatter { get; set; } = new();
    }

    public AgentInfo? ParseAgentFile(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            var frontMatter = ExtractFrontMatter(content);
            
            if (!frontMatter.ContainsKey("name"))
            {
                return null;
            }

            var agentInfo = new AgentInfo
            {
                Name = frontMatter["name"],
                Description = frontMatter.ContainsKey("description") ? frontMatter["description"] : string.Empty,
                Color = frontMatter.ContainsKey("color") ? frontMatter["color"] : "default",
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                Content = content,
                FrontMatter = frontMatter
            };

            return agentInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing agent file {filePath}: {ex.Message}");
            return null;
        }
    }

    private Dictionary<string, string> ExtractFrontMatter(string content)
    {
        var result = new Dictionary<string, string>();
        
        // 查找第一个 --- 的位置
        var firstDashIndex = content.IndexOf("---");
        if (firstDashIndex == -1) return result;
        
        // 查找第二个 --- 的位置（从第一个之后开始）
        var secondDashIndex = content.IndexOf("---", firstDashIndex + 3);
        if (secondDashIndex == -1) return result;
        
        // 提取前置内容（第一个---之前）
        var preContent = content.Substring(0, firstDashIndex).Trim();
        if (!string.IsNullOrEmpty(preContent))
        {
            result["_precontent"] = preContent;
        }
        
        // 提取 front matter 内容（两个---之间）
        var frontMatterStart = firstDashIndex + 3;
        var frontMatterLength = secondDashIndex - frontMatterStart;
        var frontMatterText = content.Substring(frontMatterStart, frontMatterLength).Trim();
        
        // 解析 front matter
        var lines = frontMatterText.Split('\n');
        string currentKey = null;
        var currentValue = new List<string>();
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;
            
            // 检查是否是新的 key: value 行
            var colonIndex = trimmedLine.IndexOf(':');
            if (colonIndex > 0 && !trimmedLine.StartsWith(" "))
            {
                // 保存之前的键值对
                if (currentKey != null)
                {
                    result[currentKey] = string.Join("\n", currentValue).Trim();
                    currentValue.Clear();
                }
                
                // 开始新的键值对
                currentKey = trimmedLine.Substring(0, colonIndex).Trim();
                var value = trimmedLine.Substring(colonIndex + 1).Trim();
                
                // 去除值周围的引号
                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                
                if (!string.IsNullOrEmpty(value))
                {
                    currentValue.Add(value);
                }
            }
            else if (currentKey != null)
            {
                // 多行值的继续
                currentValue.Add(trimmedLine);
            }
        }
        
        // 保存最后一个键值对
        if (currentKey != null)
        {
            result[currentKey] = string.Join("\n", currentValue).Trim();
        }
        
        // 提取主要内容（第二个---之后）
        var mainContentStart = secondDashIndex + 3;
        if (mainContentStart < content.Length)
        {
            var mainContent = content.Substring(mainContentStart).Trim();
            if (!string.IsNullOrEmpty(mainContent))
            {
                result["_maincontent"] = mainContent;
            }
        }
        
        return result;
    }

    public bool SaveAgentFile(AgentInfo agentInfo)
    {
        try
        {
            var frontMatter = new List<string>
            {
                "---",
                $"name: {agentInfo.Name}",
                $"description: {agentInfo.Description}",
                $"color: {agentInfo.Color}"
            };
            
            // 添加其他 front matter 属性
            foreach (var kvp in agentInfo.FrontMatter)
            {
                if (kvp.Key != "name" && kvp.Key != "description" && kvp.Key != "color")
                {
                    frontMatter.Add($"{kvp.Key}: {kvp.Value}");
                }
            }
            
            frontMatter.Add("---");
            frontMatter.Add("");
            frontMatter.Add(agentInfo.Content);
            
            File.WriteAllText(agentInfo.FilePath, string.Join("\n", frontMatter));
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving agent file {agentInfo.FilePath}: {ex.Message}");
            return false;
        }
    }

    public bool DeleteAgentFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting agent file {filePath}: {ex.Message}");
            return false;
        }
    }
}