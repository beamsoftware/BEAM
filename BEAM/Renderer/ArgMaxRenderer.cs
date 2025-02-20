﻿using System;
using BEAM.Image;
using BEAM.ImageSequence;

namespace BEAM.Renderer;

/// <summary>
/// A Renderer using the ArgMax function. Calculates the channel with the highest intensity and encodes it's number as a
/// color.
/// </summary>
/// <param name="minimumOfIntensityRange"></param>
/// <param name="maximumOfIntensityRange"></param>
public abstract class ArgMaxRenderer(double minimumOfIntensityRange, double maximumOfIntensityRange)
    : SequenceRenderer(minimumOfIntensityRange, maximumOfIntensityRange)
{
    public override byte[] RenderPixel(ISequence sequence, long x, long y)
    {
        var channels = sequence.GetPixel(x, y);
        var argMaxChannel = ArgMax(channels);
        var color = GetColor(argMaxChannel, channels.Length);
        
        return color;
    }

    //TODO: implement. Currently do not understand LineImage
    public override byte[,] RenderPixels(ISequence sequence, long[] xs, long y)
    {
        byte[,] data = new byte[xs.Length,sequence.Shape.Channels];
        for(var i = 0; i < xs.Length; i++)
        {
            var channels = sequence.GetPixel(xs[i], y);
            var argMaxChannel = ArgMax(channels);
            var color = GetColor(argMaxChannel, channels.Length);
            for(var j = 0; j < sequence.Shape.Channels; j++)
            {
                data[i,j] = color[j];
            }
        }

        return data;
    }

    /// <summary>
    /// Given an Array of channel intensities, return the first index with the highest intensity
    /// </summary>
    /// <param name="channels"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">When channels is empty</exception>
    private int ArgMax(double[] channels)
    {
        if (channels.Length == 0)
        {
            throw new ArgumentException("Amount of channels must be greater than 0.");
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

    public override string GetName() => "ArgMax";
}