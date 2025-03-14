using System.Runtime.CompilerServices;

namespace BEAM.Image;

/// <summary>
/// YZX Cube layout in image data.
/// Data is stored in y pos order, then channel number order, lastly x pos order.
/// </summary>
public sealed class YzxImageMemoryLayout : ImageMemoryLayout
{
    private readonly long _y;
    private readonly long _z;

    public YzxImageMemoryLayout(ImageShape shape) : base(shape)
    {
        _y = 1L * Shape.Channels * Shape.Width;
        _z = 1L * Shape.Width;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override long Flatten(long x, long y, int z) => x + y * _y + z * _z;
}