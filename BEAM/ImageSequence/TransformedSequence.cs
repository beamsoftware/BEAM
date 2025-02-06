using System;
using System.Linq;
using BEAM.Image;

namespace BEAM.ImageSequence;

public class TransformedSequence(ISequence originalSequence) : ISequence
{
    public double ScaleX { get; set; } = 1;
    public double ScaleY { get; set; } = 1;

    public double DrawOffsetX { get; set; } = 0;
    public double DrawOffsetY { get; set; } = 0;

    public ImageShape Shape => new(_TransformX(originalSequence.Shape.Width),
        _TransformY(originalSequence.Shape.Height),
        originalSequence.Shape.Channels);

    public double GetPixel(long x, long y, int channel)
    {
        var transform = _UndoTransform(x, y);
        return originalSequence.GetPixel(transform.x, transform.y, channel);
    }

    public double[] GetPixel(long x, long y)
    {
        var transform = _UndoTransform(x, y);
        return originalSequence.GetPixel(transform.x, transform.y);
    }

    public double[] GetPixel(long x, long y, int[] channels)
    {
        var transform = _UndoTransform(x, y);
        return originalSequence.GetPixel(transform.x, transform.y, channels);
    }

    public LineImage GetPixelLineData(long line, int[] channels)
    {
        return originalSequence.GetPixelLineData(_UndoTransformY(line), channels);
    }

    public LineImage GetPixelLineData(long[] xs, long line, int[] channels)
    {
        var transformedXs = xs.Select(_UndoTransformX).ToArray();
        return originalSequence.GetPixelLineData(transformedXs, _UndoTransformY(line), channels);
    }

    public string GetName()
    {
        return originalSequence.GetName();
    }

    public void Dispose()
    {
        originalSequence.Dispose();
        GC.SuppressFinalize(this);
    }

    private long _UndoTransformX(long x)
    {
        return (long)((x) / ScaleX);
    }

    private long _UndoTransformY(long y)
    {
        return (long)((y) / ScaleY);
    }

    private (long x, long y) _UndoTransform(long x, long y)
    {
        return (_UndoTransformX(x), _UndoTransformY(y));
    }

    private long _TransformX(long x)
    {
        return (long)(x * ScaleX);
    }

    private long _TransformY(long y)
    {
        return (long)(y * ScaleY);
    }

    private (long x, long y) _Transform(long x, long y)
    {
        return (_TransformX(x), _TransformY(y));
    }
}