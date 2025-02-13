using BEAM.Models.Log;

namespace BEAM.Exceptions;

/// <summary>
/// Error is thrown, if the user gives an invalid argument, e.g. references channel 6 for an RGB image.
/// </summary>
public class InvalidUserArgumentException : BeamException
{
    public InvalidUserArgumentException()
    {
    }

    public InvalidUserArgumentException(string message) : base(message)
    {
    }
}