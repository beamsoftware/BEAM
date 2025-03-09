using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BEAM.Renderer;
using BEAM.ViewModels;
using BEAM.Views.Utility;

namespace BEAM.Controls;

public partial class ArgMaxHSVConfigControlView : SaveUserControl
{

    public ArgMaxHSVConfigControlView(ArgMaxRendererColorHSV renderer, SequenceViewModel model)
    {
        DataContext = new ArgMaxHSVConfigControlViewModel(renderer, model);
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void Save()
    {
        ((DataContext as ArgMaxHSVConfigControlViewModel)!).Save();
    }
}