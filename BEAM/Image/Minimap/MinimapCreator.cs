using System;
using System.Threading.Tasks;
using BEAM.Image.Minimap.Utility;
using BEAM.ImageSequence;

namespace BEAM.Image.Minimap;

/// <summary>
/// The minimap for a corresponding sequence. It creates an overview over it by calculating specific values based on an algorithm for each line.
/// Must be supplied with a concrete algorithm which is used for calculations.
/// </summary>
public class MinimapCreator
{
    private readonly ISequence _sequence;
    private bool _isGenerated = false;
    
    /// <summary>
    /// A function blueprint for the callback function which is called concurrently/in parallel
    /// when the data generation process has finished.
    /// </summary>
    public delegate void MinimapGeneratedEventHandler(object sender, MinimapGeneratedEventArgs e);
    private event MinimapGeneratedEventHandler MinimapGenerated;
    
    private readonly IMinimapAlgorithm _minimapAlgorithm;
    
    /// <summary>
    /// Initializes the minimap creation process. It creates a separately running Task which generates the values.
    /// Therefor, the minimap is not instantly ready after this method call ends hence a method which is used as a callback must be supplied.
    /// </summary>
    /// <param name="sequence">The sequence based on which the minimap is based.</param>
    /// <param name="eventCallbackFunc">A function which is invoked once the minimap has finished generating its values.
    /// This is being done through the <see cref="MinimapGeneratedEventHandler"/> event.</param>
    /// <param name="algorithm">The concrete algorithm used for value calculation.</param>
    /// <exception cref="ArgumentNullException">Is any of the parameters is null.</exception>
    public MinimapCreator(ISequence sequence, MinimapGeneratedEventHandler eventCallbackFunc, IMinimapAlgorithm algorithm)
    {
        if (sequence is null || eventCallbackFunc is null || algorithm is null)
        {
            throw new ArgumentNullException(nameof(eventCallbackFunc));
        }
        _minimapAlgorithm = algorithm;
        _sequence = sequence;
        MinimapGenerated += eventCallbackFunc;
        Task.Run(GenerateMinimap);
    }

    private void GenerateMinimap()
    {
        
        //TODO: do Work with Sequence
        bool result = _minimapAlgorithm.AnalyzeSequence(_sequence);
        if (!result)
        {
            MinimapGenerated.Invoke(this, new MinimapGeneratedEventArgs(this, MinimapGenerationResult.Failure));
            return;
        }
        _isGenerated = true;
        MinimapGenerated.Invoke(this, new MinimapGeneratedEventArgs(this, MinimapGenerationResult.Success));
    }
    

    /// <summary>
    /// Returns the algorithm calculation based value for a specific line. Commonly used for plotting.
    /// </summary>
    /// <param name="line">The line whose value shall be returned.</param>
    /// <returns>The specified line's calculated value.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public float GetMinimapValue(long line)
    {
        if (!_isGenerated)
        {
            throw new NotImplementedException();
        }

        return _minimapAlgorithm.GetLineValue(line);
    }
    
    
   
    
}