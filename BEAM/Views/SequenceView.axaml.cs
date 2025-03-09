using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Styling;
using BEAM.CustomActions;
using BEAM.Datatypes;
using BEAM.Image.Displayer.Scottplot;
using BEAM.Image.Displayer.ScottPlot;
using BEAM.ImageSequence.Synchronization;
using BEAM.ViewModels;
using BEAM.ViewModels.Utility;
using NP.Ava.Visuals;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Interactivity;
using ScottPlot.Interactivity.UserActionResponses;
using ScottPlot.Plottables;
using SizeChangedEventArgs = Avalonia.Controls.SizeChangedEventArgs;

namespace BEAM.Views;

/// <summary>
/// User control for displaying and interacting with image sequences.
/// Provides plot visualization, scrolling, and mouse interaction capabilities.
/// </summary>
public partial class SequenceView : UserControl
{
    /// <summary>
    /// Label format for displaying pixel information (coordinates and RGB values).
    /// </summary>
    private const string PixelLabel = "x: {0, 10}\ny: {1, 10}\n \nR: {2, 10}\nG: {3,10}\nB: {4,10}";

    /// <summary>
    /// Scale factor for minimap width relative to the main view.
    /// </summary>
    private const double MinimapWidthScale = 0.07;

    /// <summary>
    /// Property for hosting external user controls.
    /// </summary>
    public static readonly StyledProperty<Control?> DynamicContentProperty =
        AvaloniaProperty.Register<SequenceView, Control?>(nameof(DynamicContent));


    /// <summary>
    /// Property for binding to the external user control's view model.
    /// </summary>
    public static readonly StyledProperty<object?> DynamicContentViewModelProperty =
        AvaloniaProperty.Register<SequenceView, object?>(nameof(DynamicContentViewModel));

    /// <summary>
    /// Gets or sets the dynamically loaded content control.
    /// </summary>
    public Control? DynamicContent
    {
        get => GetValue(DynamicContentProperty);
        set => SetValue(DynamicContentProperty, value);
    }
    
    private SequencePlottable? _plottable;
    private Annotation _anno;
    private readonly HorizontalLine _horizontalLine;
    private readonly VerticalLine _verticalLine;

