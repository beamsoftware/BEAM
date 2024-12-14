using BEAM.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BEAM.ViewModels;

public partial class BEAMMenuBarViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static

    [ObservableProperty] public partial string File { get; set; } = Configuration.StandardEnglish().FileMenu;
    [ObservableProperty] public partial string Open { get; set; } = Configuration.StandardEnglish().Open;
    [ObservableProperty] public partial string Exit { get; set; } = Configuration.StandardEnglish().Exit;
    [ObservableProperty] public partial string Edit { get; set; } = Configuration.StandardEnglish().Edit;
    [ObservableProperty] public partial string Paste { get; set; } = Configuration.StandardEnglish().Paste;
    [ObservableProperty] public partial string Copy { get; set; } = Configuration.StandardEnglish().Copy;
    
    [ObservableProperty] private string? _fileText;
    
    [RelayCommand]
    public async Task OpenSequence()
    {
        var file = await OpenFilePickerAsync();
        Console.WriteLine("File: " + file?.Path);
    }

    public void ExitBeam()
    {
        Environment.Exit(0);
    }
    
    private async Task<IStorageFile?> OpenFilePickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        return files?.Count >= 1 ? files[0] : null;
    }

}