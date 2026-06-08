using System;
using System;
using System.Numerics;
using Godot;

[GlobalClass]
public partial class OscillateBulletMotion : BulletMotion
{
    [Export]
    public float Amplitude { get; set; } = 50f;

    [Export]
    public float Frequency { get; set; } = 5f;

    public override void Execute(
        Span<System.Numerics.Vector2> positions,
        ReadOnlySpan<System.Numerics.Vector2> velocities,
        ReadOnlySpan<float> lifetimes,
        float delta
    )
    {
        for (int i = 0; i < positions.Length; i++)
        {
            float t = lifetimes[i];

            // Primary direction (normalized)
            var forwardDir = System.Numerics.Vector2.Normalize(velocities[i]);

            // Perpendicular direction (-y, x)
            var rightDir = new System.Numerics.Vector2(-forwardDir.Y, forwardDir.X);

            // Compute current and previous lateral offset
            float currentOffset = MathF.Sin(t * Frequency) * Amplitude;
            float prevOffset = MathF.Sin((t - delta) * Frequency) * Amplitude;
            float deltaOffset = currentOffset - prevOffset;

            // Advance position: forward velocity + lateral difference
            positions[i] += (velocities[i] * delta) + (rightDir * deltaOffset);
        }
    }
}
