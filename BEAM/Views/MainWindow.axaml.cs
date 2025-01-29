using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BEAM.ImageSequence;
using BEAM.Models;
using BEAM.ViewModels;

namespace BEAM.Views;

public partial class MainWindow : Window
{
    private DockingViewModel _dockingViewModel = new();

    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += (sender, args) =>
        {
            if (!IsInitialized) return;
            var viewmodel = (MainWindowViewModel)DataContext;
            DockView.DataContext = viewmodel.DockingVm;
        };

        AddHandler(DragDrop.DropEvent, OnDrop);
    }
    
    private static void OnDrop(object? sender, DragEventArgs e)
    {
        Console.WriteLine("Dropped");
        var data = e.Data.GetFiles();
        var vm = (MainWindowViewModel)((MainWindow)sender!).DataContext;
        
        vm.OpenSequenceFromDropCommand.Execute(data);
    }
}