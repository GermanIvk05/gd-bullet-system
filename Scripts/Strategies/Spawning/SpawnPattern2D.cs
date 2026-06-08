using System;
using System.Numerics;
using Godot;

[GlobalClass]
public abstract partial class SpawnPattern2D : Resource
{
    [Export]
    public int BulletCount { get; set; } = 10;

    /// <summary>
    /// Writes spawn data into the provided <see cref="BulletDataSlice"/>.
    /// Writes spawn data into the provided spans.
    /// <paramref name="positions"/> receives world-space spawn locations.
    /// <paramref name="velocities"/> receives unit direction vectors
    /// (the caller is responsible for applying speed and direction offsets).
    /// </summary>
    /// <param name="positions">Span for the new positions.</param>
    /// <param name="velocities">Span for the new velocities.</param>
    /// <param name="worldMatrix">The world-space transform of the spawner.</param>
    /// <returns>The number of bullets spawned.</returns>
    public abstract int Execute(
        Span<System.Numerics.Vector2> positions,
        Span<System.Numerics.Vector2> velocities,
        System.Numerics.Matrix3x2 worldMatrix
    );
}
