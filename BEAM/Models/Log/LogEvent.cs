namespace BEAM.Models.Log;

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
    Critical
}