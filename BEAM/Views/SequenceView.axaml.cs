using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using BEAM.CustomActions;
using BEAM.IMage.Displayer.Scottplot;
using BEAM.ImageSequence;
using BEAM.ImageSequence.Synchronization;
using BEAM.Log;
using BEAM.Profiling;
using BEAM.ViewModels;
using ScottPlot;
using ScottPlot.Interactivity;
using ScottPlot.Interactivity.UserActionResponses;

namespace BEAM.Views;

public partial class SequenceView : UserControl
{
    public SequenceView()
    {
        InitializeComponent();
    }

    private void FillPlot(Sequence sequence)
    {
        _ApplyDarkMode();
        _BuildCustomRightClickMenu();

        // TODO: CustomMouseActions
        // https://github.com/ScottPlot/ScottPlot/blob/main/src/ScottPlot5/ScottPlot5%20Demos/ScottPlot5%20WinForms%20Demo/Demos/CustomMouseActions.cs

        //avaPlot1.Interaction.IsEnabled = true;
        //avaPlot1.UserInputProcessor.IsEnabled = true;
        //avaPlot1.UserInputProcessor.UserActionResponses.Clear();

        //var panButton = ScottPlot.Interactivity.StandardMouseButtons.Middle;
        //var panResponse = new ScottPlot.Interactivity.UserActionResponses.MouseDragPan(panButton);
        AvaPlot1.UserInputProcessor.RemoveAll<MouseWheelZoom>();
        AvaPlot1.UserInputProcessor.UserActionResponses.Add(new CustomMouseWheelZoom(StandardKeys.Shift, StandardKeys.Control));
        AvaPlot1.PointerWheelChanged += (sender, e) =>
        {
            if (e.KeyModifiers == KeyModifiers.Control)
            { 
                var pixel = new Pixel(e.GetPosition(AvaPlot1).X, e.GetPosition(AvaPlot1).Y); 
                AvaPlot1.Plot.Axes.Zoom(pixel, 1 + 0.15 * e.Delta.X, 1 + 0.15 * e.Delta.Y);
            }
            else
            {
                AvaPlot1.Plot.Axes.ZoomIn(1 + 0.15 * e.Delta.X, 1 + 0.15 * e.Delta.Y);
            }
        };
        PlotControllerManager.AddPlotToAllControllers(AvaPlot1);
        using var _ = Timer.Start();

        AvaPlot1.Plot.Axes.InvertY();
        AvaPlot1.Plot.Axes.SquareUnits();

        var plottable = new BitmapPlottable(sequence);
        AvaPlot1.Plot.Add.Plottable(plottable);

        plottable.SequenceImage.RequestRefreshPlotEvent += (sender, args) => AvaPlot1.Refresh();
        AvaPlot1.Refresh();
        
    }

    private void _ApplyDarkMode()
    {
        if (Application.Current!.ActualThemeVariant != ThemeVariant.Dark) return;

        // change figure colors
        AvaPlot1.Plot.FigureBackground.Color = Color.FromHex("#181818");
        AvaPlot1.Plot.DataBackground.Color = Color.FromHex("#1f1f1f");

        // change axis and grid colors
        AvaPlot1.Plot.Axes.Color(Color.FromHex("#d7d7d7"));
        AvaPlot1.Plot.Grid.MajorLineColor = Color.FromHex("#404040");

        // change legend colors
        AvaPlot1.Plot.Legend.BackgroundColor = Color.FromHex("#404040");
        AvaPlot1.Plot.Legend.FontColor = Color.FromHex("#d7d7d7");
        AvaPlot1.Plot.Legend.OutlineColor = Color.FromHex("#d7d7d7");
    }

    private void _BuildCustomRightClickMenu()
    {
        var menu = AvaPlot1.Menu!;
        menu.Clear();
        menu.Add("Inspect Pixel",
            control => Logger.GetInstance().Warning(LogEvent.BasicMessage, "Not implemented yet!"));
        menu.AddSeparator();
        menu.Add("Sync to this",
            control => Logger.GetInstance().Warning(LogEvent.BasicMessage, "Not implemented yet!"));
        menu.AddSeparator();
        menu.Add("Configure colors",
            control => Logger.GetInstance().Warning(LogEvent.BasicMessage, "Not implemented yet!"));
        menu.Add("Affine Transformation",
            control => Logger.GetInstance().Warning(LogEvent.BasicMessage, "Not implemented yet!"));
        menu.AddSeparator();
        menu.Add("Export sequence",
            control => Logger.GetInstance().Warning(LogEvent.BasicMessage, "Not implemented yet!"));
    }

    private void StyledElement_OnDataContextChanged(object? sender, EventArgs e)
    {
        var vm = DataContext as SequenceViewModel;

        FillPlot(vm.Sequence);
    }
}