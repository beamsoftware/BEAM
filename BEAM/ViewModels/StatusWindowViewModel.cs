using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BEAM.Models.Log;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogEntry = BEAM.Models.Log.LogEntry;

namespace BEAM.ViewModels;

public partial class StatusWindowViewModel : ViewModelBase
{
    private readonly Logger _logger;
    public ObservableCollection<LogEntry> StatusList { get; set; }

    public StatusWindowViewModel()
    {
        _logger = Logger.GetInstance();
        StatusList = _logger.GetLogEntries();
    }

    [RelayCommand]
    private void ClearStatus()
    {
        _logger.ClearStatusBar();
    }
}