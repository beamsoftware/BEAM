using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BEAM.Models.Log;

public partial class Logger : ObservableObject, ILog
{
    private static Logger? _instance;
    
    private string _pathToLogFile;
    private LogLevel _logLevel;
    private LogEvent _logEvent;

    private ObservableCollection<Models.Log.LogEntry> _LogEntries;

    private Logger(string pathToLogFile)
    {
        _LogEntries = [];
        _pathToLogFile = pathToLogFile;
        if (!File.Exists(pathToLogFile))
        {
            CreateNewLogFile(pathToLogFile);
        }
    }

    public static Logger Init(string? pathToLogFile=null)
    {
        pathToLogFile ??= "log.txt";

        _instance = new Logger(pathToLogFile);
        return _instance;
    }

    public static Logger GetInstance()
    {
        if (_instance is null) throw new Exception("Logger instance is null");
        return _instance!;
    }
    
    public void Error(LogEvent occuredEvent)
    {
        _logLevel = LogLevel.Error;
        _logEvent = occuredEvent;
        Write("ERROR! --> " + occuredEvent);
    }

    public void Warning(LogEvent occuredEvent)
    {
        _logLevel = LogLevel.Warning;
        _logEvent = occuredEvent;
        Write("Warning --> " + occuredEvent);
    }

    public void Debug(LogEvent occuredEvent)
    {
        _logLevel = LogLevel.Debug;
        _logEvent = occuredEvent;
        Write("Debug -->  " + occuredEvent);
    }

    public void Info(LogEvent occuredEvent)
    {
        _logLevel = LogLevel.Info;
        _logEvent = occuredEvent;
        Write("Info: " + occuredEvent);
    }

    public void Error(LogEvent occuredEvent, string logMessage)
    {
        _logLevel = LogLevel.Error;
        _logEvent = occuredEvent;
        Write("ERROR! --> " + occuredEvent + ": " + logMessage);
    }

    public void Warning(LogEvent occuredEvent, string logMessage)
    {
        _logLevel = LogLevel.Warning;
        _logEvent = occuredEvent;
        Write("Warning --> " + occuredEvent + ": " + logMessage);
    }

    public void Debug(LogEvent occuredEvent, string logMessage)
    {
        _logLevel = LogLevel.Debug;
        _logEvent = occuredEvent;
        Write("Debug --> " + occuredEvent + ": " + logMessage);
    }

    public void Info(LogEvent occuredEvent, string logMessage)
    {
        _logLevel = LogLevel.Info;
        _logEvent = occuredEvent;
        Write("Info: " + occuredEvent + ": " + logMessage);
    }

    public void LogMessage(string logMessage)
    {
        _logLevel = LogLevel.Info;
        _logEvent = LogEvent.BasicMessage;
        Write("Unspecified Log: " + logMessage);
    }
    
    private void CreateNewLogFile(string pathToLogFile)
    {
        using var fs = new FileStream(pathToLogFile, FileMode.CreateNew);
        using var w = new BinaryWriter(fs);

        w.Write("New log file created at: " + DateTime.Now + "\n");
    }
    
    private void Write(string message)
    {
        using (var outputFile = new StreamWriter(_pathToLogFile, true))
        {
            outputFile.WriteLine(DateTime.Now + " " +message);
        }
        _LogEntries.Add(new LogEntry(_logLevel, Enum.GetName(_logEvent)!, message));
    }
    
    public void ClearStatusBar()
    {
        _LogEntries.Clear();
    }
    
    public ObservableCollection<Models.Log.LogEntry> GetLogEntries()
    {
        return _LogEntries;
    }
}