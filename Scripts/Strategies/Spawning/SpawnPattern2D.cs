using System;
using System.Numerics;
using Godot;

/// <summary>
/// Data Layer / Pure Game Logic seam — abstract Godot Resource that defines the
/// spawn-pattern contract for all bullet emission strategies.
/// </summary>
/// <remarks>
/// Concrete subclasses live in the Pure Game Logic Layer.  They must:
/// <list type="bullet">
///   <item>Override <see cref="Execute"/> as a pure function over its Span inputs (Rule 5.1).</item>
///   <item>Never hold references to any Godot <c>Node</c> (Rule 5.2).</item>
///   <item>Carry <c>[GlobalClass]</c> so they appear in the Godot inspector resource picker (Rule 4).</item>
///   <item>Write <em>unit</em> direction vectors into <paramref name="velocities"/>; the caller
///         (<see cref="BulletSystem2D"/>) applies speed and direction offsets via
///         <see cref="SimdMath.ApplySpeedAndRotation"/> after Execute returns.</item>
/// </list>
/// </remarks>
[GlobalClass]
public abstract partial class SpawnPattern2D : Resource
{
    /// <summary>The number of bullets emitted per spawn call.</summary>
    [Export]
    public int BulletCount { get; set; } = 10;

    /// <summary>
    /// Writes spawn data into the provided spans.
    /// <paramref name="positions"/> receives world-space spawn locations.
    /// <paramref name="velocities"/> receives unit direction vectors;
    /// the caller (<see cref="BulletSystem2D"/>) is responsible for applying speed
    /// and direction offsets via <see cref="SimdMath.ApplySpeedAndRotation"/>.
    /// </summary>
    /// <param name="positions">Span to fill with world-space spawn positions.</param>
    /// <param name="velocities">Span to fill with unit direction vectors.</param>
    /// <param name="worldMatrix">The world-space transform of the spawner node.</param>
    /// <returns>The number of bullets actually spawned (≤ positions.Length).</returns>
    public abstract int Execute(
        Span<System.Numerics.Vector2> positions,
        Span<System.Numerics.Vector2> velocities,
        System.Numerics.Matrix3x2 worldMatrix
    );
}
