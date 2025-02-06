using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;

namespace BEAM.Image.Envi;

/// <summary>
/// Decodes ENVI image data for the specified filename and value type T.
/// </summary>
public class EnviImage<T> : ITypedImage<T>, IMemoryImage
{
    private MemoryMappedFile? FileMapping { get; set; }
    private MemoryMappedViewAccessor? FileAccessor { get; set; }
    private Func<long, T> GetValue { get; init; }

    /// <summary>
    /// This image's dimensions, meaning its length, width and channel count.
    /// </summary>
    public ImageShape Shape { get; init; }

    /// <summary>
    /// The orientation pixels are being stored in memory with. 
    /// </summary>
    public ImageMemoryLayout Layout { get; init; }

    /// <summary>
    /// Creates a new instance given the filepath to an envis header and raw file, whose names must match except for their file endings.
    /// The default file endings of .hdr for header files and .raw for data files must be used.
    /// </summary>
    /// <param name="filepath">The path to both envi files including their name. Therefor their names must match.</param>
    public EnviImage(string filepath) : this(filepath, (hdr: ".hdr", raw: ".raw"))
    {
    }

    /// <summary>
    /// Creates a new instance given the filepath to an envis header and raw file, whose names must match except for their file endings.
    /// The extensions for the different types (header and data) can be customized.
    /// </summary>
    /// <param name="filepath">The path to both envi files including their name. Therefor their names must match.</param>
    /// <param name="extensions">A tuple whose first entry is the file extension for the header file and its second is the data files extension.</param>
    /// <exception cref="FileNotFoundException">If either file was found at the specified filepath.</exception>
    /// <exception cref="NotSupportedException">If no envi file, or an ENVI file with a byte order other than the host's was found.</exception>
    public EnviImage(string filepath, (string hdr, string raw) extensions)
    {
        var filepaths = (
            hdr: Path.ChangeExtension(filepath, extensions.hdr),
            raw: Path.ChangeExtension(filepath, extensions.raw));

        if (!File.Exists(filepaths.hdr))
        {
            throw new FileNotFoundException(
                $"Missing ENVI header file \"{filepaths.hdr}\"!");
        }

        if (!File.Exists(filepaths.raw))
        {
            throw new FileNotFoundException(
                $"Missing ENVI raw file \"{filepaths.raw}\"!");
        }

        var header = new EnviHeader(filepaths.hdr);

        var filetype = header.Get<string>("file type", "ENVI");
        var offset = header.Get<int>("header offset", 0);

        var byteorder = header.Get<EnviByteOrder>("byte order", EnviExtensions.GetNativeByteOrder());
        var datatype = header.Get<EnviDataType>("data type");
        var interleave = header.Get<EnviInterleave>("interleave");

        var width = header.Get<int>("samples");
        var height = header.Get<int>("lines");
        var channels = header.Get<int>("bands");

        if (!filetype.Contains("ENVI", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                $"Invalid ENVI file type \"{filetype}\"!");
        }

        if (!byteorder.IsNative())
        {
            throw new NotSupportedException(
                $"Incompatible ENVI byte order \"{byteorder}\"!");
        }

        if (datatype.IsComplex())
        {
            throw new NotSupportedException(
                $"Unsupported ENVI data type \"{datatype}\"!");
        }

        var size = 1L * width * height * channels * datatype.SizeOf();

        FileMapping = MemoryMappedFile.CreateFromFile(
            filepaths.raw,
            FileMode.OpenOrCreate,
            null,
            size + offset,
            MemoryMappedFileAccess.Read);

        FileAccessor = FileMapping.CreateViewAccessor(
            offset,
            size,
            MemoryMappedFileAccess.Read);

        GetValue = FileAccessor.CreateValueGetter<T>(datatype);

        Shape = new ImageShape(
            width,
            height,
            channels);

        Layout = interleave switch
        {
            EnviInterleave.BIP => new YxzImageMemoryLayout(Shape),
            EnviInterleave.BIL => new YzxImageMemoryLayout(Shape),
            EnviInterleave.BSQ => new ZyxImageMemoryLayout(Shape),
            _ => throw new NotSupportedException(
                $"Unsupported ENVI interleave \"{interleave}\"!")
        };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (FileAccessor != null)
        {
            FileAccessor.Dispose();
            FileAccessor = null;
        }
    }

    public double GetPixel(long x, long y, int channel)
    {
        var val = this[x, y, channel];
        return (double)Convert.ChangeType(val, typeof(double))!;
    }

    public double[] GetPixel(long x, long y)
    {
        var values = new double[Shape.Channels];
        for (var i = 0; i < Shape.Channels; i++)
        {
            values[i] = GetPixel(x, y, i);
        }

        return values;
    }

