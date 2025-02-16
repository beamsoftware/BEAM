using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BEAM.Image;
using BEAM.Image.Envi;
using BEAM.Models.Log;

namespace BEAM.ImageSequence;

/// <summary>
/// Implementation details for envi images.
/// </summary>
/// <param name="imagePaths">The envi images to use inside the sequence.</param>
public class EnviSequence(List<string> imagePaths, string name) : DiskSequence(imagePaths, name)
{
    protected internal override IImage LoadImage(int index) => EnviImage.OpenImage(ImagePaths[index]);

    protected internal override bool InitializeSequence()
    {
        var removed = ImagePaths.RemoveAll(path =>
            !(Path.GetExtension(path).Equals(".raw") || Path.GetExtension(path).Equals(".hdr")));

        var paths = ImagePaths.Select(p => Path.ChangeExtension(p, null)).Distinct().ToList();

        ImagePaths = paths.Select(p => Path.ChangeExtension(p, ".raw")).ToList();

        List<string> missingHdrFiles = [];
        foreach (var path in paths)
        {
            var hdr = Path.ChangeExtension(path, ".hdr");
            if (!File.Exists(hdr))
            {
                missingHdrFiles.Add(Path.GetFileName(hdr));
            }
        }

        if (missingHdrFiles.Count != 0)
        {
            Logger.GetInstance().Error(LogEvent.Sequence,
                $"Cannot load ENVI sequence due to missing hdr file(s): {string.Join(", ", missingHdrFiles)}");
            return false;
        }

        if (removed > 0)
        {
            Logger.GetInstance().Info(LogEvent.Sequence, $"{removed} file(s) were not loaded into the sequence.");
        }

        return true;
    }
}