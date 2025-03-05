using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BEAM.Analysis;
using BEAM.Datatypes;
using BEAM.Docking;
using BEAM.Models.Log;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using BEAM.Views;
using NP.Utilities;


namespace BEAM.ViewModels;

/// <summary>
/// View model controlling the inspection dock.
/// </summary>
public partial class InspectionViewModel : ViewModelBase, IDockBase
{
    [ObservableProperty] private Plot? _currentPlot;

    private byte _analysisProgress;

    public byte AnalysisProgress
    {
        get => _analysisProgress;
        set
        {
            _analysisProgress = value;
            OnPropertyChanged();
        }
    }

    private AnalysisProgressWindow ProgressWindow { get; set; }
    
    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _analysisRunning;
    
    private bool KeepData { get; set; }


    private SequenceViewModel _currentSequenceViewModel;
    private Analysis.Analysis _currentAnalysis;
    public ObservableCollection<SequenceViewModel> ExistingSequenceViewModels { get; } = [];

    /// <summary>
    /// Used for display of all options of AnalysisLists.
    /// </summary>
    public static List<Analysis.Analysis> AnalysisList { get; } = Analysis.Analysis.GetAllAnalysis();
    // [
    //     new PixelAnalysisChannel(),
    //     new RegionAnalysisStandardDeviationOfChannels(),
    //     new RegionAnalysisAverageOfChannels()
    // ];

    public InspectionViewModel(SequenceViewModel sequenceViewModel, DockingViewModel dock)
    {
        _currentAnalysis = Analysis.Analysis.GetAnalysis(0);
        _currentSequenceViewModel = sequenceViewModel;
        ProgressWindow = new AnalysisProgressWindow(this);
        dock.Items.CollectionChanged += DockingItemsChanged;

        foreach (var item in dock.Items)
        {
            if (item is SequenceViewModel model)
            {
                ExistingSequenceViewModels.Add(model);
            }
        }
    }


    public string Name { get; } = "Inspection Window";
    public void OnClose()
    {
        _currentSequenceViewModel.UnregisterInspectionViewModel(this);
        
        if (!_analysisRunning) return;
        AbortAnalysis();
        ProgressWindow.Close();
    }

    /// <summary>
    /// When the user interacted with the view, the coordinates of where the
    /// pointer was pressed and released, are passed to this method.
    ///
    /// Does not update, if KeepData is activated or an analysis is already running.
    /// </summary>
    public void Update(Coordinate2D pressedPoint, Coordinate2D releasedPoint)
    {
        if (KeepData) return;
        if (_analysisRunning)
        {
            Models.Log.Logger.GetInstance().Warning(LogEvent.Analysis, "Analysis is already running.");
            return;
        }
        
        StartAnalysis(pressedPoint, releasedPoint);
    }

    private void StartAnalysis(Coordinate2D pressedPoint, Coordinate2D releasedPoint)
    {
        // Only display progress for more than 100 Pixels analysed.
        if (AmountPixels(pressedPoint, releasedPoint) > 100)
        {
            ProgressWindow = new AnalysisProgressWindow(this);
            ProgressWindow.Show();
        }
        _analysisRunning = true;
        _currentAnalysis.Analyze(pressedPoint, releasedPoint, _currentSequenceViewModel.Sequence,
            this, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Calculate the amount of pixels encompassed by the rectangle parallel to the axes with these corners. 
    /// </summary>
    /// <param name="pressedPoint"></param>
    /// <param name="releasedPoint"></param>
    /// <returns></returns>
    private static double AmountPixels(Coordinate2D pressedPoint, Coordinate2D releasedPoint)
    {
        return Math.Abs((pressedPoint.Column - releasedPoint.Column) * (pressedPoint.Row - releasedPoint.Row));
    }

    /// <summary>
    /// Stops the currently running Analysis
    /// </summary>
    public void AbortAnalysis()
    {
        if (!_analysisRunning) return;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        AnalysisEnded();
    }

    /// <summary>
    /// Called when the analysis finished successfully.
    /// </summary>
    public void AnalysisEnded()
    {
        _analysisRunning = false;
        AnalysisProgress = 0;
        if (ProgressWindow.IsVisible)
        {
            ProgressWindow.Close();
        }
    }


    /// <summary>
    /// Creates a new Inspection window
    /// </summary>
    [RelayCommand]
    public void Clone()
    {
        _currentSequenceViewModel.OpenInspectionViewCommand.Execute(null);
    }

    /// <summary>
    /// When called, this method changes the currently used analysis method.
    /// </summary>
    /// <param name="index">The index of the new analysis mode</param>
    [RelayCommand]
    public Task ChangeAnalysis(int index)
    {
        if (ExistingSequenceViewModels.IsNullOrEmpty())
        {
            CurrentPlot = PlotCreator.CreatePlaceholderPlot();
            return Task.FromResult(false);
        }

        if (_analysisRunning)
        {
            AbortAnalysis();
        }
        _currentAnalysis = Analysis.Analysis.GetAnalysis((AnalysisTypes)index);
        return Task.CompletedTask;
    }

    /// <summary>
    /// This one will change the currently selected sequence to a new one.
    /// </summary>
    /// <param name="index">The index of the new sequence to be used</param>
    [RelayCommand]
    public void ChangeSequence(int index)
    {
        if (index < 0 || index >= ExistingSequenceViewModels.Count) return;
        _currentSequenceViewModel.UnregisterInspectionViewModel(this);
        _currentSequenceViewModel = ExistingSequenceViewModels[index];
        _currentSequenceViewModel.RegisterInspectionViewModel(this);
    }


    /// <summary>
    /// When the amount of docks registered by the DockingViewModel changes, this method will be called.
    /// If necessary, the list of existing SequenceVieModels will be updated.
    /// </summary>
    /// <param name="sender">the sender of the event</param>
    /// <param name="e">notification parameters</param>
    private void DockingItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is not SequenceViewModel sequenceViewModel) continue;
                ExistingSequenceViewModels.Add(sequenceViewModel);
                if (ExistingSequenceViewModels.Count == 1) SwitchToFirst();
            }

        }

        if (e.OldItems is null) return;
        {
            foreach (var item in e.OldItems)
            {
                if (item is not SequenceViewModel sequenceViewModel) continue;
                ExistingSequenceViewModels.Remove(sequenceViewModel);
                if (sequenceViewModel == _currentSequenceViewModel) SwitchToFirst();
            }
        }
    }

    /// <summary>
    /// This method will simply switch to the first sequence in the list of existing sequences.
    /// </summary>
    private void SwitchToFirst()
    {
        if (ExistingSequenceViewModels.Count == 0)
        {
            CurrentPlot = PlotCreator.CreatePlaceholderPlot();
            return;
        }
        _currentSequenceViewModel.UnregisterInspectionViewModel(this);
        _currentSequenceViewModel = ExistingSequenceViewModels[0];
        _currentSequenceViewModel.RegisterInspectionViewModel(this);
    }

    /// <summary>
    /// This method will update the new data acceptance.
    /// </summary>
    /// <param name="isChecked"></param>
    public void CheckBoxChanged(bool? isChecked)
    {
        KeepData = isChecked ?? false;
    }

    public int CurrentSequenceIndex()
    {
        return ExistingSequenceViewModels.FindIdx(_currentSequenceViewModel);
    }

    public int CurrentAnalysisIndex()
    {
        return (int)_currentAnalysis.GetAnalysisType();
    }

    public void Dispose()
    {
        if (CurrentPlot is null) return;
        CurrentPlot.Dispose();
        GC.SuppressFinalize(this);
        _cancellationTokenSource.Dispose();
    }
}