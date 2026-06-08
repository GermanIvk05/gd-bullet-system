using System;
using Godot;

[GlobalClass]
public abstract partial class BulletMotion : Resource
{
    [Export]
    public float Speed { get; set; } = 200f;

    [Export(PropertyHint.Range, "-360,360,0.1,degrees")]
    public float DirectionAngle { get; set; } = 0f;

    /// <summary>
    /// Executes this motion over a batch of bullets.
    /// </summary>
    /// <param name="positions">The active positions span.</param>
    /// <param name="velocities">The active velocities span.</param>
    /// <param name="lifetimes">The active lifetimes span.</param>
    /// <param name="delta">The time elapsed since the last frame.</param>
    public abstract void Execute(
        Span<System.Numerics.Vector2> positions,
        ReadOnlySpan<System.Numerics.Vector2> velocities,
        ReadOnlySpan<float> lifetimes,
        float delta
    );
}
