using BEAM.Datatypes.Color;
using BEAM.ViewModels;
using BEAM.Views.Utility;

namespace BEAM.Renderer;

/// <summary>
/// An ArgMaxRenderer, which maps the channel number to shades of grey.
/// </summary>
/// <param name="minimumOfIntensityRange"></param>
/// <param name="maximumOfIntensityRange"></param>
public class ArgMaxRendererGrey(double minimumOfIntensityRange, double maximumOfIntensityRange) : ArgMaxRenderer(minimumOfIntensityRange, maximumOfIntensityRange)
{
    public override RenderTypes GetRenderType()
    {
        return RenderTypes.ArgMaxRendererGrey;
    }

    protected override SequenceRenderer Create(int minimumOfIntensityRange, int maximumOfIntensityRange, double[] displayParameters)
    {
        return new ArgMaxRendererGrey(minimumOfIntensityRange, maximumOfIntensityRange);
    }

    protected override bool CheckParameters(double[] displayParameters)
    {
        return displayParameters.Length == 0;
    }

    public override object Clone()
    {
        return new ArgMaxRendererGrey(MinimumOfIntensityRange, MaximumOfIntensityRange);
    }

    public override SaveUserControl? GetConfigView(SequenceViewModel baseVm)
    {
        return null;
    }

    /// <summary>
    /// Converts the channel position of the channel with the highest intensity into an BGR value.
    /// </summary>
    /// <param name="channelNumber"></param>
    /// <param name="amountChannels"></param>
    /// <returns></returns>
    protected override BGR GetColor(int channelNumber, int amountChannels)
    {
        //calculate the relative position of the given channel in all channels
        // and map it to an int intensity between 0 and 255 for the RGB values.
        int intensity = (int)((double)channelNumber / (double)amountChannels * 255);
        return new BGR()
        {
            R = (byte)intensity,
            G = (byte)intensity,
            B = (byte)intensity
        };
    }

    public override string GetName()
    {
        return $"{base.GetName()} (Gray Scale)";
    }
}