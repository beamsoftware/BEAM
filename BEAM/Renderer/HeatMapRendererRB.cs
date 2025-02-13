using System;
using BEAM.Exceptions;
using IImage = BEAM.Image.IImage;

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

    protected override byte[] GetColor(double value, double min, double max)
    {
        if (value > max) // intensity above maximum --> hottest color displayed
        {
            byte[] hot = [255, 255, 0, 0]; // Color Red
            return hot;
        }

        if (value < min) // intensity below minimum --> coldest color displayed
        {
            byte[] cold = [255, 0, 0, 255]; // Color Blue
            return cold;
        }
        
        // if max == min, return a mixture of Red and Blue for all pixels, whose intensity = max = min
        if ((max - min) < 0.001) 
        {
            return new byte[] { 255, 127, 0, 127 }; 
        }
        double range = (max - min);
        double relative = (value - min) / range; // calculate the relative intensity inside the range between min and max --> Normalize
        // the value of the color
        byte intensity = (byte)Math.Floor(relative * (double)255);

        byte[] color = [255, intensity, 0, (byte)(255 - intensity)];
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
        // TODO remove null
        if (!CheckParameters(displayParameters))
        {
            throw new InvalidUserArgumentException("Display parameters are invalid.");
        };
        return new HeatMapRendererRB(minimumOfIntensityRange, maximumOfIntensityRange, (int)displayParameters[0], displayParameters[1], displayParameters[2]);

    }

    //TODO: Check if channel is in range for given Image, not possible yet, if image not attribute
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
}