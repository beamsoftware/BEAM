using System;
using System.Threading;
using BEAM.Image.Minimap.Utility;
using BEAM.ImageSequence;
using BEAM.Renderer;
using BEAM.ViewModels;
using BEAM.Views.Utility;

namespace BEAM.Image.Minimap;

/// <summary>
/// An abstraction of a minimap which is based on a sequence. It displays
/// information about a sequence's contents through visual means to the user.
/// It must therefore specify its view itself.
///
/// Beware: Any concrete class implementing this class must provide a parameterless constructor due to reflection invocation.
/// </summary>
public abstract class Minimap

{
    /// <summary>
    /// The sequence on which the minimap is based.
    /// </summary>
    protected ISequence? Sequence;
    protected bool IsGenerated { get; set; }

    /// <summary>
    /// Cancellation Token for the generation process. Any subclass should use this Token for its Threads
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; private set; } = new();
    
    /// <summary>
    /// A function blueprint for the callback function which is called concurrently/in parallel
    /// when the data generation process has finished.
    /// </summary>
    public delegate void MinimapGeneratedEventHandler(object sender, MinimapGeneratedEventArgs e);
    protected event MinimapGeneratedEventHandler? MinimapGenerated;

    /// <summary>
    /// Interrupts the generation process as well as all additional threads running.
    /// Useful if the corresponding sequence has been closed, yet the minimap is still running operations.
    /// </summary>
    public void StopGeneration()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
        CancellationTokenSource = new CancellationTokenSource();
    }
    
    /// <summary>
    /// Method for this class subclasses to call whenever they have
    /// finished their generation process so the original caller can be informed.
    /// </summary>
    /// <param name="e">The <see cref="MinimapGeneratedEventArgs"/> which will be used to Invoke the event.
    /// They also specify the caller.</param>
    protected  void OnMinimapGenerated(MinimapGeneratedEventArgs e)
    {
        if (MinimapGenerated is null) return;
        MinimapGenerated.Invoke(e.Minimap, e);
    }

    /// <summary>
    /// Returns the name of this Minimap. Will be used for displaying purposes.
    /// </summary>
    /// <returns>A string representing the name.</returns>
    protected abstract String GetName();
    
    /// <summary>
    /// Returns the settings popup control for this minimap. This SaveUserControl is responsible for changing settings.
    /// </summary>
    /// <returns>The corresponding SaveUserControl or null,
    /// if no settings need to be changed and no custom UI is meant to be displayed.</returns>
    public abstract SaveUserControl? GetSettingsPopupControl();
    
    

    /// <summary>
    /// Returns a deep clone of this minimap. A clone must especially keep the changable settings of its origin.
    /// </summary>
    /// <returns></returns>
    public abstract Minimap Clone();

    /// <summary>
    /// An attribute wrapper for this minimap's name.
    /// </summary>
    public String Name => GetName();
    
    
    /// <summary>
    /// Start the generation of the minimap based on the given sequence.
    /// It is recommended that the actual generation process is done in a separate thread.
    /// </summary>
    /// <param name="sequence">The sequence based on whose data the minimap is meant to be generated.</param>
    /// <param name="eventCallbackFunc">The function on which the caller will be informed
    /// once the generation has finished and the minimap can be displayed.</param>
    public abstract void StartGeneration(ISequence sequence, MinimapGeneratedEventHandler eventCallbackFunc);

    /// <summary>
    /// Sets the renderer for this Minimap. This is required if the minimap want to evaluate
    /// data based on the resulting pixel which is being displayed to the user. Must usually be called before GenerateMinimap.
    /// </summary>
    /// <param name="renderer">The new renderer that is currently being used to display an excerpt of the sequence to the user.</param>
    public abstract void SetRenderer(SequenceRenderer renderer);

    /// <summary>
    /// Returns the ViewModel for the UI excerpt of this minimap.
    /// This is the UI element the user actually gets to see.
    /// The corresponding view will be created automatically with a parameterless constructor.
    /// </summary>
    /// <returns>The ViewModel of the UI element.</returns>
    public abstract ViewModelBase GetDisplayableViewModel();
}