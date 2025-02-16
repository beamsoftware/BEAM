﻿using System.Runtime.Intrinsics;
using System.Threading;
using BEAM.Datatypes.Color;
using BEAM.Exceptions;
using BEAM.Image;
using BEAM.ImageSequence;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.Renderer;

/// <summary>
/// Renderer that maps n given channels to an ARGB color value.
/// For this three channel numbers i, j, k < n (first channel is 0) are given.
/// Red is set to the intensity of the ith channel, Green to the jth channel, Blue to the kth channel.
/// </summary>
public partial class ChannelMapRenderer : SequenceRenderer
{
    public ChannelMapRenderer(double minimumOfIntensityRange, double maximumOfIntensityRange,
        int channelRed, int channelGreen, int channelBlue)
        : base(minimumOfIntensityRange, maximumOfIntensityRange)
    {
        ChannelRed = channelRed;
        ChannelGreen = channelGreen;
        ChannelBlue = channelBlue;
    }

    [ObservableProperty] private int channelRed;
    [ObservableProperty] private int channelGreen;
    [ObservableProperty] private int channelBlue;

    /// <summary>
    /// Create the RGBA value for a given pixel of a sequence
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public override BGR RenderPixel(ISequence sequence, long x, long y)
    {
        var colors = NormalizeIntensity(Vector256.Create([
            sequence.GetPixel(x, y, ChannelBlue),
            sequence.GetPixel(x, y, ChannelGreen),
            sequence.GetPixel(x, y, ChannelRed),
            0
        ]));

        BGR color = new BGR()
        {
            B = (byte)colors[0], // b
            G = (byte)colors[1], // g
            R = (byte)colors[2], // r
        };

        return color;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="xs"></param>
    /// <param name="y"></param>
    /// <returns>[x, argb]</returns>
    public override BGR[] RenderPixels(ISequence sequence, long[] xs, long y,
        CancellationTokenSource? tokenSource = null)
    {
        var data = new BGR[xs.Length];
        var img = sequence.GetPixelLineData(xs, y, [ChannelBlue, ChannelGreen, ChannelRed]);

        for (var x = 0; x < xs.Length; x++)
        {
            tokenSource?.Token.ThrowIfCancellationRequested();
            var colors = NormailizeIntensity(Vector256.Create([
                img.GetPixel(x, 0, 0),
                img.GetPixel(x, 0, 1),
                img.GetPixel(x, 0, 2),
                0
            ]));

            data[x] = new BGR()
            {
                B = (byte)colors[0], // b
                G = (byte)colors[1], // g
                R = (byte)colors[2], // r
            };
            
            
        }

        return data;
    }

    public override RenderTypes GetRenderType()
    {
        return RenderTypes.ChannelMapRenderer;
    }

    protected override SequenceRenderer Create(int minimumOfIntensityRange, int maximumOfIntensityRange,
        double[] displayParameters)
    {
        // TODO remove null
        if (!CheckParameters(displayParameters, null))
        {
            throw new InvalidUserArgumentException("Display parameters are invalid.");
        }

        ;
        return new ChannelMapRenderer(
            minimumOfIntensityRange,
            maximumOfIntensityRange,
            (int)displayParameters[0],
            (int)displayParameters[1],
            (int)displayParameters[2]);
    }

    //TODO: Check if channels are in range for given Image, not possible yet, if image not attribute
    /// <summary>
    /// Verifies that the parameters are valid for the renderer and sequence.
    /// Returns True, if the parameters are valid, false otherwise.
    /// </summary>
    /// <param name="displayParameters"></param>
    /// <param name="image"></param>
    /// <returns></returns>
    protected override bool CheckParameters(double[] displayParameters, IImage image)
    {
        return displayParameters.Length == 3
               && !(displayParameters[0] < 0)
               && !(displayParameters[1] < 0)
               && !(displayParameters[2] < 0);
    }

    public override string GetName() => "Channel Map";
    public override object Clone()
    {
        return new ChannelMapRenderer(MinimumOfIntensityRange, MaximumOfIntensityRange, ChannelRed, ChannelGreen,
            ChannelBlue);
    }

    private Vector256<double> NormailizeIntensity(Vector256<double> intensities)
    {
        var minIntensities = Vector256.Create<double>(MinimumOfIntensityRange);
        var maxIntensities = Vector256.Create<double>(MaximumOfIntensityRange);
        var multFactor = Vector256.Create<double>(255);

        return (intensities - minIntensities) / (maxIntensities - minIntensities) * multFactor;
    }
}