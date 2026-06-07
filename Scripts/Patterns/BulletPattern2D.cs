using System;
using System.Numerics;
using Godot;

[GlobalClass]
public abstract partial class BulletPattern2D : Resource
{
    [Export]
    public int BulletCount { get; set; } = 10;

    /// <summary>
    /// Fills the provided buffer with bullet transforms relative to the given world matrix.
    /// </summary>
    /// <param name="buffer">
    /// A span of <see cref="Matrix3x2"/> to fill. Length determines the maximum number of bullets.
    /// </param>
    /// <param name="worldMatrix">
    /// The world-space transform of the spawn origin (position + rotation).
    /// </param>
    /// <returns>The number of bullets actually written to the buffer.</returns>
    public abstract int FillBuffer(Span<Matrix3x2> buffer, Matrix3x2 worldMatrix);
}
