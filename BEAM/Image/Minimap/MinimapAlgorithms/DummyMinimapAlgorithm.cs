using System.Threading.Tasks;
using BEAM.ImageSequence;

namespace BEAM.Image.Minimap.MinimapAlgorithms;

public class DummyMinimapAlgorithm : IMinimapAlgorithm
{
    public bool AnalyzeSequence(Sequence sequence)
    {
        Task.Delay(1000);
        return true;
    }

    public float GetLineValue(long line)
    {
        return 1.0f;
    }
}