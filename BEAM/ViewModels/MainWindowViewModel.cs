using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using BEAM.Exceptions;
using BEAM.ImageSequence;
using BEAM.ImageSequence.Synchronization;
using BEAM.ImageSequence.Synchronization.Manipulators;
using BEAM.Models;
using BEAM.Models.Log;
using BEAM.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BEAM.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public static readonly Configuration BaseConfig = Configuration.StandardEnglish();

    [ObservableProperty] public partial string File { get; set; } = BaseConfig.FileMenu;
    [ObservableProperty] public partial string Open { get; set; } = BaseConfig.Open;
    [ObservableProperty] public partial string OpenFolder { get; set; } = BaseConfig.OpenFolder;
    [ObservableProperty] public partial string Exit { get; set; } = BaseConfig.Exit;
    [ObservableProperty] public partial string Edit { get; set; } = BaseConfig.Edit;
    [ObservableProperty] public partial string Paste { get; set; } = BaseConfig.Paste;
    [ObservableProperty] public partial string Copy { get; set; } = BaseConfig.Copy;
    [ObservableProperty] public partial string Help { get; set; } = BaseConfig.Help;
    [ObservableProperty] public partial string View { get; set; } = BaseConfig.View;
    [ObservableProperty] public partial string Synchro { get; set; } = BaseConfig.Synchro;
    [ObservableProperty] public partial string ActivateSynchro { get; set; } = BaseConfig.ActivateSynchro;
    [ObservableProperty] public partial string DeactivateSynchro { get; set; } = BaseConfig.DeactivateSynchro;
    [ObservableProperty] public partial string ViewOpenStatusWindow { get; set; } = BaseConfig.ViewOpenStatusWindow;

    [ObservableProperty] private string? _fileText;
    [ObservableProperty] public partial DockingViewModel DockingVm { get; set; } = new();


    public static int TitleBarHeight => 38;

    private SyncedPlotController? _syncedPlotController;

    public MainWindowViewModel()
    {
        _syncedPlotController = new SyncedPlotController();
        _syncedPlotController.Register(new MouseManipulator());
        PlotControllerManager.RegisterController(_syncedPlotController);
    }

    [RelayCommand]
    private async Task OpenSequence()
    {
        var files = await OpenFilePickerAsync();
        if (files == null || files.Count == 0) return;

        var list = files.Select(f => f.Path).ToList();

        try
        {
            DockingVm.OpenSequenceView(DiskSequence.Open(list));
        }
        catch (UnknownSequenceException)
        {
            Logger.GetInstance().Error(LogEvent.OpenedFile,
                $"Cannot open files since no suitable sequence type found. (Supported sequences: {string.Join(", ", DiskSequence.SupportedSequences)})");
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

    [RelayCommand]
    private async Task OpenSequenceFromFolder()
    {
        var folder = await OpenFolderPickerAsync();

        if (folder == null) return;

        try
        {
            DockingVm.OpenSequenceView(DiskSequence.Open(folder.Path));
        }
        catch (UnknownSequenceException)
        {
            Logger.GetInstance().Error(LogEvent.OpenedFile,
                $"Cannot open folder since no suitable sequence type found. (Supported sequences: {string.Join(", ", DiskSequence.SupportedSequences)})");
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

    [RelayCommand]
    private void ExitBeam()
    {
        Environment.Exit(0);
    }

    private static async Task<IStorageFolder?> OpenFolderPickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var folder = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Open Sequence Folder",
            AllowMultiple = false,
        });

        return folder.Count >= 1 ? folder[0] : null;
    }

    private static async Task<IReadOnlyList<IStorageFile>?> OpenFilePickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open Sequence File(s)",
            AllowMultiple = true,
        });

        return files;
    }

    [RelayCommand]
    private void OpenStatusWindow()
    {
        var statusWindow = new StatusWindow();
        statusWindow.Show();
    }

    [RelayCommand]
    private void OpenAboutWindow()
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.Show();
    }

    [RelayCommand]
    private void ActivateSynchronization()
    {
        _syncedPlotController?.Activate();
        ScrollingSynchronizer.activateSynchronization();
    }

    [RelayCommand]
    private void DeactivateSynchronization()
    {
        _syncedPlotController?.Deactivate();
        ScrollingSynchronizer.deactivateSynchronization();
    }
}