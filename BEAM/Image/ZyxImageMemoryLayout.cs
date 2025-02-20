using System.Runtime.CompilerServices;

namespace BEAM.Image;

/// <summary>
/// ZYX Cube layout in image data.
/// Data is stored in channel number order, then y pos order, lastly x pos order.
/// </summary>
public sealed class ZyxImageMemoryLayout : ImageMemoryLayout
{
    private readonly long Y;
    private readonly long Z;

    public ZyxImageMemoryLayout(ImageShape shape) : base(shape)
    {
        Y = 1L * Shape.Width;
        Z = 1L * Shape.Height * Shape.Width;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override long Flatten(long x, long y, int z) => x + y * Y + z * Z;
}