    public double[] GetPixel(long x, long y, int[] channels)
    {
        var values = new double[channels.Length];
        for (var i = 0; i < channels.Length; i++)
        {
            values[i] = GetPixel(x, y, channels[i]);
        }

        return values;
    }

    public LineImage GetPixelLineData(long[] xs, long line, int[] channels)
    {
        var data = new double[xs.Length][];
        for (var i = 0; i < xs.Length; i++)
        {
            data[i] = new double[channels.Length];
        }

        //TODO: Implement in MemoryLayoutClasses for better inheritance
        switch (Layout)
        {
            // checking for memory layout -> better cache accesses
            // iterate over channels last
            case XyzImageMemoryLayout:
            case YxzImageMemoryLayout:
            {
                for (var x = 0; x < xs.Length; x++)
                {
                    for (var channelIdx = 0; channelIdx < channels.Length; channelIdx++)
                    {
                        data[x][channelIdx] = GetPixel(xs[x], line, channels[channelIdx]);
                    }
                }

                break;
            }
            // iterate over x position last
            case YzxImageMemoryLayout:
            case ZyxImageMemoryLayout:
            {
                for (var channelIdx = 0; channelIdx < channels.Length; channelIdx++)
                {
                    for (var x = 0; x < xs.Length; x++)
                    {
                        data[x][channelIdx] = GetPixel(xs[x], line, channels[channelIdx]);
                    }
                }

                break;
            }
            default:
                throw new NotImplementedException($"Efficient pixel data line getter not implemented for  layout type {Layout.GetType()} when using ENVI images");
        }

        return new LineImage(data);
    }

    public LineImage GetPixelLineData(long line, int[] channels)
    {
        var data = new double[Shape.Width][];
        for (var i = 0; i < Shape.Width; i++)
        {
            data[i] = new double[channels.Length];
        }

        switch (Layout)
        {
            // checking for memory layout -> better cache accesses
            // iterate over channels last
            case XyzImageMemoryLayout:
            case YxzImageMemoryLayout:
            {
                for (var x = 0; x < Shape.Width; x++)
                {
                    for (var channelIdx = 0; channelIdx < channels.Length; channelIdx++)
                    {
                        data[x][channelIdx] = GetPixel(x, line, channels[channelIdx]);
                    }
                }

                break;
            }
            // iterate over x position last
            case YzxImageMemoryLayout:
            case ZyxImageMemoryLayout:
            {
                for (var channelIdx = 0; channelIdx < channels.Length; channelIdx++)
                {
                    for (var x = 0; x < Shape.Width; x++)
                    {
                        data[x][channelIdx] = GetPixel(x, line, channels[channelIdx]);
                    }
                }

                break;
            }
            default:
                throw new NotImplementedException($"Efficient pixel data line getter not implemented for  layout type {Layout.GetType()} when using ENVI images");
        }

        return new LineImage(data);
    }

    public T this[long x, long y, int channel] => GetValue(Layout.Flatten(x, y, channel));
}

/// <summary>
/// This class contains the utility to open new ENVI files. 
/// </summary>
public static class EnviImage
{
    /// <summary>
    /// This method opens a new file as an envi file given the supplied path. Its corresponding header file must be located at the same location with the same name.
    /// </summary>
    /// <param name="filepath">The path to the ENVI file (header and data).</param>
    /// <returns> A generic <see cref="EnviImage{T}"/> cast to its superclass <see cref="IEnviImage"/> which represents the files found under the supplied path.</returns>
    /// <exception cref="FileNotFoundException">If either the header or the raw file were not found in under the supplied path.</exception>
    public static IMemoryImage OpenImage(string filepath)
    {
        var filepaths = (
            hdr: Path.ChangeExtension(filepath, ".hdr"),
            raw: Path.ChangeExtension(filepath, ".raw"));

        if (!File.Exists(filepaths.hdr))
        {
            throw new FileNotFoundException(
                $"Missing ENVI header file \"{filepaths.hdr}\"!");
        }

        if (!File.Exists(filepaths.raw))
        {
            throw new FileNotFoundException(
                $"Missing ENVI raw file \"{filepaths.raw}\"!");
        }

        var header = new EnviHeader(filepaths.hdr);
        var datatype = header.Get<EnviDataType>("data type");

        var type = datatype.TypeOf();
        ConstructorInfo ctor = typeof(EnviImage<>).MakeGenericType([type]).GetConstructor([typeof(string)])!;
        return (IMemoryImage)ctor.Invoke([filepath]);
    }
}