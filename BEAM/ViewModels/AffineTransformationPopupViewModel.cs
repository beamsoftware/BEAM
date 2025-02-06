// (c) Paul Stier, 2025

using BEAM.Renderer;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.ViewModels;

public partial class AffineTransformationPopupViewModel(SequenceViewModel model) : ViewModelBase
{
    [ObservableProperty] public partial decimal OffsetX { get; set; } = (decimal) model.Sequence.DrawOffsetX;
    [ObservableProperty] public partial decimal OffsetY { get; set; } = (decimal) model.Sequence.DrawOffsetY;
    [ObservableProperty] public partial decimal ScaleX { get; set; } = (decimal) model.Sequence.ScaleX;
    [ObservableProperty] public partial decimal ScaleY { get; set; } = (decimal) model.Sequence.ScaleY;

    public bool Save()
    {
        model.Sequence.DrawOffsetX = (double) OffsetX;
        model.Sequence.DrawOffsetY = (double) OffsetY;
        model.Sequence.ScaleX = (double) ScaleX;
        model.Sequence.ScaleY = (double) ScaleY;
        model.RenderersUpdated.Invoke(this, new RenderersUpdatedEventArgs());
        return true;
    }
}