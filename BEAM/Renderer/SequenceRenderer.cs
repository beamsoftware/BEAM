using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using BEAM.Datatypes.Color;
using BEAM.ImageSequence;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.Renderer;

public abstract partial class SequenceRenderer : ObservableObject, ICloneable
{
    [ObservableProperty] private double _minimumOfIntensityRange;
    [ObservableProperty] private double _maximumOfIntensityRange;

    protected double IntensityRange => MaximumOfIntensityRange - MinimumOfIntensityRange;

    private static readonly Dictionary<RenderTypes, SequenceRenderer> _mapRenderTypesToRenderers = new(){
        [RenderTypes.HeatMapRendererRb] = new HeatMapRendererRB(0, 0, 0, 0, 0),
        [RenderTypes.ChannelMapRenderer] = new ChannelMapRenderer(0, 0, 0, 0, 0),
        [RenderTypes.ArgMaxRendererGrey] = new ArgMaxRendererGrey(0, 0),
        [RenderTypes.ArgMaxRendererColorHsva] = new ArgMaxRendererColorHSV(0, 0),
    };

    // variables used for NormalizeIntensity
    private Vector256<double> _minIntensities;
    private Vector256<double> _maxIntensities;
    private Vector256<double> _multFactor;

    /// <summary>
    /// Set the Minimum- and Maximum values for the intensity values (e.g. 0 - 1 or 0 - 255)
    /// This intensity is given by the user
    /// </summary>
    /// <param name="minimumOfIntensityRange"></param>
    /// <param name="maximumOfIntensityRange"></param>
    protected SequenceRenderer(double minimumOfIntensityRange, double maximumOfIntensityRange)
    {
        if (maximumOfIntensityRange <= minimumOfIntensityRange)
        {
            throw new ArgumentException("Given maximumOfIntensityRange must be greater than " +
                                        "to minimumOfIntensityRange for color intensities rendering.");
        }

        MinimumOfIntensityRange = minimumOfIntensityRange;
        MaximumOfIntensityRange = maximumOfIntensityRange;

        _multFactor = Vector256.Create<double>(255);

        PropertyChanged += (_, _) =>
        {
            _minIntensities = Vector256.Create<double>(MinimumOfIntensityRange);
            _maxIntensities = Vector256.Create<double>(MaximumOfIntensityRange);
        };
    }

    protected Vector256<double> NormalizeIntensity(Vector256<double> intensities)
    {
        return (intensities - _minIntensities) / (_maxIntensities - _minIntensities) * _multFactor;
    }

    public static SequenceRenderer GetRenderer(RenderTypes renderType, int minimumOfIntensityRange,
        int maximumOfIntensityRange, double[] displayParameters)
    {
        SequenceRenderer renderer = _mapRenderTypesToRenderers[renderType]
            .Create(minimumOfIntensityRange, maximumOfIntensityRange, displayParameters);

        return renderer;
    }

    public abstract BGR RenderPixel(ISequence sequence, long x, long y);

    public abstract BGR[] RenderPixels(ISequence sequence, long[] xs, long y);

    public abstract RenderTypes GetRenderType();

    protected abstract SequenceRenderer Create(int minimumOfIntensityRange, int maximumOfIntensityRange,
        double[] displayParameters);

    protected abstract bool CheckParameters(double[] displayParameters);

    public abstract string GetName();
    public abstract object Clone();
}