using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BEAM.Renderer;
using BEAM.ViewModels;

namespace BEAM.Controls;

public partial class HeatMapConfigControlView : UserControl, ISaveControl
{
    public HeatMapConfigControlView(HeatMapRenderer renderer, SequenceViewModel model)
    {
        DataContext = new HeatMapConfigControlViewModel(renderer, model);
        InitializeComponent();
    }

    public void Save()
    {
        ((DataContext as HeatMapConfigControlViewModel)!).Save();
    }
}