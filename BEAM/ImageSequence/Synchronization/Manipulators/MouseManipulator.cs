using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Avalonia.Interactivity;
using BEAM.Log;
using NP.Ava.Visuals;
using ScottPlot;
using ScottPlot.Avalonia;
using Svg;

namespace BEAM.ImageSequence.Synchronization.Manipulators;

/// <summary>
/// A class used to share mouse events between a group of Avaplots.
/// </summary>
public class MouseManipulator : Manipulator
{
    /// <summary>
    /// The list of the plots, which will share mouse events.
    /// </summary>
    private List<AvaPlot> _avaPlots = [];

    private bool _isSynchronizing = false;
    
    /// <summary>
    /// Synchronizes a plot with all the other plots, which are already registered.
    /// </summary>
    /// <param name="avaPlot">The AvaPlot, which will be synchronized.</param>
    /// <returns>A Boolean, indicating, if the given AvaPlot was successfully synchronized.</returns>
    public override bool SyncPlot(AvaPlot? avaPlot)
    {
        if (avaPlot == null)
        {
            return false;
        }
        
        _avaPlots.Add(avaPlot);
        
        avaPlot.PointerEntered += (s, e) =>
        {
            if (!_isSynchronizing)
            {
                return;
            }
            
            EventSourceMapper.AddIfNotExists(e, avaPlot);
            if (EventSourceMapper.IsSource(e, avaPlot))
            {
                foreach (var plot in _avaPlots.Where(p => p != avaPlot))
                {
                    plot.RaiseEvent(e);
                }
            }
        };

        avaPlot.PointerExited += (s, e) =>
        {
            if (!_isSynchronizing)
            {
                return;
            }
            
            EventSourceMapper.AddIfNotExists(e, avaPlot);
            if (EventSourceMapper.IsSource(e, avaPlot))
            {
                foreach (var plot in _avaPlots.Where(p => p != avaPlot))
                {
                    plot.RaiseEvent(e);
                }
            }
        };

        avaPlot.PointerMoved += (s, e) =>
        {
            if (!_isSynchronizing)
            {
                return;
            }
            
            EventSourceMapper.AddIfNotExists(e, avaPlot);
            if (EventSourceMapper.IsSource(e, avaPlot))
            {
                foreach (var plot in _avaPlots.Where(p => p != avaPlot))
                {
                    plot.RaiseEvent(e);
                }
            }
        };

        avaPlot.PointerPressed += (s, e) =>
        {
            if (!_isSynchronizing)
            {
                return;
            }
            
            // check for right click -> only opens context push menu (scottplot intern)
            if (e.GetCurrentPoint(avaPlot).Properties.IsRightButtonPressed)
            {
                return;
            }
            
            EventSourceMapper.AddIfNotExists(e, avaPlot);
            if (EventSourceMapper.IsSource(e, avaPlot))
            {
                foreach (var plot in _avaPlots.Where(p => p != avaPlot))
                {
                    plot.RaiseEvent(e);
                }
            }
        };

        avaPlot.PointerReleased += (s, e) =>
        {
            if (!_isSynchronizing)
            {
                return;
            }
            
            EventSourceMapper.AddIfNotExists(e, avaPlot);
            if (EventSourceMapper.IsSource(e, avaPlot))
            {
                foreach (var plot in _avaPlots.Where(p => p != avaPlot))
                {
                    plot.RaiseEvent(e);
                }
            }
        };

        avaPlot.PointerWheelChanged += (s, e) =>
        {
            if (!_isSynchronizing)
            {
                return;
            }
            
            foreach (var plot in _avaPlots.Where(p => p != avaPlot))
            {
                plot.Plot.Axes.SetLimits(avaPlot.Plot.Axes.GetLimits());
                plot.Refresh();
                ScrollingSynchronizer.UpdateOwnScrollBar(plot);
            }   
        };

        avaPlot.Tapped += (s, e) =>
        {
            if (!_isSynchronizing)
            {
                return;
            }
            
            EventSourceMapper.AddIfNotExists(e, avaPlot);
            if (EventSourceMapper.IsSource(e, avaPlot))
            {
                foreach (var plot in _avaPlots.Where(p => p != avaPlot))
                {
                    plot.RaiseEvent(e);
                }
            }
        };

        avaPlot.DoubleTapped += (s, e) =>
        {
            if (!_isSynchronizing)
            {
                return;
            }
            
            EventSourceMapper.AddIfNotExists(e, avaPlot);
            if (EventSourceMapper.IsSource(e, avaPlot))
            {
                foreach (var plot in _avaPlots.Where(p => p != avaPlot))
                {
                    plot.RaiseEvent(e);
                }
            }
        };

        avaPlot.Holding += (s, e) =>
        {
            if (!_isSynchronizing)
            {
                return;
            }
            
            EventSourceMapper.AddIfNotExists(e, avaPlot);
            if (EventSourceMapper.IsSource(e, avaPlot))
            {
                foreach (var plot in _avaPlots.Where(p => p != avaPlot))
                {
                    plot.RaiseEvent(e);
                    //ScrollingSynchronizer.UpdateOwnScrollBar(plot);
                }
            }
        };
            
        return true;
    }

    /// <summary>
    /// Removes an AvaPlot, so that it is no longer synchronized with the other plots.
    /// </summary>
    /// <param name="avaPlot">The AvaPlot, which synchronization will be removed.</param>
    /// <returns>A Boolean indicating, if the plot was removed successfully.</returns>
    public override bool UnsyncPlot(AvaPlot? avaPlot)
    {
        return avaPlot != null && _avaPlots.Remove(avaPlot);
    }
    
    public override void activate()
    {
        _isSynchronizing = true;
    }
    
    public override void deactivate()
    {
        _isSynchronizing = false;
    }
}