using System;
using Godot;

/// <summary>
/// Pure Game Logic Layer — motion strategy that moves each bullet in a straight
/// line at its initial velocity using a single SIMD-accelerated call.
/// </summary>
/// <remarks>
/// Rule 5.1: stateless — Execute is a pure function over its Span inputs.<br/>
/// Rule 5.2: no Godot Node references.<br/>
/// Rule 5.3: delegates all math to <see cref="SimdMath.MultiplyAdd"/> for SIMD acceleration.
/// </remarks>
[GlobalClass]
public partial class LinearBulletMotion : BulletMotion
{
    /// <inheritdoc/>
    public override void Execute(
        Span<System.Numerics.Vector2> positions,
        Span<System.Numerics.Vector2> velocities,
        ReadOnlySpan<float> lifetimes,
        float delta
    )
    {
        // positions[i] += velocities[i] * delta  — SIMD accelerated.
        SimdMath.MultiplyAdd(positions, velocities, delta);
    }
}
