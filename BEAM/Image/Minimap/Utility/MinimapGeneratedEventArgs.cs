using System;

namespace BEAM.Image.Minimap.Utility;

/// <summary>
/// Event arguments for the event which is raised when a minimap finishes generating its values for all lines of a sequence.
/// </summary>
public class MinimapGeneratedEventArgs(PlotMinimap origin, MinimapGenerationResult result) : EventArgs
{
    /// <summary>
    /// The minimap which the EventArgs reference.
    /// </summary>
    public PlotMinimap Minimap { get; init; } = origin;
    /// <summary>
    /// The result fo the specified minimaps generation process.
    /// MinimapGenerationResult.Failure means the generation failed hence the minimap is not operational.
    /// </summary>
    public MinimapGenerationResult MinimapResult { get; init; } = result;
}