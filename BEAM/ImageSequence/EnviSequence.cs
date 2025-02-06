using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BEAM.Image;
using BEAM.Image.Envi;
using BEAM.Log;

namespace BEAM.ImageSequence;

/// <summary>
/// Implementation details for envi images.
/// </summary>
/// <param name="imagePaths">The envi images to use inside the sequence.</param>
public class EnviSequence(List<string> imagePaths, string name) : DiskSequence(imagePaths, name)
{
    protected override IImage LoadImage(int index) => EnviImage.OpenImage(imagePaths[index]);

    protected override bool InitializeSequence()
    {
        var removed = imagePaths.RemoveAll(path =>
            !(Path.GetExtension(path).Equals(".raw") || Path.GetExtension(path).Equals(".hdr")));

        List<string> missingHdrFiles = [];
        foreach (var path in imagePaths.Where(p => Path.GetExtension(p).Equals(".raw")))
        {
            var hdr = Path.ChangeExtension(path, ".hdr");
            if (!imagePaths.Contains(hdr))
            {
                missingHdrFiles.Add(Path.GetFileName(hdr));
            }
        }

        if (missingHdrFiles.Count != 0)
        {
            Logger.GetInstance().Error(LogEvent.Critical, $"Cannot load ENVI sequence due to missing hdr files: {string.Join(", ", missingHdrFiles)}");
            return false;
        }

        imagePaths.RemoveAll(path => !Path.GetExtension(path).Equals(".hdr"));

        if (removed > 0)
        {
            Logger.GetInstance().Info(LogEvent.OpenedFile, $"{removed} file(s) were not loaded into the sequence.");
        }

        return true;
    }
}