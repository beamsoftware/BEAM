using System;

namespace BEAM.Renderer.Attributes;
/// <summary>
/// This Attribute defines the range within which the raw values are contained.
/// </summary>
/// <param name="min">The lower bound should always be at most the lowest present value</param>
/// <param name="max">The higher bound should always be at least the highest present value</param>

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
public class ValueRangeAttribute(int min, int max) : Attribute
{
    /// <summary>
    /// The lower bound of the range the values are contained in.
    /// </summary>
    public int Min = min;
    /// <summary>
    /// The higher bound of the range the values are contained in.
    /// </summary>
    public int Max = max;
}