using System;
using BEAM.Image;
using BEAM.Renderer.Attributes;

namespace BEAM.ImageSequence;

/// <summary>
/// This class is used to represent a sequence, from which a certain portion at the beginning is cut off.
/// </summary>
/// <param name="name">The name of the sequence</param>
/// <param name="startOffset">The offset in the sequence, before which its content will be cut</param>
/// /// <param name="endOffset">The offset in the sequence, after which its content will be cut</param>
/// <param name="originalSequence">The original Sequence</param>
public class CutSequence(string name, long startOffset, long endOffset, ISequence originalSequence) : ISequence
{
    private ImageShape? _shape;

    public double GetPixel(long x, long y, int channel)
    {
        return originalSequence.GetPixel(x, y + startOffset, channel);
    }

    public double[] GetPixel(long x, long y)
    {
        return originalSequence.GetPixel(x, y + startOffset);
    }

    public double[] GetPixel(long x, long y, int[] channels)
    {
        return originalSequence.GetPixel(x, y + startOffset, channels);
    }

    public LineImage GetPixelLineData(long line, int[] channels)
    {
        return originalSequence.GetPixelLineData(line + startOffset, channels);
    }

    public LineImage GetPixelLineData(long[] xs, long line, int[] channels)
    {
        return originalSequence.GetPixelLineData(xs, line + startOffset, channels);
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
        _shape = new ImageShape(originalShape.Width, endOffset - startOffset, originalShape.Channels);
    }


    public void Dispose()
    {
        originalSequence.Dispose();
        GC.SuppressFinalize(this);
    }
}