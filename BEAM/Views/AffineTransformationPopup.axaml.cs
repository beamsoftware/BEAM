using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BEAM.ViewModels;

namespace BEAM.Views;

public partial class AffineTransformationPopup : Window
{
    public AffineTransformationPopup(SequenceViewModel model)
    {
        DataContext = new AffineTransformationPopupViewModel(model);
        InitializeComponent();
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TrySave(object? sender, RoutedEventArgs e)
    {
        if (((DataContext as AffineTransformationPopupViewModel)!).Save())
        {
            Close();
        }

        // If execution is here -> Save failed, hints in controls
    }
}