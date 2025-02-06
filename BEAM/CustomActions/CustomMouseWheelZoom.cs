using System;
using ScottPlot;
using ScottPlot.Interactivity;
using ScottPlot.Interactivity.UserActions;

namespace BEAM.CustomActions;

/// <summary>
/// A class used to implement the custom mouse wheel zoom functionality, wanted by the project.
/// Using the Mouse Wheel scrolls through the plot, while holding the Control key and scrolling zooms in and out.
/// </summary>
/// <param name="horizontalLockKey">The Key, which locks the Zooming to the horizontal axis.</param>
/// <param name="verticalLockKey">The Key, which locks the zooming to the vertical axis.</param>
public class CustomMouseWheelZoom(Key horizontalLockKey, Key verticalLockKey) : IUserActionResponse
{
    /// <summary>
    /// If enabled, when the mouse zooms while hovered over an axis only that axis will be changed.
    /// </summary>
    public bool ZoomAxisUnderMouse { get; set; } = true;

    /// <summary>
    /// If enabled with <see cref="ZoomAxisUnderMouse"/>, all axes of the same direction will be changed together.
    /// </summary>
    public bool LockParallelAxes { get; set; } = false;

    Key LockHorizontalKey { get; } = horizontalLockKey;

    Key LockVerticalKey { get; } = verticalLockKey;

    public void ResetState(Plot plot) { }

    /// <summary>
    /// Fraction of the axis range to change when zooming in and out.
    /// </summary>
    public double ZoomFraction { get; set; } = 0.15;
    
    /// <summary>
    /// Fraction of the axis range to change when scrolling.
    /// </summary>
    public double ScrollFraction { get; set; } = 0.075;

    private double ZoomInFraction => 1 + ZoomFraction;
    private double ZoomOutFraction => 1 / ZoomInFraction;

    public ResponseInfo Execute(Plot plot, IUserAction userInput, KeyboardState keys)
    {
        if (keys.IsPressed(StandardKeys.Control))
        {
            if (userInput is MouseWheelUp mouseDownInput)
            {
                double xFrac = keys.IsPressed(LockHorizontalKey) ? 1 : ZoomInFraction;
                double yFrac = keys.IsPressed(LockVerticalKey) ? 1 : ZoomInFraction;
                MouseAxisManipulation.MouseWheelZoom(plot, xFrac, yFrac, mouseDownInput.Pixel, LockParallelAxes);
                return new ResponseInfo() { RefreshNeeded = true };
            }

            if (userInput is MouseWheelDown mouseUpInput)
            {
                double xFrac = keys.IsPressed(LockHorizontalKey) ? 1 : ZoomOutFraction;
                double yFrac = keys.IsPressed(LockVerticalKey) ? 1 : ZoomOutFraction;
                MouseAxisManipulation.MouseWheelZoom(plot, xFrac, yFrac, mouseUpInput.Pixel, LockParallelAxes);
                return new ResponseInfo() { RefreshNeeded = true };
            }

            return ResponseInfo.NoActionRequired;   
        }
        else
        {
            if (userInput is MouseWheelUp mouseDownInput)
            {
                var ySize = plot.Axes.GetLimits().Bottom - plot.Axes.GetLimits().Top;
                var pixel = new Pixel(mouseDownInput.Pixel.X, mouseDownInput.Pixel.Y + ScrollFraction * ySize);
                MouseAxisManipulation.DragPan(plot, mouseDownInput.Pixel, pixel);
                return new ResponseInfo() { RefreshNeeded = true };
            }
            
            if (userInput is MouseWheelDown mouseUpInput)
            {
                var ySize = plot.Axes.GetLimits().Bottom - plot.Axes.GetLimits().Top;
                var pixel = new Pixel(mouseUpInput.Pixel.X, mouseUpInput.Pixel.Y - ScrollFraction * ySize);
                MouseAxisManipulation.DragPan(plot, mouseUpInput.Pixel, pixel);
                return new ResponseInfo() { RefreshNeeded = true };
            }
            return ResponseInfo.NoActionRequired;
        }
    }
}
