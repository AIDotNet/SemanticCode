using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SemanticCode.Pages;

public partial class ClaudeCodeSettingsView : UserControl
{
    public ClaudeCodeSettingsView()
    {
        InitializeComponent();
    }

    private void BaseUrlComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // ComboBox selection is already handled by binding
    }

    private void ModelComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // ComboBox selection is already handled by binding
    }

    private void SmallFastModelComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // ComboBox selection is already handled by binding
    }

    private void ComboBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            comboBox.IsDropDownOpen = !comboBox.IsDropDownOpen;
        }
    }
}