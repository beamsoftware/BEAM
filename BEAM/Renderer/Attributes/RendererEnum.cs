namespace BEAM.Renderer.Attributes;

/// <summary>
/// This enum represents all the available Renderers and is used to create renderer attributes
/// for (natural) sequences (e.g. Skia/EnviSequence).
/// </summary>
public enum RendererEnum
{
    ChannelMapRenderer,
    HeatMapRendererRB,
    ArgMaxRendererGrey,
    ArgMaxRendererColorHSV
}