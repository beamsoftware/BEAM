using System;
using BEAM.Models.Log;

namespace BEAM.Exceptions;

public class BeamException : Exception
{
    protected BeamException()
    {
    }

    protected BeamException(string message) : base(message)
    {
    }
}