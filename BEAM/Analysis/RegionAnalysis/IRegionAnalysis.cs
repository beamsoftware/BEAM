using System.Drawing;
using Avalonia.Logging;
using BEAM.ViewModels;

namespace BEAM.Analysis;

public interface IRegionAnalysis
{
    Avalonia.Controls.Control AnalyseRegion(SubSequence area);
    Avalonia.Controls.Control AnalyseRegion(Point topLeft, Point bottomRight, SequenceViewModel viewModel);
    Avalonia.Controls.Control AnalyseRegion(long posX, long posY, SequenceViewModel viewModel);
}