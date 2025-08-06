using System;
using System.IO;
using System.Text.Json;

namespace SemanticCode.Services;

public class UpdateConfigService
{
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
        "SemanticCode", 
        "update-config.json");
    
    public class UpdateConfig
    {
        public string IgnoredVersion { get; set; } = string.Empty;
        public DateTime LastCheckTime { get; set; } = DateTime.MinValue;
    }
    
    public UpdateConfig LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                return JsonSerializer.Deserialize<UpdateConfig>(json) ?? new UpdateConfig();
            }
        }
        catch (Exception)
        {
            // 忽略读取错误
        }
        
        return new UpdateConfig();
    }
    
    public void SaveConfig(UpdateConfig config)
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
            
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception)
        {
            // 忽略保存错误
        }
    }
    
    public void IgnoreVersion(string version)
    {
        var config = LoadConfig();
        config.IgnoredVersion = version;
        SaveConfig(config);
    }
    
    public bool IsVersionIgnored(string version)
    {
        var config = LoadConfig();
        return config.IgnoredVersion == version;
    }
}