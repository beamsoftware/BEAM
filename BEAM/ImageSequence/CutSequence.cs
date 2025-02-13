using System;
using System.Collections.Generic;
using BEAM.Image;

namespace BEAM.ImageSequence;

/// <summary>
/// This class is used to represent a sequence, from which a certain portion at the beginning is cut off.
/// </summary>
/// <param name="name">The name of the sequence</param>
/// <param name="offset">The offset in the sequence, which content before, will be cut</param>
/// <param name="originalSequence">The original Sequence</param>
public class CutSequence(string name, long offset, ISequence originalSequence) : ISequence
{
    private ImageShape? _shape;

    public double GetPixel(long x, long y, int channel)
    {
        return originalSequence.GetPixel(x, y + offset, channel);
    }

    public double[] GetPixel(long x, long y)
    {
        return originalSequence.GetPixel(x, y + offset);
    }

    public double[] GetPixel(long x, long y, int[] channels)
    {
        return originalSequence.GetPixel(x, y + offset, channels);
    }

    public LineImage GetPixelLineData(long line, int[] channels)
    {
        return originalSequence.GetPixelLineData(line + offset, channels);
    }

    public LineImage GetPixelLineData(long[] xs, long line, int[] channels)
    {
        return originalSequence.GetPixelLineData(xs, line + offset, channels);
    }

    public string GetName()
    {
        return name;
    }

    public ImageShape Shape
    {
        get
        {
            if (_shape is not null)
            {
                return _shape.Value;
            }

            _InitializeShape();
            return _shape!.Value;
        }
    }

    private void _InitializeShape()
    {
        var originalShape = originalSequence.Shape;
        _shape = new ImageShape(originalShape.Width, originalShape.Height - offset, originalShape.Channels);
    }


    public void Dispose()
    {
        originalSequence.Dispose();
        GC.SuppressFinalize(this);
    }
}