using Avalonia.Controls;
using BEAM.Renderer;
using BEAM.ViewModels;
using BEAM.Views.Utility;

namespace BEAM.Controls;

/// The view class for managing the settings of a <see cref="ChannelMapRenderer"/>.
public partial class ChannelMapConfigControlView : SaveUserControl
{
    public ChannelMapConfigControlView(ChannelMapRenderer renderer, SequenceViewModel model)
    {
        DataContext = new ChannelMapConfigControlViewModel(renderer, model);
        InitializeComponent();
    }

    public override void Save()
    {
        ((DataContext as ChannelMapConfigControlViewModel)!).Save();
    }
}