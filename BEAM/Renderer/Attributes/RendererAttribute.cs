namespace BEAM.Renderer.Attributes;
/// <summary>
/// This Attribute defines the default renderer for a sequence.
/// </summary>
/// <param name="defaultRenderer">The enum representation of the default Renderer which is to be used for this sequence.</param>

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
public class RendererAttribute(RendererEnum defaultRenderer) : System.Attribute
{
    public readonly RendererEnum DefaultRenderer = defaultRenderer;
}