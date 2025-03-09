using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using BEAM.Datatypes;
using BEAM.Docking;
using BEAM.Image.Minimap.Utility;
using BEAM.ImageSequence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BEAM.Renderer;
using BEAM.Models.Log;
using BEAM.Renderer.Attributes;
using BEAM.Views.Minimap.Popups;


namespace BEAM.ViewModels;

/// <summary>
/// View model controlling the view of a single sequence.
/// Manages sequence rendering, minimap generation, and inspection view coordination.
/// </summary>
public partial class SequenceViewModel : ViewModelBase, IDockBase
{
    public const int DefaultSequenceRangeLowerBound = 0;
    public const int DefaultSequenceRangeUpperBound = 1;
    public event EventHandler<CloseEventArgs> CloseEvent = delegate { };

    /// <summary>
    /// View model for handling docking functionality.
    /// </summary>
    [ObservableProperty] public partial DockingViewModel DockingVm { get; set; } = new();
    /// <summary>
    /// The position where the pointer was initially pressed.
    /// </summary>
    [ObservableProperty] public partial Coordinate2D PressedPointerPosition { get; set; } = new();
    /// <summary>
    /// The position where the pointer was released.
    /// </summary>
    [ObservableProperty] public partial Coordinate2D ReleasedPointerPosition { get; set; } = new();

    /// <summary>
    /// Collection of inspection view models connected to this sequence.
    /// </summary>
    private List<InspectionViewModel> _connectedInspectionViewModels = new();

    /// <summary>
    /// Event raised when renderers are updated.
    /// </summary>
    public EventHandler<RenderersUpdatedEventArgs> RenderersUpdated = delegate { };
    /// <summary>
    /// Event raised when the sequence is cut.
    /// </summary>
    public EventHandler<RenderersUpdatedEventArgs> CutSequence = delegate { };

    /// <summary>
    /// The sequence being displayed and managed.
    /// </summary>
    public TransformedSequence Sequence { get; set; }

    /// <summary>
    /// Available renderers for the sequence.
    /// </summary>
    public SequenceRenderer[] Renderers;
    
    /// <summary>
    /// Index of the currently selected renderer.
    /// </summary>
    public int RendererSelection;


    private Image.Minimap.Minimap? _currentMinimap;
    /// <summary>
    /// Event raised when the minimap has been changed.
    /// </summary>
    public EventHandler<EventArgs> MinimapHasChanged = delegate { };

    

    /// <summary>
    /// Collection of minimap view models.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<ViewModelBase> MinimapVms { get; set; } =
        new ObservableCollection<ViewModelBase>();

    /// <summary>
    /// Initializes a new instance of the SequenceViewModel.
    /// Sets up renderers based on the sequence type.
    /// </summary>
    /// <param name="sequence">The sequence to be visualized.</param>
    /// <param name="dockingVm">The docking view model instance.</param>
    public SequenceViewModel(ISequence sequence, DockingViewModel dockingVm)
    {
        Sequence = new TransformedSequence(sequence);
        DockingVm = dockingVm;

        DockingVm = dockingVm;
        int min = 0;
        int max = 1;
        ValueRangeAttribute range;
        try
        {
            range = sequence.GetType().GetCustomAttributes<ValueRangeAttribute>().First();
        }
        catch (Exception ex) when (ex is TargetInvocationException or ReflectionTypeLoadException or IndexOutOfRangeException or InvalidOperationException)
        {
            range = new ValueRangeAttribute(DefaultSequenceRangeLowerBound, DefaultSequenceRangeUpperBound);
        }

        min = range.Min;
        max = range.Max;

        RendererEnum selectedOption;
        try
        {
            selectedOption = sequence.GetType().GetCustomAttributes<RendererAttribute>().First().DefaultRenderer;
        }
        catch (Exception ex) when (ex is TargetInvocationException or ReflectionTypeLoadException or IndexOutOfRangeException or InvalidOperationException)
        {
            selectedOption= RendererEnum.HeatMapRendererRB;
        }

        RendererSelection = (int)selectedOption;
        var rendererOptions = Enum.GetValues(typeof(RendererEnum));
        Renderers = new SequenceRenderer[rendererOptions.Length];
        for(var i = 0; i < rendererOptions.Length; i++)
        {
            Renderers[i] = ((RendererEnum)((rendererOptions.GetValue(i)) ?? RendererEnum.ChannelMapRenderer)).Sequence(min, max);
        }
        
        _currentMinimap = SettingsUtilityHelper<Image.Minimap.Minimap>.GetDefaultClones().Active;
        if (_currentMinimap is not null)
        {
            if (RendererSelection < Renderers.Length && RendererSelection >= 0)
            {
                _currentMinimap.SetRenderer(Renderers[RendererSelection]);
                _currentMinimap.StartGeneration(sequence, OnMinimapGenerated);
            }
        }
        
    }
    

