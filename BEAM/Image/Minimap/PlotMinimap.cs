using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using BEAM.Docking;
using BEAM.Image.Minimap.MinimapAlgorithms;
using BEAM.Image.Minimap.Utility;
using BEAM.ImageSequence;
using BEAM.ViewModels.Minimap;
using BEAM.Views.Minimap;
using ScottPlot;

namespace BEAM.Image.Minimap;

/// <summary>
/// The minimap for a corresponding sequence. It creates an overview over it by calculating specific values based on an algorithm for each line.
/// Must be supplied with a concrete algorithm which is used for calculations.
/// </summary>
public class PlotMinimap : Minimap
{
    private int _compactionFactor = 100;
    /// <summary>
    /// The default minimap algorithm which will be used if this class is instantiated
    /// without a specific algorithm through its base class constructor.
    /// </summary>
    private static readonly IMinimapAlgorithm DefaultAlgorithm = new PixelSumAlgorithm(100);
    private bool _isGenerated;
    private MinimapPlotViewModel viewModel;


    /// <summary>
    /// The underlying algorithm used to calculate values for pixel lines. These values will later be displayed in the plot.
    /// </summary>
    private readonly IMinimapAlgorithm _minimapAlgorithm;
    
    public PlotMinimap(Sequence sequence, MinimapGeneratedEventHandler eventCallbackFunc) : base(sequence, eventCallbackFunc)
    {
        this._minimapAlgorithm = DefaultAlgorithm;
        Task.Run(GenerateMinimap, CancellationTokenSource.Token);
    }
    
    
    /// <summary>
    /// Initializes the minimap creation process. It creates a separately running Task which generates the values.
    /// Therefor, the minimap is not instantly ready after this method call ends hence a method which is used as a callback must be supplied.
    /// </summary>
    /// <param name="sequence">The sequence based on which the minimap is based.</param>
    /// <param name="eventCallbackFunc">A function which is invoked once the minimap has finished generating its values.
    /// This is being done through the <see cref="Minimap"/>'s MinimapGeneratedEventHandler event.</param>
    /// <param name="algorithm">The concrete algorithm used for value calculation.</param>
    /// <exception cref="ArgumentNullException">If any of the parameters is null.</exception>
    public PlotMinimap(Sequence sequence, MinimapGeneratedEventHandler eventCallbackFunc, IMinimapAlgorithm algorithm) : base(sequence, eventCallbackFunc)
    {
        ArgumentNullException.ThrowIfNull(algorithm);
        _minimapAlgorithm = algorithm;
        Task.Run(GenerateMinimap, CancellationTokenSource.Token);
    }



    /// <summary>
    /// Handles the logic for creating the minimap data alongside its
    /// visual representation in the required format(<see cref="Avalonia.Controls.UserControl"/>).
    /// </summary>
    private void GenerateMinimap()
    {
        
        //TODO: do Work with Sequence
        bool result = _minimapAlgorithm.AnalyzeSequence(Sequence, this.CancellationTokenSource.Token);
        Console.WriteLine("Returned " + result);
        if (!result)
        {
            Console.WriteLine("Returend false");
            // TODO: Should event also contain caller?
            OnMinimapGenerated(new MinimapGeneratedEventArgs(this, MinimapGenerationResult.Failure));
            return;
        }

        Plot plot = new Plot();
        double[]values = new double[Sequence.Shape.Height / 100];
        for (int i = 0; i < Sequence.Shape.Height / 100; i++)
        {
            values[i] = _minimapAlgorithm.GetLineValue(i * 100);
            Console.WriteLine("Line " + i + " Value: " + values[i]);
        }

        plot.Add.Bars(values);
        viewModel = new MinimapPlotViewModel(plot);
        _isGenerated = true;
        Console.WriteLine("Hello World");
        OnMinimapGenerated(new MinimapGeneratedEventArgs(this, MinimapGenerationResult.Success));
    }
    

    /// <summary>
    /// Returns the algorithm calculation based value for a specific line. Commonly used for plotting.
    /// </summary>
    /// <param name="line">The line whose value shall be returned.</param>
    /// <returns>The specified line's calculated value.</returns>
    /// <exception cref="InvalidOperationException">Thrown to indicate that
    /// the minimap has not yet finished its generation process.</exception>
    public float GetMinimapValue(long line)
    {
        if (!_isGenerated)
        {
            throw new InvalidOperationException();
        }

        return _minimapAlgorithm.GetLineValue(line);
    }


    public override IDockBase GetDock()
    {
        return viewModel;
    }
}