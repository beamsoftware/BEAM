// (c) Paul Stier, 2025

using BEAM.Renderer;
using BEAM.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.Controls;

public partial class HeatMapConfigControlViewModel(HeatMapRenderer renderer, SequenceViewModel model)
    : ViewModelBase, ISaveControl
{
    [ObservableProperty] public partial decimal Channel { get; set; } = renderer.Channel;
    [ObservableProperty] public partial decimal Min { get; set; } = 0;

    [ObservableProperty] public partial decimal Max { get; set; } = model.Sequence.Shape.Channels - 1;

    public void Save()
    {
        renderer.Channel = (int)Channel;
    }
}