    /// <summary>
    /// Gets or sets the view model for the dynamic content.
    /// </summary>
    public object? DynamicContentViewModel
    {
        get => GetValue(DynamicContentViewModelProperty);
        set => SetValue(DynamicContentViewModelProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the SequenceView control.
    /// Sets up plot lines, annotations, and event handlers.
    /// </summary>
    public SequenceView()
    {
        InitializeComponent();
        _horizontalLine = AvaPlot1.Plot.Add.HorizontalLine(0);
        _verticalLine = AvaPlot1.Plot.Add.VerticalLine(0);
        SizeChanged += OnSizeChanged;
        _anno = AvaPlot1.Plot.Add.Annotation(string.Format(PixelLabel, 0, 0, 0, 0, 0));
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var vm = DataContext as SequenceViewModel;
        if (vm == null || vm.MinimapVms.Count == 0)
        {
            return;
        }


        if (vm.MinimapVms.Any())
        {
            if (vm.MinimapVms.First() is not SizeAdjustableViewModelBase mapViewModel) return;
            mapViewModel.NotifySizeChanged(this, new ViewModels.Utility.SizeChangedEventArgs(e.NewSize.Width * MinimapWidthScale, e.NewSize.Height));
        }
    }

    private void PreparePlot()
    {
        _ApplyTheme();
        _BuildCustomRightClickMenu();

        ScrollingSynchronizerMapper.AddSequence(this);
        AvaPlot1.UserInputProcessor.RemoveAll<MouseWheelZoom>();
        AvaPlot1.UserInputProcessor.RemoveAll<MouseDragZoom>(); // Remove option to zoom with right key
        AvaPlot1.UserInputProcessor.UserActionResponses.Add(new CustomMouseWheelZoom(StandardKeys.Shift,
            StandardKeys.Control));

        // Add ability to select area with right mouse button pressed
        AvaPlot1.UserInputProcessor.UserActionResponses.Add(new CustomAreaSelection(StandardMouseButtons.Right));

        Bar1.Scroll += (_, e) =>
        {
            var vm = (DataContext as SequenceViewModel)!;
            var plot = AvaPlot1.Plot;
            // Difference of bottom and top sometimes negative? Math.Abs required.
            var ySize = Math.Abs(plot.Axes.GetLimits().Bottom - plot.Axes.GetLimits().Top);
            // Minus 100 to allow to scroll higher than the sequence for a better inspection of the start.
            var top = (e.NewValue / 100.0) * vm.Sequence.Shape.Height - 100.0;
            AvaPlot1.Plot.Axes.SetLimitsY(top, top + ySize);
            AvaPlot1.Refresh();
            ScrollingSynchronizerMapper.Synchronize(this);
        };

        AvaPlot1.PointerWheelChanged += (_, _) =>
        {
            UpdateScrollBar();
            ScrollingSynchronizerMapper.Synchronize(this);
        };

        AddScrollBarUpdating();

        PlotControllerManager.AddPlotToAllControllers(AvaPlot1);

        AvaPlot1.Plot.Axes.InvertY();
        AvaPlot1.Plot.Axes.SquareUnits();
        // Reset the axis to the initial value of the scrollbar.
        UpdateScrolling(0);
        AvaPlot1.Refresh();
    }

    private void AddScrollBarUpdating()
    {
        AvaPlot1.PointerEntered += (_, e) =>
        {
            var coordinates = AvaPlot1.Plot.GetCoordinates(new Pixel(e.GetPosition(AvaPlot1).X, e.GetPosition(AvaPlot1).Y));
            UpdatePositionAnnotation(coordinates.X, coordinates.Y);
            UpdateScrollBar();
        };

        AvaPlot1.PointerExited += (_, e) =>
        {
            var coordinates = AvaPlot1.Plot.GetCoordinates(new Pixel(e.GetPosition(AvaPlot1).X, e.GetPosition(AvaPlot1).Y));
            UpdatePositionAnnotation(coordinates.X, coordinates.Y);
            UpdateScrollBar();
        };

        AvaPlot1.PointerMoved += (_, e) =>
        {
            var coordinates = AvaPlot1.Plot.GetCoordinates(new Pixel(e.GetPosition(AvaPlot1).X, e.GetPosition(AvaPlot1).Y));
            UpdatePositionAnnotation(coordinates.X, coordinates.Y);
            UpdateScrollBar();
        };

        AvaPlot1.PointerPressed += (_, e) =>
        {
            var coordinates = AvaPlot1.Plot.GetCoordinates(new Pixel(e.GetPosition(AvaPlot1).X, e.GetPosition(AvaPlot1).Y));
            UpdatePositionAnnotation(coordinates.X, coordinates.Y);
            UpdateScrollBar();
        };

        AvaPlot1.PointerReleased += (_, e) =>
        {
            var coordinates = AvaPlot1.Plot.GetCoordinates(new Pixel(e.GetPosition(AvaPlot1).X, e.GetPosition(AvaPlot1).Y));
            UpdatePositionAnnotation(coordinates.X, coordinates.Y);
            UpdateScrollBar();
        };

        AvaPlot1.PointerCaptureLost += (_, _) =>
        {
            UpdateScrollBar();
        };

        AvaPlot1.PointerWheelChanged += (_, e) =>
        {
            var coordinates = AvaPlot1.Plot.GetCoordinates(new Pixel(e.GetPosition(AvaPlot1).X, e.GetPosition(AvaPlot1).Y));
            UpdatePositionAnnotation(coordinates.X, coordinates.Y);
            UpdateScrollBar();
        };
    }

    private void _ApplyTheme()
    {
        var currentTheme = Application.Current!.ActualThemeVariant;

        // change figure colors
        Application.Current.TryGetResource("WindowBg", currentTheme, out var background);
        var backgroundColor = (Avalonia.Media.Color)background!;
        AvaPlot1.Plot.FigureBackground.Color = new Color(backgroundColor.R, backgroundColor.G, backgroundColor.B);

        Application.Current.TryGetResource("BackgroundColorDark", currentTheme, out var backgroundDark);
        var backgroundColorDark = (Avalonia.Media.Color)backgroundDark!;
        AvaPlot1.Plot.DataBackground.Color =
            new Color(backgroundColorDark.R, backgroundColorDark.G, backgroundColorDark.B);

        // change axis and grid colors
        Application.Current.TryGetResource("FontColor", currentTheme, out var fontColorScottPlot);
        var fontColor = (Avalonia.Media.Color)fontColorScottPlot!;
        AvaPlot1.Plot.Axes.Color(new Color(fontColor.R, fontColor.G, fontColor.B));


        // AvaPlot1.Plot.Grid.MajorLineColor = Colors.Aqua;

        // change legend colors
        // AvaPlot1.Plot.Legend.BackgroundColor = Colors.Aqua;
        // AvaPlot1.Plot.Legend.FontColor = Colors.Aqua;
        // AvaPlot1.Plot.Legend.OutlineColor = Colors.Aqua;

        // AvaPlot1.Plot.Grid.MajorLineColor = Color.FromHex("#404040");
        //
        // // change legend colors
        // AvaPlot1.Plot.Legend.BackgroundColor = Color.FromHex("#404040");
        // AvaPlot1.Plot.Legend.FontColor = new Color(fontColor.R, fontColor.G, fontColor.B);
        // AvaPlot1.Plot.Legend.OutlineColor = Color.FromHex("#d7d7d7");
    }

    private void _BuildCustomRightClickMenu()
    {
        var vm = (SequenceViewModel?)DataContext;
        if (vm is null)
        {
            return;
        }

        var menu = AvaPlot1.Menu!;
        menu.Clear();

        menu.Add("Inspect",
            _ => _OpenInspectionViewModel());
        menu.AddSeparator();
        menu.Add("Sync to this",
            _ =>
            {
                ScrollingSynchronizerMapper.ActivateSynchronization();
                ScrollingSynchronizerMapper.Synchronize(this);
                PlotControllerManager.ActivateSynchronization();
            });
        menu.AddSeparator();
        menu.Add("Configure colors", _ => _OpenColorsPopup());
        menu.Add("Affine Transformation", _ => _OpenTransformPopup());
        menu.AddSeparator();
        menu.Add("Cut Sequence", _ => _OpenCutPopup());
        menu.Add("Export sequence", _ => _OpenExportPopup());
        menu.Add("Change Minimap", _ => vm.OpenMinimapSettings());
    }


    private void PointerReleasedHandler(object? sender, PointerReleasedEventArgs args)
    {

        var point = args.GetCurrentPoint(sender as Control);
        var x = point.Position.X;
        var y = point.Position.Y;

        var coordInPlot = new Coordinate2D(AvaPlot1.Plot.GetCoordinates(new Pixel(x, y)));

        var vm = (SequenceViewModel?)DataContext;
        if (vm is null) return;
        vm.ReleasedPointerPosition = coordInPlot;
        vm.UpdateInspectionViewModel();
    }

    private void _SetPlottable(SequencePlottable plottable)
    {
        if (_plottable is not null) AvaPlot1.Plot.Remove(_plottable);

        _plottable = plottable;
        AvaPlot1.Plot.Add.Plottable(_plottable!);
        _plottable!.SequenceImage.RequestRefreshPlotEvent += (_, _) => AvaPlot1.Refresh();
        AvaPlot1.Refresh();
    }

    private void PointerPressedHandler(object sender, PointerPressedEventArgs args)
    {

        var point = args.GetCurrentPoint(sender as Control);
        var x = point.Position.X;
        var y = point.Position.Y;

        var coordInPlot = new Coordinate2D(AvaPlot1.Plot.GetCoordinates(new Pixel(x, y)));

        var vm = (SequenceViewModel?)DataContext;
        if (vm is null) return;
        vm.PressedPointerPosition = coordInPlot;
    }

    private void PointerMovedHandler(object? sender, PointerEventArgs args)
    {

        var pointInPlot = AvaPlot1.Plot.GetCoordinates(
            (float)args.GetPosition(AvaPlot1).X * AvaPlot1.DisplayScale,
            (float)args.GetPosition(AvaPlot1).Y * AvaPlot1.DisplayScale);

        _horizontalLine.Position = pointInPlot.Y;
        _verticalLine.Position = pointInPlot.X;
        AvaPlot1.Refresh();
    }

    private void StyledElement_OnDataContextChanged(object? sender, EventArgs e)
    {
        var vm = DataContext as SequenceViewModel;
        if (vm == null)
        {
            return;
        }
        vm.MinimapHasChanged += Layoutable_OnLayoutUpdated;

        PreparePlot();

        var isDark = Application.Current!.ActualThemeVariant == ThemeVariant.Dark;
        var checkerBoard = new CheckerboardPlottable(isDark);
        AvaPlot1.Plot.Add.Plottable(checkerBoard);

        // sets the plottable
        _SetPlottable(new SequencePlottable(vm.Sequence, vm.CurrentRenderer));

        // Changed the sequence view -> full rerender
        vm.RenderersUpdated += (_, _) =>
        {
            if (_plottable is null) return;
            _plottable.SequenceImage.Reset();
            _plottable!.ChangeRenderer(vm.CurrentRenderer);
            AvaPlot1.Plot.MoveToTop(_anno);
            AvaPlot1.Refresh();
        };

        vm.CutSequence += (_, _) =>
        {
            _SetPlottable(new SequencePlottable(vm.Sequence, vm.CurrentRenderer));

            var oldLimits = AvaPlot1.Plot.Axes.GetLimits();
            var ySize = oldLimits.Bottom - oldLimits.Top;
            var newLimits = new AxisLimits(oldLimits.Left, oldLimits.Right, -ySize / 3, 2 * ySize / 3);
            AvaPlot1.Plot.Axes.SetLimits(newLimits);
            AvaPlot1.Plot.MoveToTop(_anno);
            AvaPlot1.Refresh();
        };

        vm.CloseEvent += (_, _) =>
        {
            ScrollingSynchronizerMapper.RemoveSequence(this);
            PlotControllerManager.RemovePlotFromAllControllers(AvaPlot1);
        };

        var currentTheme = Application.Current.ActualThemeVariant;

        Application.Current.TryGetResource("AnnotationColor", currentTheme, out var annotationColor);
        var color = (Avalonia.Media.Color)(annotationColor ?? new Avalonia.Media.Color(255, 39, 191, 179));
        _anno.LabelBackgroundColor = new Color(color.R, color.G, color.B).WithAlpha(0.7);
        _anno.LabelShadowColor = Colors.Transparent;
        AvaPlot1.Plot.MoveToTop(_anno);
    }

    /// <summary>
    /// Updates the Position Annotation to show the given position and the pixel values in the plot
    /// of this position in the sequence.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate</param>
    public void UpdatePositionAnnotation(double x, double y)
    {
        if (DataContext is not SequenceViewModel vm)
        {
            return;
        }

        // View is moved by 0.5, so adding 0.5 to get correct pixel values
        x = Math.Round(x);
        y = Math.Round(y);

        // If outside the sequence just show the position and no values.
        if (x >= vm.Sequence.Shape.Width || y >= vm.Sequence.Shape.Height || x < 0 || y < 0)
        {
            _anno.Text = string.Format(PixelLabel, x, y, 0, 0, 0);
            return;
        }

        var bytes = vm.Renderers[vm.RendererSelection].RenderPixel(vm.Sequence, (long)(x), (long)(y));
        _anno.Text = string.Format(PixelLabel, x, y, bytes.R, bytes.G, bytes.B);
    }

    private void _OpenTransformPopup()
    {
        AffineTransformationPopup popup = new((SequenceViewModel)DataContext!);
        var v = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        popup.ShowDialog(v!.MainWindow!);
    }

    private void _OpenColorsPopup()
    {
        ColorSettingsPopup popup = new((SequenceViewModel)DataContext!);
        var v = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        popup.ShowDialog(v!.MainWindow!);
    }

    private void _OpenCutPopup()
    {
        CutSequencePopup popup = new((SequenceViewModel)DataContext!);
        var v = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        popup.ShowDialog(v!.MainWindow!);
    }

    private void _OpenExportPopup()
    {
        ExportSequenceView popup = new((SequenceViewModel)DataContext!);
        var v = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        popup.ShowDialog(v!.MainWindow!);
    }

    private void _OpenInspectionViewModel()
    {
        if (DataContext is not SequenceViewModel sequenceViewModel) return;
        sequenceViewModel.OpenInspectionView();
    }

    /// <summary>
    /// This method updates the value of the Scrollbar and display the corresponding position in the sequence.
    /// </summary>
    /// <param name="val">The new value of the ScrollBar</param>
    public void UpdateScrolling(double val)
    {
        var vm = (DataContext as SequenceViewModel)!;
        var plot = AvaPlot1.Plot;
        var ySize = plot.Axes.GetLimits().Bottom - plot.Axes.GetLimits().Top;
        var top = (val / 100.0) * vm.Sequence.Shape.Height - 100.0;
        AvaPlot1.Plot.Axes.SetLimitsY(top, top + ySize);
        AvaPlot1.Refresh();
        Bar1.Value = val;
    }

    /// <summary>
    /// This method updates the displayed position in the sequence to that of another AvaPlot and
    /// sets the corresponding value for the ScrollBar.
    /// </summary>
    /// <param name="otherPlot">The AvaPlot, which limits will be used to set the display limits of this sequence.</param>
    public void UpdateScrolling(AvaPlot otherPlot)
    {
        AvaPlot1.Plot.Axes.SetLimits(otherPlot.Plot.Axes.GetLimits());
        AvaPlot1.Refresh();
        UpdateScrollBar();
    }

    /// <summary>
    /// This method updates the value of the Scrollbar, setting it to the value corresponding to the displayed position in the sequence.
    /// </summary>
    public void UpdateScrollBar()
    {
        var vm = (DataContext as SequenceViewModel)!;
        var val = ((AvaPlot1.Plot.Axes.GetLimits().Top + 100.0) / vm.Sequence.Shape.Height) * 100;
        Bar1.Value = val <= 0.0 ? 0.0 : double.Min(val, 100.0);
    }



    private void Layoutable_OnLayoutUpdated(object? sender, EventArgs e)
    {
        var vm = DataContext as SequenceViewModel;
        if (vm == null || vm.MinimapVms.Count == 0)
        {
            return;
        }

        if (vm.MinimapVms.Count > 0)
        {
            var mapViewModel = (vm.MinimapVms.First() as SizeAdjustableViewModelBase);
            if (mapViewModel is null)
            {
                return;
            }
            mapViewModel.NotifySizeChanged(this, new ViewModels.Utility.SizeChangedEventArgs(this.GetSize().X * MinimapWidthScale, this.GetSize().Y));
        }
    }
}