using System;
using System.Linq;
using BEAM.Controls;
using BEAM.Datatypes;
using BEAM.Datatypes.Color;
using BEAM.ImageSequence;
using BEAM.ViewModels;
using BEAM.Views.Utility;

namespace BEAM.Renderer;

/// <summary>
/// An ArgMaxRenderer, which maps the channel number to colors, chosen by the user.
/// </summary>
/// <param name="minimumOfIntensityRange"></param>
/// <param name="maximumOfIntensityRange"></param>
public class ArgMaxRendererColorHSV(double minimumOfIntensityRange, double maximumOfIntensityRange) : ArgMaxRenderer(minimumOfIntensityRange, maximumOfIntensityRange)
{
    private ChannelHSVMap _channelHsvMap = new(0);

    public override RenderTypes GetRenderType()
    {
        return RenderTypes.ArgMaxRendererColorHsva;
    }

    protected override SequenceRenderer Create(int minIntensityRange, int maximumOfIntensityRange, double[] displayParameters)
    {
        return new ArgMaxRendererColorHSV(minIntensityRange, maximumOfIntensityRange);
    }

    protected override bool CheckParameters(double[] displayParameters)
    {
        return displayParameters.Length == 0;
    }

    public override object Clone()
    {
        return new ArgMaxRendererColorHSV(MinimumOfIntensityRange, MaximumOfIntensityRange);
    }

    public override SaveUserControl GetConfigView(SequenceViewModel baseVm)
    {
        return new ArgMaxHSVConfigControlView(this, baseVm);
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
        var intensity = ((double)(channelNumber + 1) / (double)amountChannels); // in [0, 1]
        var color = new HueColorLut();
        return color[intensity];
    }

    public override BGR RenderPixel(ISequence sequence, long x, long y)
    {
        var channels = sequence.GetPixel(x, y);
        if (channels.Length != _channelHsvMap.AmountChannels)
        {
            throw new ArgumentException("Channel count mismatch for sequence and ChannelHSVMap of ArgMax!");
        }

        var argMaxChannel = channels
            .Select((value, index) => new { Value = value, Index = index }) // Create an anonymous type with value and index
            .Where(t => _channelHsvMap.IsChannelUsed(t.Index)) // Filter based on ChannelUsed
            .Select(t => t.Value) // Select only the values
            .ToArray() // Convert to array
            .ArgMax(); // Call ArgMax on the filtered array

        var color = GetColor(argMaxChannel, channels.Length);

        return color;
    }

    public override BGR[] RenderPixels(ISequence sequence, long[] xs, long y)
    {
        var usedChannelIndices = _channelHsvMap.GetUsedChannels();

        var line = sequence.GetPixelLineData(xs, y, usedChannelIndices);
        var data = new BGR[xs.Length];

        for (var x = 0; x < xs.Length; x++)
        {
            var argMaxChannel = line.GetPixel(x, 0).ArgMax(); // Call ArgMax on the filtered array

            var color = _channelHsvMap.GetColorBGR(usedChannelIndices[argMaxChannel]);

            data[x] = color;
        }

        return data;
    }

    /// <summary>
    /// Sets the ChannelHSVMap to a new ChannelHSVMap created from the given Array of ChannelToHSVs,
    ///  but only if the amount of channels does not change.
    /// If the amount of channels originally was zero, the update takes place,
    /// initializing the ChannelHSVMap
    /// </summary>
    /// <param name="channels">The channels to update the map from channels to hsv color.</param>
    /// <exception cref="ArgumentException">If the numver of channels does not match</exception>
    public void UpdateChannelHSVMap(ChannelToHSV[] channels)
    {
        if (_channelHsvMap.AmountChannels != 0 && channels.Length != _channelHsvMap.AmountChannels)
        {
            throw new ArgumentException("Channel count mismatch for ChannelHSVMap of ArgMax! Changed from: "
                                        + _channelHsvMap.AmountChannels + " to " + channels.Length);
        }

        _channelHsvMap = new ChannelHSVMap(channels);
    }

    public ChannelHSVMap GetChannelHsvMap()
    {
        return _channelHsvMap.Clone();
    }

    public override string GetName()
    {
        return $"{base.GetName()} (Color HSV)";
    }
}