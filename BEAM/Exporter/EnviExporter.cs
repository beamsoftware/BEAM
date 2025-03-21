using System;
using System.IO;
using System.Threading;
using Avalonia.Platform.Storage;
using BEAM.Image.Envi;
using BEAM.ImageSequence;
using BEAM.Models.Log;
using BEAM.Renderer;
using BEAM.ViewModels;

namespace BEAM.Exporter;

/// <summary>
/// Provides functionality to export image sequences in the ENVI format.
/// </summary>
public static class EnviExporter
{
    /// <summary>
    /// Exports the given sequence to the specified path in the ENVI format.
    /// </summary>
    /// <param name="path">The path where the files will be saved.</param>
    /// <param name="sequence">The sequence to be exported.</param>
    /// <param name="renderer">The renderer used for the sequence.</param>
    /// <param name="vm">The ViewModel used to store the progress as a data bound value.</param>
    public static void Export(IStorageFile path, TransformedSequence sequence, SequenceRenderer renderer, ExportProgressWindowViewModel vm)
    {

        CreateHeaderFile(path, sequence);
        using var stream = File.OpenWrite($"{path.Path.LocalPath}.raw");
        using var writer = new BinaryWriter(stream);
        long steps = sequence.Shape.Height / 100;
        CancellationToken ctx = vm.GetCancellationToken();
        for (var y = 0; y < sequence.Shape.Height; y++)
        {
            ctx.ThrowIfCancellationRequested();
            for (var x = 0; x < sequence.Shape.Width; x++)
            {
                var data = sequence.GetPixel(x, y);
                for (var k = 0; k < sequence.Shape.Channels; k++)
                {
                    writer.Write(data[k]);
                }
            }

            if (y % steps == 0)
            {
                vm.ActionProgress = (byte)Math.Round((y / (double)sequence.Shape.Height) *100);
            }
            
        }

        vm.Close();
        Logger.GetInstance().LogMessage($"Finished exporting sequence {sequence.GetName()} as ENVI to {path.Path.LocalPath}");
    }

    /// <summary>
    /// Creates the header file for the ENVI export.
    /// </summary>
    /// <param name="path">The folder where the header file will be saved.</param>
    /// <param name="sequence">The sequence for which the header file is created.</param>
    private static void CreateHeaderFile(IStorageFile path, TransformedSequence sequence)
    {
        var samples = sequence.Shape.Width;
        var lines = sequence.Shape.Height;
        var bands = sequence.Shape.Channels;
        const int headerOffset = 0;
        const string fileType = "ENVI Standard";
        const int dataType = (int)EnviDataType.Double;
        const EnviInterleave interleave = EnviInterleave.BIP;
        const int byteOrder = 0;
        var header = $"ENVI\r\nsamples = {samples}\r\nlines = {lines}\r\nbands = {bands}\r\nheader offset = {headerOffset}\r\nfile type = {fileType}\r\ndata type = {dataType}\r\ninterleave = {interleave.ToString().ToLower()}\r\nbyte order = {byteOrder}";
        using var stream = File.OpenWrite($"{path.Path.LocalPath}.hdr");
        using var writer = new StreamWriter(stream);
        writer.Write(header);
    }

    /// <summary>
    /// Cleans up any remnants of the export process. if it was canceled prematurely.
    /// </summary>
    /// <param name="folder">The path to which the original export was meant to take place.</param>
    public static void Cleanup(IStorageFile folder)
    {
        File.Delete($"{folder.Path.AbsolutePath}.hdr");
        File.Delete($"{folder.Path.AbsolutePath}.raw");
    }
}