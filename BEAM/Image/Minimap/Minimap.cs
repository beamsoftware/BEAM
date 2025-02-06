using System;
using System.Threading;
using Avalonia.Controls;
using BEAM.Image.Minimap.Utility;
using BEAM.ImageSequence;

namespace BEAM.Image.Minimap;

/// <summary>
/// An abstraction of a minimap which is based on a sequence. It displays
/// information about a sequence's contents through visual means to the user.
/// </summary>
public abstract class Minimap

{
    /// <summary>
    /// The sequence based on which the minimap is based.
    /// </summary>
    protected readonly Sequence Sequence;
    protected virtual bool IsGenerated { get; set; } = false;
    /// <summary>
    /// Cancellation Token for the generation process. Any subclass should use this Token for its Threads
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; init; }
    
    /// <summary>
    /// A function blueprint for the callback function which is called concurrently/in parallel
    /// when the data generation process has finished.
    /// </summary>
    public delegate void MinimapGeneratedEventHandler(object sender, MinimapGeneratedEventArgs e);
    protected event MinimapGeneratedEventHandler MinimapGenerated;
    
    
    /// <summary>
    /// The UI-Element which is displayed in the view.
    /// </summary>
    protected UserControl? DisplayedMinimap;
    
    
    /// <summary>
    /// Initializes the minimap creation process. It is intended to create a separately running Task which generates the values.
    /// Therefor, the minimap is not instantly ready after this method call ends hence a method which is used as a callback must be supplied.
    /// </summary>
    /// <param name="sequence">The sequence based on which the minimap is based.</param>
    /// <param name="eventCallbackFunc">A function which is invoked once the minimap has finished generating its values.
    /// This is being done through the <see cref="MinimapGeneratedEventHandler"/> event.</param>
    /// <exception cref="ArgumentNullException">If any of the parameters is null.</exception>
    public Minimap(Sequence sequence, MinimapGeneratedEventHandler eventCallbackFunc )
    {
        ArgumentNullException.ThrowIfNull(sequence);
        ArgumentNullException.ThrowIfNull(eventCallbackFunc);
        this.Sequence = sequence;
        CancellationTokenSource = new();
        MinimapGenerated += eventCallbackFunc;
    }
    
    /// <summary>
    /// Returns a UI element which can be included by a view.
    /// </summary>
    /// <returns>The UI-Element as a <see cref="Avalonia.Controls.UserControl"/></returns>
    public  UserControl GetMinimap()
    {
        if (!IsGenerated || DisplayedMinimap is null)
        {
            throw new InvalidOperationException();
        }
        return DisplayedMinimap;
    }

    /// <summary>
    /// Interrupts the generation process as well as all additional threads running.
    /// Useful if the corresponding sequence has been closed, yet the minimap is still running operations.
    /// </summary>
    public void StopGeneration()
    {
        CancellationTokenSource.Cancel();
    }
    
    /// <summary>
    /// Method for this class subclasses to call whenever they have
    /// finished their generation process so the original caller can be informed.
    /// </summary>
    /// <param name="e">The <see cref="MinimapGeneratedEventArgs"/> which will be used to Invoke the event.
    /// They also specify the caller.</param>
    protected  void OnMinimapGenerated(MinimapGeneratedEventArgs e)
    {
        // TODO: e.Minimap vs this
        MinimapGenerated.Invoke(e.Minimap, e);
    }
}