using System.Collections.Generic;
using System.IO;
using System.Linq;
using BEAM.Image;
using BEAM.Image.Skia;
using BEAM.Models.Log;
using SkiaSharp;

namespace BEAM.ImageSequence;

/// <summary>
/// Implementation details for skia images.
/// </summary>
/// <param name="imagePaths">The skia images to use inside the sequence</param>
public class SkiaSequence(List<string> imagePaths, string name) : DiskSequence(imagePaths, name)
{
    private readonly List<string> _imagePaths = imagePaths;
    protected internal override IImage LoadImage(int index) => new SkiaImage<byte>(_imagePaths[index]);

    protected internal override bool InitializeSequence()
    {
        var removed = _imagePaths.RemoveAll(path => !Path.GetExtension(path).Equals(".png"));
        if (removed > 0)
            Logger.GetInstance().Info(LogEvent.Sequence, $"{removed} file(s) were not loaded into the sequence.");

        List<string> invalidImgs = [];

        // check all used images heights
        var firstImg = SKCodec.Create(_imagePaths[0]);
        var firstImgWidth = firstImg.Info.Width;
        var firstImgHeight = firstImg.Info.Height;

        // iterate over all remaining images (last image can have any height), but not any width
        foreach (var se in _imagePaths.Skip(1).Reverse().Skip(1).Reverse().Where(path =>
                 {
                     var codec = SKCodec.Create(path);
                     return codec.Info.Width != firstImgWidth || codec.Info.Height != firstImgHeight;
                 }).ToList())
        {
            _imagePaths.Remove(se);
            invalidImgs.Add(se);
        }

        if (_imagePaths.Count > 1)
        {
            // check if last image has correct width
            var lastImg = SKCodec.Create(_imagePaths[^1]);
            if (lastImg.Info.Width != firstImgWidth)
            {
                invalidImgs.Add(_imagePaths[^1]);
                _imagePaths.Remove(_imagePaths[^1]);
            }
        }

        if (invalidImgs.Count > 0)
        {
            Logger.GetInstance().Warning(LogEvent.Sequence,
                $"Cannot add {invalidImgs.Count} image(s) to the sequence due to invalid shape. Affected files: {string.Join(", ", invalidImgs)}");
        }

        return true;
    }
}