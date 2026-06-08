using System;
using Godot;

[GlobalClass]
public partial class LinearBulletMotion : BulletMotion
{
    public override void Execute(
        Span<System.Numerics.Vector2> positions,
        Span<System.Numerics.Vector2> velocities,
        ReadOnlySpan<float> lifetimes,
        float delta
    )
    {
        SimdMath.MultiplyAdd(positions, velocities, delta);
    }
}
