// (c) Paul Stier, 2025

using System;
using Avalonia.Platform;
using BEAM.Image.Skia;
using SkiaSharp;

namespace BEAM.Image;

public class AssetImage(string assetUri) : SkiaImage<byte>(AssetLoader.Open(new Uri(assetUri)))
{
    public SKBitmap GetBitmap()
    {
        return Data!;
    }
}