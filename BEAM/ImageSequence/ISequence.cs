using BEAM.Image;

namespace BEAM.ImageSequence;

public interface ISequence : IImage
{
    public string GetName();
}