using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace BEAM.Image.Bitmap;

public sealed partial class BgraBitmap : SKBitmap, IBitmap<BGRA>
{
  
  public ref BGRA this[int x, int y]
  {
    get
    {
      var bytes = GetPixelSpan().Slice(
        y * RowBytes +
        x * BytesPerPixel,
        BytesPerPixel);
      
      return ref MemoryMarshal.AsRef<BGRA>(bytes);
    }
  }

  public ref BGRA this[long i]
  {
    get
    {
      var x = (int)(i % Width);
      var y = (int)(i / Width);

      var bytes = GetPixelSpan().Slice(
        y * RowBytes +
        x * BytesPerPixel,
        BytesPerPixel);

      return ref MemoryMarshal.AsRef<BGRA>(bytes);
    }
  }

  public BgraBitmap(int width, int height) :
    base(width, height, SKColorType.Bgra8888, SKAlphaType.Premul)
  {

    Debug.Assert(Marshal.SizeOf<BGRA>() == BytesPerPixel);
    
  }


  /// <summary>
  /// Returns a writable Span of the data's (byte array)  as bytes. This is a workaround in comparison
  /// to the less permissive SKBitmap superclass.
  /// </summary>
  /// <returns>A byte span to the memory region of the pixel data.</returns>
  public new unsafe Span<byte> GetPixelSpan()
  { 
    IntPtr length;
    return new Span<byte>((void*) base.GetPixels(out length), (int)length);
  }

  public void Read(string path)
  {
    throw new NotImplementedException();
  }

  public void Write(string path)
  {
    throw new NotImplementedException();
  }
}
