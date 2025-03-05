namespace BEAM.Models.Log;

/// <summary>
/// Log events inside BEAM.
/// </summary>
public enum LogEvent
{
    Sequence,
    FileNotFound,
    UnknownFileFormat,
    OpenedFile,
    ClosedFile,
    ExportedFile,
    BasicMessage,
    ThrownException,
    Critical,
    Analysis
}