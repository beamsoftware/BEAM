using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BEAM.ViewModels;

namespace BEAM.Views;

/// <summary>
/// Displays the progress of an analysis and allows aborting a currently running analysis.
/// </summary>
public partial class AnalysisProgressWindow : Window
{
    public AnalysisProgressWindow(InspectionViewModel inspectionViewModel)
    {
        InitializeComponent();
        DataContext = inspectionViewModel;
    }
    
    /// <summary>
    /// Handles the close event to close the window.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void Close(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}