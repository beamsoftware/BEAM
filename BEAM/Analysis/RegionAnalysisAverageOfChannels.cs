using System;
using System.Threading;
using BEAM.Datatypes;
using BEAM.ImageSequence;
using BEAM.ViewModels;
using ScottPlot;
using Double = double;
using Timer = BEAM.Profiling.Timer;

namespace BEAM.Analysis;

/// <summary>
/// Class implementing methods for calculating the average of the Channel values of all pixels in a region.
/// Returns a plot depicting the average of the pixels in the region for each channels.
/// </summary>
public class RegionAnalysisAverageOfChannels : Analysis
{
    private const string Name = "Region Analysis Average";

    private double[] _sumChannels = [];  // sum of channel values
    private Coordinate2D _topLeft;
    private Coordinate2D _bottomRight;
    private int _amountChannels;
    private InspectionViewModel? _inspectionViewModel;
    private CancellationToken _token;

    protected override void PerformAnalysis(Coordinate2D pointerPressedPoint, Coordinate2D pointerReleasedPoint,
        ISequence sequence, InspectionViewModel inspectionViewModel, CancellationToken cancellationToken)
    {
        using var _ = Timer.Start("Region analysis (avg of channels)");
        _topLeft =
            new Coordinate2D((long)Math.Min(pointerPressedPoint.Row, pointerReleasedPoint.Row),
                (long)Math.Min(pointerPressedPoint.Column, pointerReleasedPoint.Column));

        _bottomRight =
            new Coordinate2D((long)Math.Max(pointerPressedPoint.Row, pointerReleasedPoint.Row),
                (long)Math.Max(pointerPressedPoint.Column, pointerReleasedPoint.Column));

        _amountChannels = sequence.Shape.Channels;
        _inspectionViewModel = inspectionViewModel;
        _token = cancellationToken;

        // Calculate the average and store them in _sumChannels
        _CalculateResult(sequence);
    }

    protected override Plot GetAnalysisPlot()
    {
        var plot = PlotCreator.CreateFormattedBarPlot(_sumChannels);
        plot.Title(Name);
        return plot;
    }

    public override AnalysisTypes GetAnalysisType()
    {
        return AnalysisTypes.RegionAnalysisAverageOfChannels;
    }

    /// <summary>
    /// Calculates the average of the channels in the region and stores the result in _sumChannels
    /// </summary>
    /// <param name="sequence"></param>
    private void _CalculateResult(ISequence sequence)
    {
        _sumChannels = new double[_amountChannels];
        var progressDisplayInterval = (_bottomRight.Row - _topLeft.Row) * (_bottomRight.Column - _topLeft.Column) / 100;
        var counterToNextProgressDisplay = progressDisplayInterval;
        
        // if the analysed region has less than 100 pixels, do not display the progress
        if (counterToNextProgressDisplay < 1) counterToNextProgressDisplay = Double.PositiveInfinity;
        
        // stores the current process as percentage based value (Progress = relative amount of pixels already visited)
        byte currentProgress = 0;
        SetProgress(_inspectionViewModel ?? throw new InvalidOperationException(), currentProgress);

        // fill _sumChannels(Squared) with the correct values given in the sequence
        for (var row = _topLeft.Row; row <= _bottomRight.Row; row++)
        {
            for (var column = _topLeft.Column; column <= _bottomRight.Column; column++)
            {
                _UpdateWithPixel(sequence.GetPixel((long)column, (long)row));
                CheckAndCancelAnalysis(_token);

                // report progress to InspectionViewModel
                counterToNextProgressDisplay--;
                if (counterToNextProgressDisplay > 0) continue;
                currentProgress++;
                counterToNextProgressDisplay = progressDisplayInterval;
                SetProgress(_inspectionViewModel, currentProgress);
            }
        }

        _calculateAverage();
    }

    /// <summary>
    /// Update _sumChannels with the given pixel.
    /// </summary>
    /// <param name="pixel"></param>
    private void _UpdateWithPixel(double[] pixel)
    {
        for (int channel = 0; channel < _amountChannels; channel++)
        {
            _sumChannels[channel] += pixel[channel];
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
    private void _calculateAverage()
    {
        var amountPixels = _AmountPixels();

        for (int i = 0; i < _amountChannels; i++)
        {
            _sumChannels[i] /= amountPixels;
        }
    }

    public override string ToString()
    {
        return Name;
    }

}