using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SemanticCode.ViewModels;

namespace SemanticCode.Views;

public partial class UpdateNotificationDialog : Window
{
    public UpdateNotificationDialog()
    {
        InitializeComponent();
    }
    
    public UpdateNotificationDialog(UpdateNotificationViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}