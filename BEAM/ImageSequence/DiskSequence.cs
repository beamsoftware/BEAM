using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BEAM.Exceptions;
using BEAM.Image;

namespace BEAM.ImageSequence;

/// <summary>
/// Loads and manages an entire sequence.
/// </summary>
/// <param name="imagePaths">The paths of the images to use inside the sequence.</param>
public abstract class DiskSequence(List<string> imagePaths, string name) : ISequence
{
    /// Do not use -> set internally on first get
    private long? _singleFileHeight;

    public long SingleImageHeight
    {
        get
        {
            if (_singleFileHeight is not null)
            {
                return _singleFileHeight.Value;
            }

            _singleFileHeight = GetImage(0).Shape.Height;
            return _singleFileHeight.Value;
        }
    }

    private ImageShape? _shape;

    /// <summary>
    /// The Shape (width, height, channel count) of the entire sequence.
    /// </summary>
    public ImageShape Shape
    {
        get
        {
            if (_shape is not null)
            {
                return _shape.Value;
            }

            _InitializeShape();
            return _shape.Value;
        }
    }

    public double GetPixel(long x, long y, int channel)
    {
        var imageIdx = y / SingleImageHeight;
        var imageLine = y % SingleImageHeight;

        var img = GetImage((int)imageIdx);
        return img.GetPixel(x, imageLine, channel);
    }

    public double[] GetPixel(long x, long y)
    {
        var imageIdx = y / SingleImageHeight;
        var imageLine = y % SingleImageHeight;

        var img = GetImage((int)imageIdx);
        return img.GetPixel(x, imageLine);
    }

    public double[] GetPixel(long x, long y, int[] channels)
    {
        var imageIdx = y / SingleImageHeight;
        var imageLine = y % SingleImageHeight;

        var img = GetImage((int)imageIdx);
        return img.GetPixel(x, imageLine, channels);
    }

    public LineImage GetPixelLineData(long line, int[] channels)
    {
        var imageIdx = line / SingleImageHeight;
        var imageLine = line % SingleImageHeight;

        var img = GetImage((int)imageIdx);
        return img.GetPixelLineData(imageLine, channels);
    }

    public LineImage GetPixelLineData(long[] xs, long line, int[] channels)
    {
        var imageIdx = line / SingleImageHeight;
        var imageLine = line % SingleImageHeight;

        var img = GetImage((int)imageIdx);
        return img.GetPixelLineData(xs, imageLine, channels);
    }

    public string GetName()
    {
        return name;
    }

    private Mutex _loadedImagesMutex = new();
    private IImage?[] _loadedImages = new IImage?[imagePaths.Count];

    /// <summary>
    /// Loads the desired image so that it's data can be accessed randomly.
    /// </summary>
    /// <param name="index">The index of the desired image.</param>
    /// <returns>The loaded image.</returns>
    protected abstract IImage LoadImage(int index);

    /// <summary>
    /// Initializes and validates the sequence.
    /// </summary>
    /// <returns>True if the sequence has been initialised, false if an error occured (then see log)</returns>
    protected abstract bool InitializeSequence();

    /// <summary>
    /// Returns the desired image. Loads the image into main memory on-demand if necessary.
    /// </summary>
    /// <param name="index">The index of the image to load.</param>
    /// <returns>The image with the correct index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the image index is out of range</exception>
    public IImage GetImage(int index)
    {
        if (index < 0 || index >= _loadedImages.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range [0, amount of images)");
        }

        var img = _loadedImages[index];
        if (img is not null) return img;

        _loadedImagesMutex.WaitOne();
        img = _loadedImages[index];
        if (img is not null)
        {
            _loadedImagesMutex.ReleaseMutex();
            return img;
        }

        img = LoadImage(index);
        _loadedImages[index] = img;
        _loadedImagesMutex.ReleaseMutex();
        return img;
    }

    /// <summary>
    /// Calculates the shape of the entire sequence.
    /// Only run once: when Shape is accessed the first tie
    /// </summary>
    private void _InitializeShape()
    {
        int length = imagePaths.Count;

        var firstImage = GetImage(0);
        long width = firstImage.Shape.Width;
        long height = firstImage.Shape.Height;
        _singleFileHeight = height;
        int channels = firstImage.Shape.Channels;

        if (length == 1)
        {
            _shape = new ImageShape(width, height, channels);
            return;
        }

        height *= (length - 1);

        var lastImage = GetImage(length - 1);
        height += lastImage.Shape.Height;

        _shape = new ImageShape(width, height, firstImage.Shape.Channels);
    }

