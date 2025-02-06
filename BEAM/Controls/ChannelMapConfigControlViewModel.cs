// (c) Paul Stier, 2025

using BEAM.Renderer;
using BEAM.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.Controls;

public partial class ChannelMapConfigControlViewModel(ChannelMapRenderer renderer, SequenceViewModel model)
    : ViewModelBase, ISaveControl
{
    [ObservableProperty] public partial decimal ChannelRed { get; set; } = renderer.ChannelRed;
    [ObservableProperty] public partial decimal ChannelGreen { get; set; } = renderer.ChannelGreen;
    [ObservableProperty] public partial decimal ChannelBlue { get; set; } = renderer.ChannelBlue;

    [ObservableProperty] public partial decimal MinChannel { get; set; } = 0;
    [ObservableProperty] public partial decimal MaxChannel { get; set; } = model.Sequence.Shape.Channels - 1;

    public void Save()
    {
        renderer.ChannelRed = (int)ChannelRed;
        renderer.ChannelGreen = (int)ChannelGreen;
        renderer.ChannelBlue = (int)ChannelBlue;
    }
}