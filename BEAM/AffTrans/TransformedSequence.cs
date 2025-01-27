using System;
using BEAM.ImageSequence;

namespace BEAM.AffTrans;

public class TransformedSequence(Sequence originalSequence, Transformer transformer) 
{
    public double [] GetPixel(long x, long y)
    {
        (long transformedX, long transformedY) transformedCoordinate = transformer.Transform(x, y);
        double [] scaledPixel = originalSequence.GetPixel(transformedCoordinate.transformedX, transformedCoordinate.transformedY);
        return scaledPixel;
    }
    
    public double GetPixel(long x, long y, byte channel)
    {
        (long transformedX, long transformedY) transformedCoordinate = transformer.Transform(x, y);
        double scaledPixel = originalSequence.GetPixel(transformedCoordinate.transformedX, transformedCoordinate.transformedY, channel);
        return scaledPixel;
    }
    public double[,] GetPixelLine(long y)
    {
        throw new NotImplementedException();
        long transformedY = transformer.TransformY(y);
        // TODO: FIX
        //double [,] scaledPixel = originalSequence.GetPixelLine(transformedY);
        //return scaledPixel;
    }
    
}