    /// <summary>
    /// Registers an inspection view model to receive updates.
    /// </summary>
    /// <param name="inspectionViewModel">The inspection view model to register.</param>
    public void RegisterInspectionViewModel(InspectionViewModel inspectionViewModel)
    {
        _connectedInspectionViewModels.Add(inspectionViewModel);
        inspectionViewModel.Update(
            _correctInvalid(PressedPointerPosition.OffsetBy(0.5, 0.5)), 
            _correctInvalid(ReleasedPointerPosition.OffsetBy(0.5, 0.5)));
    }
    
    /// <summary>
    /// Unregisters an inspection view model from updates.
    /// </summary>
    /// <param name="inspectionViewModel">The inspection view model to unregister.</param>
    public void UnregisterInspectionViewModel(InspectionViewModel inspectionViewModel)
    {
        _connectedInspectionViewModels.Remove(inspectionViewModel);
    }

    /// <summary>
    /// Updates all registered inspection view models with current pointer positions.
    /// </summary>
    [RelayCommand]
    public void UpdateInspectionViewModel()
    {
        Coordinate2D pointPressed = _correctInvalid(PressedPointerPosition.OffsetBy(0.5, 0.5));
        Coordinate2D pointReleased = _correctInvalid(ReleasedPointerPosition.OffsetBy(0.5, 0.5));
        foreach (var inspectionViewModel in _connectedInspectionViewModels)
        {
            inspectionViewModel.Update(pointPressed, pointReleased);
        }
    }

    /// <summary>
    /// Ensures coordinate points are within valid sequence bounds.
    /// </summary>
    /// <param name="point">The point to validate.</param>
    /// <returns>A corrected coordinate within sequence bounds.</returns>
    private Coordinate2D _correctInvalid(Coordinate2D point)
    {
        double x = point.Column;
        double y = point.Row;
        
        if(x < 0)
            x = 0;
        else if (x >= Sequence.Shape.Width)
            x = Sequence.Shape.Width - 1;
        if(y < 0)
            y = 0;
        else if(y >= Sequence.Shape.Height)
            y = Sequence.Shape.Height - 1;
        
        return new Coordinate2D(y, x);
    }

    public string Name => Sequence.GetName();

    /// <summary>
    /// Gets the current sequence renderer.
    /// </summary>
    public SequenceRenderer CurrentRenderer => Renderers[RendererSelection];

    public void Dispose()
    {
        Sequence.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Changes the current minimap and initiates generation.
    /// </summary>
    /// <param name="minimap">The new minimap to use.</param>
    public void ChangeCurrentMinimap(Image.Minimap.Minimap minimap)
    {
        if (_currentMinimap is not null)
        {
            _currentMinimap.StopGeneration();
        }
        
        MinimapVms.Clear();
        _currentMinimap = minimap;
        _currentMinimap.SetRenderer(Renderers[RendererSelection]);
        _currentMinimap.StartGeneration(Sequence, OnMinimapGenerated);
    }

   
    /// <summary>
    /// Opens the minimap settings popup.
    /// </summary> 
    [RelayCommand]
    public void OpenMinimapSettings()
    {
        DefaultMinimapPopupView minimapPopup = new(this);
        var v = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        if (v is null || v.MainWindow is null)
        {
            Logger.GetInstance().Error(LogEvent.Critical, "Unable to find ApplicationLifetime or MainWindow");
            return;
        }

        minimapPopup.ShowDialog(v.MainWindow);
    }
    
    /// <summary>
    /// Handles minimap generation completion and updates the UI.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments containing the generated minimap.</param>
    public void OnMinimapGenerated(object sender, MinimapGeneratedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Clear the existing minimap controls
            MinimapVms.Clear();
            
            var newMinimapVm = e.Minimap.GetDisplayableViewModel();
            MinimapVms.Add(newMinimapVm);
            MinimapHasChanged(this, EventArgs.Empty);
        });
    }

    /// <summary>
    /// Opens a new inspection view for the sequence.
    /// </summary>
    [RelayCommand]
    public void OpenInspectionView()
    {
        InspectionViewModel inspectionViewModel = new InspectionViewModel(this, DockingVm);
        _connectedInspectionViewModels.Add(inspectionViewModel);
        DockingVm.OpenDock(inspectionViewModel);
        
        inspectionViewModel.Update(
            _correctInvalid(PressedPointerPosition.OffsetBy(0.5, 0.5)), 
            _correctInvalid(ReleasedPointerPosition.OffsetBy(0.5, 0.5)));
    }
    

    /// <summary>
    /// Performs cleanup when closing the view model.
    /// </summary>
    public void OnClose()
    {
        CloseEvent.Invoke(this, new CloseEventArgs());
        _currentMinimap?.StopGeneration();
    }
    
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    /// Disables and clears the current minimap.
    /// </summary>
    public void DisableMinimap()
    {
        _currentMinimap?.StopGeneration();
        MinimapVms.Clear();
    }
}

public class CloseEventArgs
{
}