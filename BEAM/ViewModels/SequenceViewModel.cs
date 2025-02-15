using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Avalonia;
using BEAM.Datatypes;
using BEAM.Docking;
using BEAM.ImageSequence;
using BEAM.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot;
using Svg;

namespace BEAM.ViewModels;

public partial class SequenceViewModel : ViewModelBase, IDockBase
{
    [ObservableProperty] public partial DockingViewModel DockingVm { get; set; } = new();
    [ObservableProperty] public partial Coordinate2D pressedPointerPosition { get; set; } = new();
    [ObservableProperty] public partial Coordinate2D releasedPointerPosition { get; set; } = new();


    public Sequence Sequence { get; }

    private List<InspectionViewModel> _ConnectedInspectionViewModels = new();

    public SequenceViewModel(Sequence sequence, DockingViewModel dockingVm)
    {
        Sequence = sequence;
        DockingVm = dockingVm;
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
    public async Task UpdateInspectionViewModel(Coordinate2D point)
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
    
    public string Name { get; } =  DateTime.Now.Microsecond.ToString();

    public override string ToString()
    {
        return Name;
    }
}