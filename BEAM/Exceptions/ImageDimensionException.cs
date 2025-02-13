using BEAM.Models.Log;

namespace BEAM.Exceptions;

public class ImageDimensionException : BeamException
{
    public ImageDimensionException()
    {
    }

    public ImageDimensionException(string message) : base(message)
    {
    }
}