using System;
using ScottPlot;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using BEAM.Image.Displayer;
using BEAM.ImageSequence;
using BEAM.Renderer;
using ScottPlot.Avalonia;

namespace BEAM.IMage.Displayer.Scottplot;

public sealed class BitmapPlottable(ISequence sequence, SequenceRenderer renderer, long startLine = 0) : IPlottable
{
    public bool IsVisible { get; set; } = true;
    public IAxes Axes { get; set; } = new Axes();
    public IEnumerable<LegendItem> LegendItems => Enumerable.Empty<LegendItem>();

    public readonly SequenceImage SequenceImage = new(sequence, startLine, renderer);

    public AxisLimits GetAxisLimits()
    {
        //return new AxisLimits(0, sequence.Shape.Width, 0, sequence.Shape.Width);
        return new AxisLimits(0, sequence.Shape.Width, Math.Floor(sequence.Shape.Width / 2.0) + startLine,
            -Math.Floor(sequence.Shape.Width / 2.0) + startLine);
    }

    public void ChangeRenderer(SequenceRenderer renderer)
    {
        SequenceImage.Renderer = renderer;
    }

    public void Render(RenderPack rp)
    {
        // Drawing offset for transformed sequence
        var xOffset = 0.0;
        var yOffset = 0.0;
        if (sequence is TransformedSequence transformedSequence)
        {
            xOffset = transformedSequence.DrawOffsetX;
            yOffset = transformedSequence.DrawOffsetY;
        }

        rp.Plot.Axes.InvertY();

        // min <-> max flipped since inverted Y axis
        var minY = rp.Plot.Grid.YAxis.Max;
        var maxY = rp.Plot.Grid.YAxis.Min;
        SequenceImage.Update((long)minY, (long)maxY, rp.Canvas.DeviceClipBounds.Height);

        // drawing the images
        using SKPaint paint = new();
        paint.FilterQuality = SKFilterQuality.None;

        for (var i = 0; i < SequenceImage.GetRenderedPartsCount(); i++)
        {
            var preview = SequenceImage.GetRenderedPart(i);
            // TODO: fix bug where Bitmap is null
            if (preview?.Bitmap is null) continue;

            var coordinateRect = new CoordinateRect()
            {
                Left = xOffset,
                Right = sequence.Shape.Width + xOffset,
                Top = preview.YStart + yOffset,
                Bottom = preview.YEnd + yOffset
            };

            var dest = Axes.GetPixelRect(coordinateRect);
            rp.Canvas.DrawBitmap(preview.Bitmap, dest.ToSKRect(), paint);
        }
    }
}