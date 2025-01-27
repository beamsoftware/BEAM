using System.Drawing;
using BEAM.ImageSequence;

namespace BEAM.Analysis;

public interface IPixelAnalysis
{
    Avalonia.Controls.Control analysePixel(long posX, long posY, Sequence sequence);
    Avalonia.Controls.Control analysePixel(Point point, Sequence sequence);
}