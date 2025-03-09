using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using BEAM.Controls;
using BEAM.Renderer;
using BEAM.Views.Utility;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.ViewModels;

/// <summary>
/// View model controlling the renderer settings popup
/// </summary>
public partial class ColorSettingsPopupViewModel : ViewModelBase
{
    /// <summary>
    /// The dynamically created controls based on the available renderers.
    /// </summary>
    public ObservableCollection<Control> RendererSelectionControls { get; } = [];
    private readonly SequenceViewModel _sequenceViewModel;

    private int _selection;

    private readonly List<SaveUserControl> _controls = [];

    /// <summary>
    /// The minimum value of the range of raw values the sequence is drawn at.
    /// e.g.: 8-Bit PNG uses 0
    /// </summary>
    [ObservableProperty]
    public partial decimal Min { get; set; }

    /// <summary>
    /// The maximum value of the range of raw values the sequence is drawn at.
    /// e.g.: 8-Bit PNG uses 255
    /// </summary>
    [ObservableProperty]
    public partial decimal Max { get; set; }

    public ColorSettingsPopupViewModel(SequenceViewModel sequenceViewModel)
    {
        _sequenceViewModel = sequenceViewModel;

        Min = (decimal)sequenceViewModel.CurrentRenderer.MinimumOfIntensityRange;
        Max = (decimal)sequenceViewModel.CurrentRenderer.MaximumOfIntensityRange;

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
        button.IsCheckedChanged += (s, _) =>
        {
            var btn = (RadioButton)s!;
            if (btn.IsChecked ?? false) _selection = index;
        };

        return button;
    }

    private StackPanel _BuildPanel(SequenceRenderer renderer)
    {
        var panel = new StackPanel() { Margin = new Thickness(30, 0, 0, 0) };
        
        SaveUserControl? configView = renderer.GetConfigView(_sequenceViewModel);
        if (configView is null)
        {
            return panel;
        }
        panel.Children.Add(configView);
        _controls.Add(configView);
        return panel;
    }

    /// <summary>
    /// Saves the current settings to the sequence renderers and redraws the sequence.
    /// </summary>
    /// <returns>Whether the settings could be saved successfully.</returns>
    public bool Save()
    {
        foreach (var control in _controls)
        {
            control.Save();
        }

        foreach (var renderer in _sequenceViewModel.Renderers)
        {
            renderer.MinimumOfIntensityRange = (double)Min;
            renderer.MaximumOfIntensityRange = (double)Max;
        }

        _sequenceViewModel.RendererSelection = _selection;
        _sequenceViewModel.RenderersUpdated.Invoke(this, new RenderersUpdatedEventArgs());
        return true;
    }
}