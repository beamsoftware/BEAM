using System.Collections.Generic;
using System.Linq;
using ScottPlot.Avalonia;

namespace BEAM.ImageSequence.Synchronization;

/// <summary>
/// A controller responsible for storing all active Syncronisation sets in the form of <see cref="SyncedPlotController"/>s.
/// </summary>
public static class PlotControllerManager
{
    /// <summary>
    /// All stored <see cref="SyncedPlotController"/>s. Every one represents a set of synchronized plots.
    /// </summary>
    private static readonly List<SyncedPlotController> PlotControllers = [];

    /// <summary>
    /// Registers and thereby stores a new synchronization set.
    /// </summary>
    /// <param name="controller">The new synchronization set.</param>
    /// <returns>A Boolean representing whether the operation was successful or not.
    /// Adding an existing set will lead to an unsuccessful operation.</returns>
    public static bool RegisterController(SyncedPlotController controller)
    {
        if(PlotControllers.Contains(controller))
        {
            return false;
        }
        PlotControllers.Add(controller);
        return true;
    }
    
    /// <summary>
    /// Removes an already existing and registered synchronization set from the existing ones.
    /// </summary>
    /// <param name="controller">The synchronization set to be removed.</param>
    /// <returns>A Boolean representing whether the operation was successful or not.
    /// Removing a set which was not stored will lead to an unsuccessful operation.</returns>
    public static bool Unregister(SyncedPlotController controller)
    {
        if(!PlotControllers.Contains(controller))
        {
            return false;
        }
        PlotControllers.Remove(controller);
        return true;
    }
    
    /// <summary>
    /// Adds a plot to all stored synchronization sets.
    /// </summary>
    /// <param name="plot">The AvaPlot, which will be added.</param>
    /// <returns>A Boolean indicating, whether the plot was added successfully.</returns>
    public static bool AddPlotToAllControllers(AvaPlot plot)
    {
        return PlotControllers.All(controller => controller.AddPlot(plot));
    }

    /// <summary>
    /// Removes a plot from all stored synchronization sets.
    /// </summary>
    /// <param name="plot">The AvaPlot, which will be removed.</param>
    /// <returns>A Boolean indicating, whether the plot was removed successfully.</returns>
    public static bool RemovePlotFromAllControllers(AvaPlot plot)
    {
        return PlotControllers.All(controller => controller.RemovePlot(plot));
    }

    public static void activateSynchronization()
    {
        foreach (var controller in PlotControllers)
        {
            controller.activate();
        }
    }
    
    public static void deactivateSynchronization()
    {
        foreach (var controller in PlotControllers)
        {
            controller.deactivate();
        }
    }
}