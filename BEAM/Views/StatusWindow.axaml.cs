using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using BEAM.Log;
using BEAM.ViewModels;

namespace BEAM.Views;

public partial class StatusWindow : Window
{
    public StatusWindow()
    {
        InitializeComponent();
        DataContext = new StatusWindowViewModel();
        AddHandler(KeyDownEvent, OnKeyDown);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        Console.WriteLine("Hello");
        if (e.Key != Key.Escape) return;
        Console.WriteLine("Hello");
        Close();
    }
}