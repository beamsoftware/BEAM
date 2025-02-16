using System.Net;
using BEAM.Models.Log;

namespace BEAM.Tests;

public class LoggerTests
{
    [Fact]
    public void CreateNewLogger()
    {
        // Arrange
        var logger = Logger.GetInstance();
        
        // Act
        logger.Error(LogEvent.FileNotFound, "Test");
        
        logger.Warning(LogEvent.UnknownFileFormat);
        
        logger.Info(LogEvent.ClosedFile, "A File was closed");
        
        logger.LogMessage("This is a test log message");

        // Assert
        Assert.True(File.Exists("../../../loggerTests/testLogs/testLog.txt"));
    }
    

}