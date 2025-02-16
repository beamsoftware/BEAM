using System;
using BEAM.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.Models.Log;

public partial class LogEntry(LogLevel level, string occuredEvent, string message) : ViewModelBase
{
    [ObservableProperty] public partial LogLevel Level { get; set; } = level;
    
    // not nullable since LogLevel is not
    public string LevelStr => Enum.GetName(Level)!.ToUpper();

    [ObservableProperty] public partial string Event { get; set; } = occuredEvent;
    [ObservableProperty] public partial string Message { get; set; } = message;
}