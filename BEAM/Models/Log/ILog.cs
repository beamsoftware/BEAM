namespace BEAM.Models.Log;

/// <summary>
/// Interface for logging events.
/// </summary>
public interface ILog
{
    public void Error(LogEvent occuredEvent);
    public void Warning(LogEvent occuredEvent);
    public void Debug(LogEvent occuredEvent);
    public void Info(LogEvent occuredEvent);
    
    public void Error(LogEvent occuredEvent, string logMessage);
    public void Warning(LogEvent occuredEvent,string logMessage);
    public void Debug(LogEvent occuredEvent,string logMessage);
    public void Info(LogEvent occuredEvent,string logMessage);
    
    public void LogMessage(string logMessage);
    
}