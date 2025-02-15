using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Avalonia;
using BEAM.Datatypes;
using BEAM.Docking;
using BEAM.IMage.Displayer.Scottplot;
using BEAM.ImageSequence;
using BEAM.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot;
using Svg;
using BEAM.Renderer;

namespace BEAM.ViewModels;

public partial class SequenceViewModel : ViewModelBase, IDockBase
{
    [ObservableProperty] public partial DockingViewModel DockingVm { get; set; } = new();
    [ObservableProperty] public partial Coordinate2D pressedPointerPosition { get; set; } = new();
    [ObservableProperty] public partial Coordinate2D releasedPointerPosition { get; set; } = new();
    
    private List<InspectionViewModel> _ConnectedInspectionViewModels = new();

    public EventHandler<RenderersUpdatedEventArgs> RenderersUpdated = delegate { };
    public EventHandler<RenderersUpdatedEventArgs> CutSequence = delegate { };

    public TransformedSequence Sequence { get; set; }

    public SequenceRenderer[] Renderers;
    public int RendererSelection;


    public SequenceViewModel(ISequence sequence, DockingViewModel dockingVm)
    {
        Sequence = new TransformedSequence(sequence);
        DockingVm = dockingVm;

        var (min, max) = sequence switch
        {
            SkiaSequence => (0, 255),
            _ => (0, 1)
        };

        Renderers =
        [
            new ChannelMapRenderer(min, max, 2, 1, 0),
            new HeatMapRendererRB(min, max, 0, 0.1, 0.9),
            new ArgMaxRendererGrey(min, max)
        ];

        RendererSelection = sequence switch
        {
            SkiaSequence => 0,
            _ => 1
        };
    }
    
    public void RegisterInspectionViewModel(InspectionViewModel inspectionViewModel)
    {
        _ConnectedInspectionViewModels.Add(inspectionViewModel);
    }
    
    public void UnregisterInspectionViewModel(InspectionViewModel inspectionViewModel)
    {
        _ConnectedInspectionViewModels.Remove(inspectionViewModel);
    }

    [RelayCommand]
    public async Task UpdateInspectionViewModel()
    {
        Console.WriteLine("Rectangle is detected:" + pressedPointerPosition.ToString() + ", " + releasedPointerPosition.ToString());
        
        foreach (var inspectionViewModel in _ConnectedInspectionViewModels)
        {
            inspectionViewModel.Update(pressedPointerPosition, releasedPointerPosition);
        }
    }

    [RelayCommand]
    public async Task OpenInspectionView()
    {
        InspectionViewModel inspectionViewModel = new InspectionViewModel(this);
        _ConnectedInspectionViewModels.Add(inspectionViewModel);
        DockingVm.OpenDock(inspectionViewModel);
        inspectionViewModel.Update(pressedPointerPosition, releasedPointerPosition);
    }
    
    
    private Coordinate2D _correctInvalid(Coordinate2D point)
    {
        double x = point.Row;
        double y = point.Column;
        
        if(x < 0)
            x = 0;
        else if (x > Sequence.Shape.Width)
            x = Sequence.Shape.Width;
        if(y < 0)
            y = 0;
        else if(y > Sequence.Shape.Height)
            y = Sequence.Shape.Height;
        return new Coordinate2D(x, y);
    }

    public string Name => Sequence.GetName();

    public SequenceRenderer CurrentRenderer => Renderers[RendererSelection];
}