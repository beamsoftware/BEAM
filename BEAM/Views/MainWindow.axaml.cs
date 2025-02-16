using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BEAM.Exceptions;
using BEAM.ImageSequence;
using BEAM.Models;
using BEAM.Models.Log;
using BEAM.ViewModels;

namespace BEAM.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += (sender, args) =>
        {
            if (!IsInitialized) return;
            var viewmodel = (MainWindowViewModel)DataContext!;
            DockView.DataContext = viewmodel.DockingVm;
        };

        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext!;

        var data = e.Data.GetFiles();
        if (data is null) return;

        var list = data.Select(f => f.Path).ToList();

        try
        {
            vm.DockingVm.OpenSequenceView(DiskSequence.Open(list));
        }
        catch (UnknownSequenceException)
        {
            Logger.GetInstance().Error(LogEvent.OpenedFile,
                $"Cannot open dragged-in files since no suitable sequence type found. (Supported sequences: {string.Join(", ", DiskSequence.SupportedSequences)})");
        }
        catch (EmptySequenceException)
        {
            Logger.GetInstance().Info(LogEvent.OpenedFile, "The sequence to be opened is empty");
        }
        catch (InvalidSequenceException)
        {
            // not handled, since the sequence will write a log message
        }
    }
}