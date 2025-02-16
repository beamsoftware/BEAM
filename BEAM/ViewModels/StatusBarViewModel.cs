using System;
using System.Collections.Specialized;
using System.IO.Pipelines;
using BEAM.Models.Log;
using BEAM.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogEntry = BEAM.Models.Log.LogEntry;

namespace BEAM.ViewModels;

public partial class StatusBarViewModel : ViewModelBase
{
    private static StatusBarViewModel? _instance;
    
    private long _infoCounter = 0;
    private long _warningCounter = 0;
    private long _errorCounter = 0;

    [ObservableProperty] public partial bool InfoVisible { get; set; } = false;
    [ObservableProperty] public partial bool WarningVisible { get; set; } = false;
    [ObservableProperty] public partial bool ErrorVisible { get; set; } = false;
    [ObservableProperty] public partial bool StatusBarVisible { get; set; } = false;

    [ObservableProperty] public partial string InfoText { get; set; } = "";
    [ObservableProperty] public partial string WarningText { get; set; } = "";
    [ObservableProperty] public partial string ErrorText { get; set; } = "";

    public StatusBarViewModel()
    {
        var logger = Logger.GetInstance();
        logger.GetLogEntries().CollectionChanged += OnLogEntriesChanged;
    }

    private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            Clear();
            return;
        }

        if (e is not { Action: NotifyCollectionChangedAction.Add, NewItems.Count: > 0 }) return;

        var item = (LogEntry) e.NewItems[0]!;
        switch (item.Level)
        {
            case LogLevel.Info:
                AddInfo(item.Message);
                break;
            case LogLevel.Warning:
                AddWarning(item.Message);
                break;
            case LogLevel.Error:
                AddError(item.Message);
                break;
            case LogLevel.Debug:
            default:
                break;
        }
    }

    public static StatusBarViewModel GetInstance()
    {
        return _instance ?? (_instance = new StatusBarViewModel());
    }
    
    public void AddInfo(string infoMessage)
    {
        _infoCounter += 1;
        InfoVisible = true;
        StatusBarVisible = true;
        InfoText = _infoCounter >= 100 ? "99+ Info" : _infoCounter + " Info";
    }
    
    public void AddWarning(string warningMessage)
    {
        _warningCounter += 1;
        WarningVisible = true;
        StatusBarVisible = true;
        WarningText = _warningCounter >= 100 ? "99+ Warnings" : _warningCounter + " Warnings";
    }
    
    public void AddError(string errorMessage)
    {
        _errorCounter += 1;
        ErrorText = _errorCounter >= 100 ? "99+ Errors" : _errorCounter + " Errors";
        ErrorVisible = true;
        StatusBarVisible = true;
    }
    
    public void Clear()
    {
        _infoCounter = 0;
        _warningCounter = 0;
        _errorCounter = 0;
        InfoVisible = false;
        WarningVisible = false;
        ErrorVisible = false;
        StatusBarVisible = false;
        InfoText = "";
        WarningText = "";
        ErrorText = "";
    }
    
    [RelayCommand]
    public void OpenStatusWindow()
    {
        var statusWindow = new StatusWindow();
        statusWindow.Show();
    }
}