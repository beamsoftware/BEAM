using System;
using BEAM.Docking;
using BEAM.ImageSequence;
using BEAM.Renderer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BEAM.ViewModels;

public partial class SequenceViewModel : ViewModelBase, IDockBase
{
    public EventHandler<RenderersUpdatedEventArgs> RenderersUpdated = delegate { };

    public Sequence Sequence { get; }

    public SequenceRenderer[] Renderers;
    public int RendererSelection;

    public SequenceViewModel(Sequence sequence)
    {
        Sequence = sequence;
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

    public string Name => Sequence.GetName;

    public SequenceRenderer CurrentRenderer => Renderers[RendererSelection];
}