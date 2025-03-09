using Avalonia.Controls;
using BEAM.Renderer;
using BEAM.ViewModels;
using BEAM.Views.Utility;

namespace BEAM.Controls;

/// The view class for managing the settings of a <see cref="HeatMapRenderer"/>.
public partial class HeatMapConfigControlView : SaveUserControl
{
    public HeatMapConfigControlView(HeatMapRenderer renderer, SequenceViewModel model)
    {
        DataContext = new HeatMapConfigControlViewModel(renderer, model);
        InitializeComponent();
    }

    public override void Save()
    {
        ((DataContext as HeatMapConfigControlViewModel)!).Save();
    }
}