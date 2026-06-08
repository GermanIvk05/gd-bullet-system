using System;
using System.Numerics;
using Godot;

[GlobalClass]
public partial class CurveBulletMotion : BulletMotion
{
    [Export]
    public Curve SpeedCurve { get; set; }

    [Export]
    public float CurveDuration { get; set; } = 1.0f;

    public override void Execute(
        Span<System.Numerics.Vector2> positions,
        Span<System.Numerics.Vector2> velocities,
        ReadOnlySpan<float> lifetimes,
        float delta
    )
    {
        for (int i = 0; i < positions.Length; i++)
        {
            float t = lifetimes[i] / CurveDuration;
            float multiplier = SpeedCurve != null ? SpeedCurve.SampleBaked(t) : 1f;

            positions[i] += velocities[i] * multiplier * delta;
        }
    }
}
