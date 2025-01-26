﻿using BEAM.Exceptions;
using BEAM.ImageSequence;

namespace BEAM.Renderer;

/// <summary>
/// A Renderer using the ArgMax function. Calculates the channel with the highest intensity and encodes it's number as a
/// color.
/// </summary>
/// <param name="minimumOfIntensityRange"></param>
/// <param name="maximumOfIntensityRange"></param>
public abstract class ArgMaxRenderer(int minimumOfIntensityRange, int maximumOfIntensityRange)
    : SequenceRenderer(minimumOfIntensityRange, maximumOfIntensityRange)
{
    public override byte[] RenderPixel(Sequence sequence, long x, long y)
    {
        var channels = sequence.GetPixel(x, y);
        var argMaxChannel = ArgMax(channels);
        var color = GetColor(argMaxChannel, channels.Length);
        
        return color;
    }

    //TODO: implement. Currently do not understand LineImage
    public override byte[,] RenderPixels(Sequence sequence, long[] xs, long y)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Given an Array of channel intensities, return the first index with the highest intensity
    /// </summary>
    /// <param name="channels"></param>
    /// <returns></returns>
    /// <exception cref="ChannelException"></exception>
    private int ArgMax(double[] channels)
    {
        if (channels.Length <= 0)
        {
            throw new ChannelException("Channels must be greater than 0.");
        }
        
        double maxIntensity = MinimumOfIntensityRange;
        int maxPosition = 0;
        for (int i = 0; i < channels.Length; i++)
        {
            if (channels[i] > maxIntensity)
            {
                maxIntensity = channels[i];
                maxPosition = i;
            }
        }

        return maxPosition;
    }
    
    protected abstract byte[] GetColor(int channelNumber, int amountChannels);
}