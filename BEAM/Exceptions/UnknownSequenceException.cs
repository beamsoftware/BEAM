using BEAM.Models.Log;

namespace BEAM.Exceptions;

public class UnknownSequenceException : BeamException
{
    public UnknownSequenceException()
    {
    }

    public UnknownSequenceException(string message) : base(message)
    {
    }
}