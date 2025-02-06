using System.Threading;
using BEAM.Exceptions;
using BEAM.ImageSequence;

namespace BEAM.Renderer;

/// <summary>
/// General abstract class for a HeatMapRenderer.
/// It implements the function to Render a pixel.
/// The concrete function, that converts the channel intensity to a color is implemented by the subclasses.
/// </summary>
public abstract class HeatMapRenderer : SequenceRenderer
{
    /// <summary>
    /// The channel that is used in the HeatMap for the intensity of the HeatMap.
    /// </summary>
    public int Channel { get; set; }

    private double _relMaxColdestIntensity = 0; // initial value
    private double _relMinHottestIntensity = 1; // initial value

    // The highest absolute intensity that is represented with the coldest color.
    private double _absMaxColdestIntensity;

    // The lowest absolute intensity that is represented with the hottest color.
    private double _absMinHottestIntensity;

    /// <summary>
    /// The highest relative intensity between 0 and 1 that is represented with the coldest color.
    /// --> value between 0 and 100% of intensity range
    /// </summary>
    public double RelMaxColdestIntensity
    {
        get { return _relMaxColdestIntensity; }
        set
        {
            if (value >= 0 && value <= _relMinHottestIntensity)
            {
                _relMaxColdestIntensity = value;
                _absMaxColdestIntensity = value * IntensityRange + MinimumOfIntensityRange;
            }
            else
            {
                throw new InvalidUserArgumentException(
                    "The lower bound of temperature must be between 0 and upper bound!");
            }
        }
    }

    /// <summary>
    /// The lowest relative intensity between 0 and 1 that is represented with the coldest color.
    /// --> value between 0 and 100% of intensity range
    /// </summary>
    public double RelMinHottestIntensity
    {
        get { return _relMinHottestIntensity; }
        set
        {
            if (value <= 1 && value >= _relMaxColdestIntensity)
            {
                _relMinHottestIntensity = value;
                _absMinHottestIntensity = value * IntensityRange + MinimumOfIntensityRange;
            }
            else
            {
                throw new InvalidUserArgumentException(
                    "The upper bound of temperature must be between lower bound and 1!");
            }
        }
    }


    protected HeatMapRenderer(double minimumOfIntensityRange, double maximumOfIntensityRange, int channel,
        double relMaxColdestIntensity, double relMinHottestIntensity)
        : base(minimumOfIntensityRange, maximumOfIntensityRange)
    {
        Channel = channel;
        RelMaxColdestIntensity = relMaxColdestIntensity;
        RelMinHottestIntensity = relMinHottestIntensity;
    }

    public override byte[] RenderPixel(ISequence sequence, long x, long y)
    {
        return GetColor(sequence.GetPixel(x, y, Channel),
            IntensityRange * RelMaxColdestIntensity + MinimumOfIntensityRange,
            IntensityRange * RelMinHottestIntensity + MinimumOfIntensityRange);
    }

    public override byte[,] RenderPixels(ISequence sequence, long[] xs, long y,
        CancellationTokenSource? tokenSource = null)
    {
        var data = new byte[xs.Length, 4];
        var img = sequence.GetPixelLineData(xs, y, [Channel]);

        // TODO: SIMD
        for (var i = 0; i < xs.Length; i++)
        {
            tokenSource?.Token.ThrowIfCancellationRequested();

            var color = GetColor(img.GetPixel(i, 0, 0), MinimumOfIntensityRange, MaximumOfIntensityRange);
            data[i, 0] = color[1];
            data[i, 1] = color[2];
            data[i, 2] = color[3];
            data[i, 3] = color[0];
        }

        return data;
    }

    /// <summary>
    /// The specific function that converts the intensity of a channel into a Color.
    /// Is implemented by a subclass.
    /// </summary>
    /// <param name="value">The intensity of the chosen channel</param>
    /// <param name="min">The highest intensity of the channel, that is displayed as the coldest intensity.</param>
    /// <param name="max">The lowest intensity of the channel, that is displayed as the highest intensity.</param>
    /// <returns>The ARGB values of the final Color to be displayed.
    /// (A, R, G, B) each color from 0 - 255. A = 0 : fully transparent</returns>
    protected abstract byte[] GetColor(double value, double min, double max);

    public override string GetName() => "Heatmap";
}