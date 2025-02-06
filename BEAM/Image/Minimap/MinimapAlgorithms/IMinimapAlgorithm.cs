using BEAM.ImageSequence;

namespace BEAM.Image.Minimap.MinimapAlgorithms;

/// <summary>
/// An algorithm which calculates values for each pixel line in a sequence.
/// How and based on which parameters these values are being calculated are heavily depended on the concrete implementation.
/// </summary>
public interface IMinimapAlgorithm
{
    /// <summary>
    /// A concrete implementation of an algorithms which generates values for all lines of a sequence.
    /// </summary>
    /// <param name="sequence"> The sequence based on which the values are being calculated.</param>
    /// <returns> A Boolean representing whether the generation finished successfully.</returns>
    bool AnalyzeSequence(Sequence sequence);

    /// <summary>
    /// Returns the algorithm calculation based value for a specific line. Commonly used for plotting.
    /// </summary>
    /// <param name="line">The line whose value shall be returned.</param>
    /// <returns>The specified line's calculated value.</returns>
    float GetLineValue(long line);
}