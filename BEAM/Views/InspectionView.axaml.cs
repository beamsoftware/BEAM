using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BEAM.ViewModels;
using ScottPlot;

namespace BEAM.Views;

/// <summary>
/// Code behind the inspection view.
/// </summary>
// ReSharper disable once UnusedType.Global
public partial class InspectionView : UserControl
{
    public InspectionView()
    {
        InitializeComponent();
        DataContextChanged += DataContextChangedHandling;
    }

    /// <summary>
    /// Updates the Plot with the given new plot.
    /// </summary>
    /// <param name="newPlot">The new plot to be displayed</param>
    private void FillPlot(Plot newPlot)
    {
        newPlot.PlotControl = AnalysisPlot;
        AnalysisPlot.Reset(newPlot);
        AnalysisPlot.Refresh();
    }

    private void DataContextChangedHandling(object? sender, EventArgs eventArgs)
    {
        if (DataContext is not InspectionViewModel vm) return;
        SequencePicker.SelectedIndex = vm.CurrentSequenceIndex();
        AnalysisPicker.SelectedIndex = vm.CurrentAnalysisIndex();
        vm.PropertyChanged += (_, e) =>
        {
            // Update the plot, if the plot changed and is not null
            if (e.PropertyName != nameof(InspectionViewModel.CurrentPlot)) return;
            if (vm.CurrentPlot != null) FillPlot(vm.CurrentPlot);
        };
        
        if (vm.CurrentPlot is null) return;
        FillPlot(vm.CurrentPlot);
    }

    public void CloneButton_Clicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not InspectionViewModel vm) return;
        vm.Clone();
    }

    public void AnalysisPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not InspectionViewModel vm || vm.CurrentPlot is null) return;
        Task result = vm.ChangeAnalysis(AnalysisPicker.SelectedIndex);
        if (result.IsCompletedSuccessfully)
        {
            FillPlot(vm.CurrentPlot);
        }
        else
        {
            AnalysisPicker.SelectedIndex = vm.CurrentAnalysisIndex();
        }
    }

    public void SequencePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not InspectionViewModel vm) return;
        vm.ChangeSequence(SequencePicker.SelectedIndex);
    }

    public void CheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (DataContext is not InspectionViewModel vm) return;
        vm.CheckBoxChanged(KeepDataCheckBox.IsChecked);
    }
}