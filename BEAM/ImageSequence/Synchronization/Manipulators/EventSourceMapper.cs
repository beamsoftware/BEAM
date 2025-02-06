using System.Collections.Generic;
using Avalonia.Interactivity;
using ScottPlot.Avalonia;

namespace BEAM.ImageSequence.Synchronization.Manipulators;

/// <summary>
/// A static class used to register and temporarily store Events, that are supposed
/// to be saved with their corresponding source. Used to check, if a given Event originated
/// from a specific plot or not. Can only save a specified amount of events and their sources,
/// before the oldest ones will be deleted.
/// </summary>
public static class EventSourceMapper
{
    /// <summary>
    /// Used to save the order in which the events were stored.
    /// </summary>
    private static Queue<RoutedEventArgs> _eventQueue = new();
    /// <summary>
    /// The events mapped to their specific plot sources, from which the event originated,
    /// </summary>
    private static Dictionary<RoutedEventArgs, AvaPlot?> _sourcePlots = new();

    /// <summary>
    /// Add an event and its source plot to the mapping.
    /// </summary>
    /// <param name="eventSource">The event, which will be stored.</param>
    /// <param name="plot">The source plot from which the event originated.</param>
    public static void AddEventSource(RoutedEventArgs eventSource, AvaPlot? plot)
    {
        if (_eventQueue.Count >= 100)
        {
            var e = _eventQueue.Dequeue();
            _sourcePlots.Remove(e);
        }
        _eventQueue.Enqueue(eventSource);
        _sourcePlots.Add(eventSource, plot);
    }
    
    /// <summary>
    /// Checks whether a given plot is the source of the given event.
    /// </summary>
    /// <param name="e">The event for which the source will be checked.</param>
    /// <param name="plot">The plot which will be checked for being the source.</param>
    /// <returns>A Boolean representing, if the plot is the source of the event.</returns>
    public static bool IsSource(RoutedEventArgs e, AvaPlot plot) {
        if (_sourcePlots.TryGetValue(e, out var source))
        {
            return source != null && source.Equals(plot);
        }
        return false;
    }

    /// <summary>
    /// Adds a given event and source plot mapping, if the event is
    /// not already mapped to another (or the same) plot.
    /// </summary>
    /// <param name="e">The event, which will be mapped to the plot.</param>
    /// <param name="plot">The plot, which will be mapped to the event.</param>
    public static void AddIfNotExists(RoutedEventArgs e, AvaPlot plot)
    {
        if (!_sourcePlots.ContainsKey(e))
        {
            AddEventSource(e, plot);
        }
    }
    
    /// <summary>
    /// Clear the event queue and the event and plots mapping.
    /// </summary>
    public static void Clear()
    {
        _eventQueue.Clear();
        _sourcePlots.Clear();
    }
}