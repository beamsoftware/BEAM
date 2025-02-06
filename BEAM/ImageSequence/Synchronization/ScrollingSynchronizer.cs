using System.Collections.Generic;
using BEAM.Views;
using ScottPlot.Avalonia;

namespace BEAM.ImageSequence.Synchronization;

public static class ScrollingSynchronizer
{
    private static List<SequenceView> _sequences = new List<SequenceView>();
    private static Dictionary<AvaPlot, SequenceView> _viewMapping = new Dictionary<AvaPlot, SequenceView>();
    public static bool IsSynchronizing = false;
    
    public static void addSequence(SequenceView sequence)
    {
        _sequences.Add(sequence);
        _viewMapping.Add(sequence.AvaPlot1, sequence);
    }

    public static void synchronize(SequenceView sequenceView)
    {
        if (IsSynchronizing)
        {
            foreach (var view in _sequences)
            {
                if (view != sequenceView)
                {
                    view.UpdateScrolling(sequenceView.AvaPlot1);
                }
            }
        }
    }

    public static void UpdateOwnScrollBar(AvaPlot plot)
    {
        _viewMapping[plot].UpdateScrollBar();
    }
}