using System;
using BEAM.Controls;
using BEAM.Datatypes.Color;
using BEAM.Exceptions;
using BEAM.ViewModels;
using BEAM.Views.Utility;

namespace BEAM.Renderer;

/// <summary>
/// Concrete Renderer for a HeatMap.
/// The intensity of this channel is encoded in an ARGB-value and returned.
/// The hottest color is red, the coldest blue, always no transparency.
/// </summary>
public class HeatMapRendererRB : HeatMapRenderer
{
    /// <summary>
    /// Constructor for a HeatMapRendererRB
    /// </summary>
    /// <param name="minimumOfIntensityRange"></param>
    /// <param name="maximumOfIntensityRange"></param>
    /// <param name="relMaxColdestIntensity"></param>
    /// <param name="relMinHottestIntensity"></param>
    /// <param name="channel"></param>
    public HeatMapRendererRB(double minimumOfIntensityRange, double maximumOfIntensityRange,
        int channel, double relMaxColdestIntensity, double relMinHottestIntensity) : base(
        minimumOfIntensityRange, maximumOfIntensityRange,
        channel, relMaxColdestIntensity, relMinHottestIntensity)
    {
    }

    protected override BGR GetColor(double value, double min, double max)
    {
        if (value > max) // intensity above maximum --> hottest color displayed
        {
            return new BGR() { R = 255 }; // Color Red;
        }

        if (value < min) // intensity below minimum --> coldest color displayed
        {
            return new BGR() { B = 255 }; // Color Blue
        }

        // if max == min, return a mixture of Red and Blue for all pixels, whose intensity = max = min
        if ((max - min) < 0.001)
        {
            return new BGR() { B = 127, R = 127 };
        }

        double range = (max - min);
        double relative = (value - min) / range; // calculate the relative intensity inside the range between min and max --> Normalize
        // the value of the color
        byte intensity = (byte)Math.Floor(relative * (double)255);

        var color = new BGR() { R = intensity, B = (byte)(255 - intensity) };
        return color;
    }

    public override RenderTypes GetRenderType()
    {
        return RenderTypes.HeatMapRendererRb;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="minimumOfIntensityRange"></param>
    /// <param name="maximumOfIntensityRange"></param>
    /// <param name="displayParameters">
    /// channel, MaxColdestIntensity, MinHottestIntensity
    /// </param>
    /// <returns></returns>
    /// <exception cref="InvalidUserArgumentException"></exception>
    protected override SequenceRenderer Create(int minimumOfIntensityRange, int maximumOfIntensityRange, double[] displayParameters)
    {
        if (!CheckParameters(displayParameters))
        {
            throw new InvalidUserArgumentException("Display parameters are invalid.");
        }
        return new HeatMapRendererRB(minimumOfIntensityRange, maximumOfIntensityRange, (int)displayParameters[0], displayParameters[1], displayParameters[2]);

    }

    protected override bool CheckParameters(double[] displayParameters)
    {
        if (displayParameters.Length != 3
            || displayParameters[0] < 0 // the channel
            || displayParameters[1] > 1 // maxColdestIntensity
            || displayParameters[1] < 0
            || displayParameters[2] < displayParameters[1] // minHottestIntensity
            || displayParameters[2] > 1)
        {
            return false;
        }

        return true;
    }

    public override object Clone()
    {
        return new HeatMapRendererRB(MinimumOfIntensityRange, MaximumOfIntensityRange, Channel, RelMinHottestIntensity, RelMinHottestIntensity);
    }

    public override SaveUserControl GetConfigView(SequenceViewModel baseVm)
    {
        return new HeatMapConfigControlView(this, baseVm);
    }
}