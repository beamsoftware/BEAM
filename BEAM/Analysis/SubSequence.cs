using Avalonia;
using BEAM.ImageSequence;

namespace BEAM.Analysis;

public abstract class SubSequence
{
    public Sequence sourceSequence;

    private Point _topLeft;
    
    public abstract double _GetPixelFromSequence(Point coord, int channel);

    public abstract double getPixel(Point coord, int channel);
}