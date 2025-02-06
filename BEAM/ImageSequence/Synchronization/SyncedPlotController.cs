using System.Collections.Generic;
using System.Linq;
using BEAM.ImageSequence.Synchronization.Manipulators;
using ScottPlot.Avalonia;

namespace BEAM.ImageSequence.Synchronization;

/// <summary>
/// A class responsible for handling the synchronization of certain attributes (defined by the manipulators this class stores)
/// between different plots (defined by the plots this class stores).
/// </summary>
public class SyncedPlotController
{
    /// <summary>
    /// THe plots which are being synced.
    /// </summary>
    private List<AvaPlot> Plots { get; init; } = [];
    /// <summary>
    /// The manipulators which sync the plot's attributes. Different manipulators sync different attributes.
    /// </summary>
    private List<Manipulator> Manipulators { get; init; } = [];
    
    /// <summary>
    /// Registers and thereby adds a new manipulator which is meant to sync new attributes between plots.
    /// It will start syncing all plots which have already been added.
    /// The manipulator will not be removed if it encounters an error with syncing any plot.
    /// Instead, unsyncing all plots will be attempted.
    /// </summary>
    /// <param name="manipulator">The new manipulator to be added.
    /// It should not overlap with the synchronisation of any other already existing manipulator.</param>
    /// <returns>A Boolean representing whether the operation was successful.
    /// Adding an already existing manipulator or the manipulator having issues with adding a plot can be sources of errors.</returns>
    public bool Register(Manipulator manipulator)
    {
        if (Manipulators.Contains(manipulator))
        {
            return false;
        }
        if (manipulator.SyncAllPlots(Plots))
        {
            Manipulators.Add(manipulator);
            return true;
        }

        manipulator.UnsyncAllPlots(Plots);
        return false;
    }
    
    /// <summary>
    /// Removes an existing manipulator which was meant to sync new attributes between plots.
    /// It will unsync all plots which have already been added by this controller.
    /// The manipulator will not be removed if it encounters an error with unsyncing any plot.
    /// </summary>
    /// <param name="manipulator">The manipulator which is meant to be removed.</param>
    /// <returns>A Boolean representing whether the operation was successful.
    /// Removing a not previously added manipulator or the manipulator having issues with removing a plot can be sources of errors.</returns>
    public bool Remove(Manipulator manipulator)
    {

        if (!Manipulators.Contains(manipulator))
        {
            return false;
        }

        if (!manipulator.UnsyncAllPlots(Plots)) return false;
        Manipulators.Remove(manipulator);
        return true;
    }

    /// <summary>
    /// Adds a new plot to the set of synchronized plots. It will be synced to all manipulators. 
    /// </summary>
    /// <param name="plot">The new plot to be added.</param>
    /// <returns>A Boolean representing whether the operation was successful.
    /// Adding an already existing plot or the manipulators having issues with adding the plot may be sources of errors.</returns>
    public bool AddPlot(AvaPlot plot)
    {
        if (Plots.Contains(plot))
        {
            return false;
        }
        
        Plots.Add(plot);

        return Manipulators.All(manipulator => manipulator.SyncPlot(plot));
    }
    
    /// <summary>
    /// Removes a previously added plot from the set of synchronized plots. It will be no longer synced to any manipulators from this controller.
    /// </summary>
    /// <param name="plot">The plot which is meant to be removed.</param>
    /// <returns>A Boolean representing whether the operation was successful.
    /// Removing a not previously added plot or the manipulators having issues with adding the plot may be sources of errors.</returns>
    public bool RemovePlot(AvaPlot plot)
    {
        if (!Plots.Contains(plot))
        {
            return false;
        }

        Plots.Remove(plot);

        return Manipulators.All(manipulator => manipulator.UnsyncPlot(plot));
    }

    public void activate()
    {
        foreach (var manipulator in Manipulators)
        {
            manipulator.activate();
        }
    }
    
    public void deactivate()
    {
        foreach (var manipulator in Manipulators)
        {
            manipulator.deactivate();
        }
    }
}