using System.Collections.Generic;
using System.Linq;
using ScottPlot.Avalonia;

namespace BEAM.ImageSequence.Synchronization.Manipulators;

/// <summary>
/// A class which synchronizes certain attributes between different plots,like e.g. sideways movement or scrolling.
/// </summary>
public abstract class Manipulator
{
    /// <summary>
    /// Adds a plot to the set of synchronised plots. If only one plot has been added so far,
    /// the already added one will be used as a base for any future added plots.
    /// Meaning: The plots which has been added the longest time age should be used as a base.
    /// Returns true if plot is null.
    /// </summary>
    /// <param name="avaPlot">The plot which will be synced with the existing ones (if any).</param>
    /// <returns>A Boolean representing whether the operation as successful.</returns>
    public abstract bool SyncPlot(AvaPlot? avaPlot);
    
    /// <summary>
    /// Removes a plot from the set of synchronised plots.
    /// Returns true if plot is null.
    /// </summary>
    /// <param name="avaPlot">The plot which will no longer b synced with the existing ones (if any).</param>
    /// <returns>A Boolean representing whether the operation as successful.</returns>
    public abstract bool UnsyncPlot(AvaPlot? avaPlot);

    /// <summary>
    /// Unsync all plots actions according to the attributes this manipulator observers.
    /// None of the specified plots will therefore be synced anymore.
    /// </summary>
    /// <param name="plots">All plots which are meant to be desynced from one another and the other plots. </param>
    /// <returns>A Boolean representing whether the operation was successful for every plot. False if it failed during any plot's desynchronization.</returns>
    public bool UnsyncAllPlots(IEnumerable<AvaPlot>? plots)
    {
        return plots is null || plots.All(UnsyncPlot);
    }
    
    /// <summary>
    /// Sync all plots actions according to the attributes this manipulator observers.
    /// The first list entry will be used as the base (aligning every other entry's attributes to it) if no plot has previously been added.
    /// </summary>
    /// <param name="plots">All plots which are meant to be synced to one another. First entry is used as a base.</param>
    /// <returns>A Boolean representing whether the operation was successful for every plot. False if it failed during any plot's synchronization.</returns>
    public bool SyncAllPlots(IEnumerable<AvaPlot>? plots)
    {
        return plots is null || plots.All(SyncPlot);
    }

    public abstract void activate();
    
    public abstract void deactivate();
}