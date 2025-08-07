using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SemanticCode.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemanticCode.Views;

public partial class McpEditDialog : Window
{
    public McpEditDialog()
    {
        InitializeComponent();
    }

    public static async Task<bool> ShowDialog(Window owner, ProjectInfo project, IList<McpServerInfo> allMcpServers)
    {
        var dialog = new McpEditDialog();
        var viewModel = new McpEditViewModel();
        
        dialog.DataContext = viewModel;
        await viewModel.Initialize(project, allMcpServers);
        
        var result = await dialog.ShowDialog<bool>(owner);
        
        if (result)
        {
            await viewModel.SaveChanges();
        }
        
        return result;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}