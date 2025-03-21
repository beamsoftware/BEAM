using BEAM.Image.Minimap.MinimapAlgorithms;
using BEAM.ViewModels.Minimap.Popups.EmbeddedSettings;
using BEAM.Views.Utility;

namespace BEAM.Views.Minimap.Popups.EmbeddedSettings;


/// <summary>
/// Control view for configuring the pixel
/// threshold algorithm that sums up all pixels who have a channel whose value is larger than a baseline.
/// </summary>
public partial class PixelThresholdAnySumAlgorithmConfigControlView : SaveUserControl
{
    /// <summary>
    /// Initializes a new instance of the configuration view with the specified algorithm.
    /// </summary>
    /// <param name="algorithm">The algorithm instance to be configured.</param>
    public PixelThresholdAnySumAlgorithmConfigControlView(RenderedPixelAnyThresholdAlgorithm algorithm)
    {
        InitializeComponent();
        DataContext = new PixelThresholdAnySumAlgorithmConfigControlViewModel(algorithm);
    }

    /// <summary>
    /// Saves the current configuration settings through the view model.
    /// </summary>
    public override void Save()
    {
        (DataContext as PixelThresholdAnySumAlgorithmConfigControlViewModel)!.Save();
    }
}