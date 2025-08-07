using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SemanticCode.ViewModels;
using System.Threading.Tasks;

namespace SemanticCode.Views;

public partial class SessionHistoryWindow : Window
{
    public SessionHistoryWindow()
    {
        InitializeComponent();
    }

    public static async Task ShowDialog(Window owner, string projectName, string projectPath)
    {
        var window = new SessionHistoryWindow();
        var viewModel = new SessionHistoryViewModel();
        
        window.DataContext = viewModel;
        await viewModel.Initialize(projectName, projectPath);
        
        await window.ShowDialog(owner);
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}