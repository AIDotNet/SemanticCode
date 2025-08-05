using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SemanticCode.ViewModels;

namespace SemanticCode;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        
        var viewName = name.Replace("SemanticCode.ViewModels", "SemanticCode.Pages", StringComparison.Ordinal);
        var type = Type.GetType(viewName);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + viewName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}