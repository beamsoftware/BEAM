using BEAM.Image;
using BEAM.Renderer;

namespace BEAM.ImageSequence;

/// <summary>
/// Base interface for sequences.
/// </summary>
public interface ISequence : IImage
{
    /// <summary>
    /// Name of the sequence.
    /// The user can distinguish between sequences using this name.
    /// </summary>
    /// <returns>The name of the sequence</returns>
    public string GetName();

    public (double min, double max) GetDefaultMinMaxValues();
    public RenderTypes GetDefaultRenderType();
}