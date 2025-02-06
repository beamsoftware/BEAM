using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BEAM.Renderer;
using BEAM.ViewModels;

namespace BEAM.Controls;

public partial class ChannelMapConfigControlView : UserControl, ISaveControl
{
    public ChannelMapConfigControlView(ChannelMapRenderer renderer, SequenceViewModel model)
    {
        DataContext = new ChannelMapConfigControlViewModel(renderer, model);
        InitializeComponent();
    }

    public void Save()
    {
        ((DataContext as ChannelMapConfigControlViewModel)!).Save();
    }
}