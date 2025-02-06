// (c) Paul Stier, 2025

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using BEAM.Controls;
using BEAM.Renderer;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.ViewModels;

public partial class ColorSettingsPopupViewModel : ViewModelBase
{
    public ObservableCollection<Control> RendererSelectionControls { get; } = [];
    private SequenceViewModel _sequenceViewModel;

    private int _selection;

    private List<ISaveControl> _controls = [];

    [ObservableProperty] private decimal _min;
    [ObservableProperty] private decimal _max;

    public ColorSettingsPopupViewModel(SequenceViewModel sequenceViewModel)
    {
        _sequenceViewModel = sequenceViewModel;

        Min = (decimal) sequenceViewModel.CurrentRenderer.MinimumOfIntensityRange;
        Max = (decimal) sequenceViewModel.CurrentRenderer.MaximumOfIntensityRange;

        _selection = _sequenceViewModel.RendererSelection;

        for (var index = 0; index < sequenceViewModel.Renderers.Length; index++)
        {
            var renderer = sequenceViewModel.Renderers[index];
            RendererSelectionControls.Add(_BuildSelectionButton(renderer, index));
            RendererSelectionControls.Add(_BuildPanel(renderer));
        }
    }

    private RadioButton _BuildSelectionButton(SequenceRenderer renderer, int index)
    {
        var button = new RadioButton()
        {
            GroupName = "RendererSelectionGroup",
            Content = renderer.GetName(),
            IsChecked = (_selection == index),
        };
        button.IsCheckedChanged += (s, e) =>
        {
            var btn = (RadioButton)s!;
            if (btn.IsChecked ?? false) _selection = index;
        };

        return button;
    }

    private StackPanel _BuildPanel(SequenceRenderer renderer)
    {
        var panel = new StackPanel() { Margin = new Thickness(30, 0, 0, 0) };

        switch (renderer)
        {
            case HeatMapRenderer htmRenderer:
                var hmView = new HeatMapConfigControlView(htmRenderer, _sequenceViewModel);
                panel.Children.Add(hmView);
                _controls.Add(hmView);
                break;
            case ChannelMapRenderer chmRenderer:
                var chmView = new ChannelMapConfigControlView(chmRenderer, _sequenceViewModel);
                panel.Children.Add(chmView);
                _controls.Add(chmView);
                break;
            case ArgMaxRenderer:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(renderer));
        }

        return panel;
    }

    public bool Save()
    {
        foreach (var control in _controls)
        {
            control.Save();
        }

        foreach (var renderer in _sequenceViewModel.Renderers)
        {
            renderer.MinimumOfIntensityRange = (double) Min;
            renderer.MaximumOfIntensityRange = (double) Max;
        }

        _sequenceViewModel.RendererSelection = _selection;
        _sequenceViewModel.RenderersUpdated.Invoke(this, new RenderersUpdatedEventArgs());
        return true;
    }
}