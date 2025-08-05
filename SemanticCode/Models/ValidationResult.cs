using System.Collections.Generic;
using System.Linq;

namespace SemanticCode.Models;

public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    
    public bool IsValid => !Errors.Any();
    public bool HasWarnings => Warnings.Any();
    
    public void AddError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            Errors.Add(error);
        }
    }
    
    public void AddWarning(string warning)
    {
        if (!string.IsNullOrWhiteSpace(warning))
        {
            Warnings.Add(warning);
        }
    }
    
    public string GetErrorMessage()
    {
        if (!Errors.Any())
            return string.Empty;
            
        return string.Join("\n", Errors);
    }
    
    public string GetWarningMessage()
    {
        if (!Warnings.Any())
            return string.Empty;
            
        return string.Join("\n", Warnings);
    }
    
    public string GetAllMessages()
    {
        var messages = new List<string>();
        
        if (Errors.Any())
        {
            messages.Add("错误:");
            messages.AddRange(Errors.Select(e => "• " + e));
        }
        
        if (Warnings.Any())
        {
            if (messages.Any())
                messages.Add("");
            messages.Add("警告:");
            messages.AddRange(Warnings.Select(w => "• " + w));
        }
        
        return string.Join("\n", messages);
    }
}