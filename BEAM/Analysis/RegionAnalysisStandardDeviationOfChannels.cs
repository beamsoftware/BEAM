﻿using System;
using BEAM.Datatypes;
using BEAM.ImageSequence;
using BEAM.Profiling;
using ScottPlot;

namespace BEAM.Analysis;

/// <summary>
/// Class implementing methods for calculating the standard deviation of the Channel values of all pixels in a region.
/// Returns a plot depicting the standard deviation of the pixels in the region for each channel.
/// </summary>
public class RegionAnalysisStandardDeviationOfChannels : Analysis
{
    private const string Name = "Region Analysis Standard Deviation";

    private double[] _sumChannels = []; // sum of channel values
    private double[] _sumChannelsSquared = []; // sum of channel values squared
    private Coordinate2D _topLeft;
    private Coordinate2D _bottomRight;
    private int _amountChannels;

    public override Plot Analyze(Coordinate2D pointerPressedPoint, Coordinate2D pointerReleasedPoint,
        ISequence sequence)
    {
        using var _ = Timer.Start("Region analysis (std deviation of channels)");
        _topLeft =
            new Coordinate2D((long)Math.Min(pointerPressedPoint.Row, pointerReleasedPoint.Row),
                (long)Math.Min(pointerPressedPoint.Column, pointerReleasedPoint.Column));

        _bottomRight =
            new Coordinate2D((long)Math.Max(pointerPressedPoint.Row, pointerReleasedPoint.Row),
                (long)Math.Max(pointerPressedPoint.Column, pointerReleasedPoint.Column));

        _amountChannels = sequence.Shape.Channels;

        Plot plot;

        // Catch trivial case of only one pixel selected
        if (Math.Abs(_AmountPixels() - 1) < 0.001)
        {
            _sumChannelsSquared = new double[_amountChannels];

            plot = PlotCreator.CreateFormattedBarPlot(_sumChannelsSquared);
            plot.Title(Name);
            return plot;

            //return PlotCreator.CreateFormattedBarPlot(_sumChannelsSquared);
        }

        // Calculate the standard deviations and store them in _sumChannelsSquared
        _CalculateResult(sequence);

        plot = PlotCreator.CreateFormattedBarPlot(_sumChannelsSquared);
        plot.Title(Name);
        return plot;
    }


    /// <summary>
    /// Calculates the standard deviation of the channels in the region and stores the result in _sumChannelsSquared
    /// </summary>
    /// <param name="sequence"></param>
    private void _CalculateResult(ISequence sequence)
    {
        _sumChannels = new double[_amountChannels];
        _sumChannelsSquared = new double[_amountChannels];

        // fill _sumChannels(Squared) with the correct values given in the sequence
        for (var row = _topLeft.Row; row <= _bottomRight.Row; row++)
        {
            for (var column = _topLeft.Column; column <= _bottomRight.Column; column++)
            {
                _UpdateWithPixel(sequence.GetPixel((long)column, (long)row));
            }
        }

        _calculateMeans();
        for (int i = 0; i < _amountChannels; i++)
        {
            _sumChannelsSquared[i] = _CalculateStandardDeviation(i);
        }
    }


    /// <summary>
    /// Update _sumChannels(Squared) with the given pixel.
    /// </summary>
    /// <param name="pixel"></param>
    private void _UpdateWithPixel(double[] pixel)
    {
        for (int channel = 0; channel < _amountChannels; channel++)
        {
            _sumChannels[channel] += pixel[channel];
            _sumChannelsSquared[channel] += Math.Pow(pixel[channel], 2);
        }
    }

    /// <summary>
    /// Calculate the amount of pixels in the given region.
    /// </summary>
    private double _AmountPixels()
    {
        return (_bottomRight.Row - _topLeft.Row + 1) * (_bottomRight.Column - _topLeft.Column + 1);
    }

    /// <summary>
    /// Convert _sumChannels entries to the means of the values of the channels.
    /// </summary>
    private void _calculateMeans()
    {
        var amountPixels = _AmountPixels();

        for (int i = 0; i < _amountChannels; i++)
        {
            _sumChannels[i] /= amountPixels;
        }
    }

    /// <summary>
    /// Calculate the standard deviation of the given channel.
    /// stdDev(i) = (sumSquared(i) - amountPixels * mean(i)²) / (amountPixels - 1)
    /// </summary>
    /// <returns></returns>
    private double _CalculateStandardDeviation(int channel)
    {
        if (channel < 0 || channel >= _amountChannels)
        {
            throw new ArgumentException("Channel Number given ( " + channel + " ) must be between 0 and " +
                                        _amountChannels + "!");
        }

        var amountPixels = _AmountPixels();
        var meanSquared = Math.Pow(_sumChannels[channel], 2);
        var stdDev = (_sumChannelsSquared[channel] - amountPixels * meanSquared)
                     / (_AmountPixels() - 1);

        return Math.Sqrt(stdDev);
    }

    public override string ToString()
    {
        return Name;
    }
}