    /// <summary>
    /// The supported file types and their respective Sequence type
    /// </summary>
    private static readonly Dictionary<string, Type> SequenceTypes = new()
    {
        [".png"] = typeof(SkiaSequence),
        [".raw"] = typeof(EnviSequence),
        [".hdr"] = typeof(EnviSequence),
    };


    /// <summary>
    /// Opens a new sequence.
    /// </summary>
    /// <param name="paths">The image paths the sequence uses.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <returns>The sequence</returns>
    /// <exception cref="EmptySequenceException">Thrown when no images are being passed or all found file extensions are unsupported</exception>
    /// <exception cref="UnknownSequenceException">Thrown when no suitable sequence can be found in the paths</exception>
    public static DiskSequence Open(List<string> paths, string name)
    {
        if (paths.Count == 0) throw new EmptySequenceException("Empty sequences are not supported");

        var extensions = paths.Select(Path.GetExtension).ToHashSet();
        foreach (var extension in extensions.OfType<string>())
        {
            if (!SequenceTypes.TryGetValue(extension, out var type)) continue;

            var sequence = _InstantiateFromType(type, paths, name);
            if (!sequence.InitializeSequence())
            {
                throw new InvalidSequenceException("Sequence could not be loaded due to error (see log)!");
            }

            return sequence;
        }

        throw new UnknownSequenceException($"Cannot find sequence in extensions: {string.Join(", ", extensions)}");
    }

    /// <summary>
    /// Opens a sequence from a folder.
    /// </summary>
    /// <param name="folder">The path to the folder with the sequence inside.</param>
    /// <returns>The opened sequence</returns>
    /// <exception cref="EmptySequenceException">Thrown when no images are being passed or all found file extensions are unsupported.</exception>
    /// <exception cref="UnknownSequenceException">Thrown when the folder does not exist.</exception>
    public static DiskSequence Open(string folder)
    {
        if (!Directory.Exists(folder)) throw new UnknownSequenceException($"Cannot find folder: {folder}");

        var filePaths = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly);

        var pathList = filePaths.Order().ToList();

        return Open(pathList, Path.GetDirectoryName(folder) ?? folder);
    }

    /// <summary>
    /// Opens a new sequence.
    /// </summary>
    /// <param name="paths">The image paths the sequence uses.</param>
    /// <returns>The sequence</returns>
    /// <exception cref="UnknownSequenceException">Thrown when no suitable sequence can be found in the paths</exception>
    public static DiskSequence Open(List<Uri> paths)
    {
        var name = string.Join(", ", paths.Select(p => Path.GetFileName(p.AbsolutePath)));
        return Open(paths.Select(u => u.LocalPath).ToList(), name);
    }

    /// <summary>
    /// Opens a sequence from a folder.
    /// </summary>
    /// <param name="folder">The uri to the folder with the sequence inside</param>
    /// <returns>The opened sequence</returns>
    /// <exception cref="UnknownSequenceException">Thrown when the folder does not exist.</exception>
    public static DiskSequence Open(Uri folder)
    {
        Console.WriteLine(folder.ToString());
        return Open(folder.LocalPath);
    }

    /// <summary>
    /// Instantiates a sequence object from a sequence type and image paths.
    /// </summary>
    /// <param name="type">The type of the sequence.</param>
    /// <param name="paths">The paths of the used images.</param>
    /// <returns>The sequence object</returns>
    private static DiskSequence _InstantiateFromType(Type type, List<string> paths, string name)
    {
        if (!type.IsSubclassOf(typeof(DiskSequence)))
        {
            throw new CriticalBeamException($"{type} is not a subclass of Sequence!");
        }

        // opens constructor with List<string> as parameter (main constructor of Sequence)
        var ctor = type.GetConstructor([typeof(List<string>), typeof(string)]);
        if (ctor is null)
        {
            throw new CriticalBeamException($"Correct sequence constructor for {type} is not found!");
        }

        return (DiskSequence)ctor.Invoke([paths, name]);
    }

    private void ReleaseUnmanagedResources()
    {
        for (var i = 0; i < _loadedImages.Length; i++)
        {
            if (_loadedImages[i] is null) continue;
            _loadedImages[i]!.Dispose();
            _loadedImages[i] = null;
        }
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _loadedImagesMutex.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~DiskSequence()
    {
        Dispose(false);
    }
}