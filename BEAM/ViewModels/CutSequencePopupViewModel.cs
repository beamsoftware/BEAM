using System;
using BEAM.ImageSequence;
using BEAM.Renderer;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.ViewModels;

public partial class CutSequencePopupViewModel : ViewModelBase
{
    private readonly SequenceViewModel _sequenceViewModel;
    [ObservableProperty]
    public partial long Offset { get; set; } = 0;

    [ObservableProperty]
    public partial long MaxOffset { get; set; }

    public CutSequencePopupViewModel(SequenceViewModel model)
    {
        _sequenceViewModel = model;
        MaxOffset = _sequenceViewModel.Sequence.Shape.Height - 1;
    }
    
    public bool Save()
    {
        if (Offset < 0 || Offset > MaxOffset)
        {
            return false;
        }   
        _sequenceViewModel.Sequence = new TransformedSequence(new CutSequence(_sequenceViewModel.Sequence.GetName(),
                                        Offset, _sequenceViewModel.Sequence));
        _sequenceViewModel.CutSequence(this, new RenderersUpdatedEventArgs(Offset));
        return true;
    }
}