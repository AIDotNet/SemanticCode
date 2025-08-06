using System.ComponentModel;
using Avalonia.Controls;

namespace SemanticCode.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Instead of closing, hide the window to system tray
        e.Cancel = true;
        this.Hide();
    }
}