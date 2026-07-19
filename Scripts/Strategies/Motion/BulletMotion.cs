using System;
using Godot;

/// <summary>
/// Data Layer / Pure Game Logic seam — abstract Godot Resource that defines the
/// motion contract for all bullet movement strategies.
/// </summary>
/// <remarks>
/// Concrete subclasses live in the Pure Game Logic Layer.  They must:
/// <list type="bullet">
///   <item>Override <see cref="Execute"/> as a pure function over its Span inputs (Rule 5.1).</item>
///   <item>Never hold references to any Godot <c>Node</c> (Rule 5.2).</item>
///   <item>Prefer SIMD intrinsics (<c>System.Numerics.Vector2</c>) for inner loops (Rule 5.3).</item>
///   <item>Carry <c>[GlobalClass]</c> so they appear in the Godot inspector resource picker (Rule 4).</item>
/// </list>
/// </remarks>
[GlobalClass]
public abstract partial class BulletMotion : Resource
{
    /// <summary>The base movement speed applied to all bullets on spawn (units/second).</summary>
    [Export]
    public float Speed { get; set; } = 200f;

    /// <summary>
    /// Optional rotational offset applied to the spawn velocity on top of the
    /// spawner's world rotation, in degrees (range −360 … 360).
    /// </summary>
    [Export(PropertyHint.Range, "-360,360,0.1,degrees")]
    public float DirectionAngle { get; set; } = 0f;

    /// <summary>
    /// Executes this motion strategy over a contiguous batch of active bullets.
    /// </summary>
    /// <remarks>
    /// Implementations must treat this as a pure function: read input spans,
    /// write output spans, and cache no state between calls (Rule 5.1).
    /// </remarks>
    /// <param name="positions">Mutable world-space positions of active bullets.</param>
    /// <param name="velocities">Mutable velocity vectors (can be modified for acceleration effects).</param>
    /// <param name="lifetimes">Read-only elapsed lifetimes for each bullet (seconds).</param>
    /// <param name="delta">Elapsed time since the previous frame (seconds).</param>
    public abstract void Execute(
        Span<System.Numerics.Vector2> positions,
        Span<System.Numerics.Vector2> velocities,
        ReadOnlySpan<float> lifetimes,
        float delta
    );
}
