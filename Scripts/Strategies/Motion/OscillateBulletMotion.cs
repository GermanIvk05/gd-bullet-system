using System;
using System.Numerics;
using Godot;

/// <summary>
/// Pure Game Logic Layer — motion strategy that adds a sinusoidal lateral
/// oscillation on top of the bullet's primary forward velocity.
/// </summary>
/// <remarks>
/// The lateral offset is computed as the delta between the sine-wave value at
/// the current and previous lifetime, so the oscillation integrates cleanly with
/// varying frame-rates without drift.<br/><br/>
/// Rule 5.1: stateless — Execute is a pure function over its Span inputs.<br/>
/// Rule 5.2: no Godot Node references.<br/>
/// Rule 5.3: scalar loop is used here because per-bullet perpendicular vector
/// computation requires a conditional branch-free normalize that defeats
/// auto-vectorisation; a future SIMD path can replace this if profiling warrants.
/// </remarks>
[GlobalClass]
public partial class OscillateBulletMotion : BulletMotion
{
    /// <summary>The peak lateral displacement from the bullet's path centre (world units).</summary>
    [Export]
    public float Amplitude { get; set; } = 50f;

    /// <summary>The oscillation frequency (cycles per second).</summary>
    [Export]
    public float Frequency { get; set; } = 5f;

    /// <inheritdoc/>
    public override void Execute(
        Span<System.Numerics.Vector2> positions,
        Span<System.Numerics.Vector2> velocities,
        ReadOnlySpan<float> lifetimes,
        float delta
    )
    {
        for (int i = 0; i < positions.Length; i++)
        {
            float t = lifetimes[i];

            // Primary direction (unit vector along velocity).
            var forwardDir = Vector2.Normalize(velocities[i]);

            // Perpendicular direction: rotate 90° counter-clockwise (-y, x).
            var rightDir = new Vector2(-forwardDir.Y, forwardDir.X);

            // Compute current and previous lateral offset to get the frame delta.
            float currentOffset = MathF.Sin(t * Frequency) * Amplitude;
            float prevOffset = MathF.Sin((t - delta) * Frequency) * Amplitude;
            float deltaOffset = currentOffset - prevOffset;

            // Advance position: forward velocity + lateral sine delta.
            positions[i] += (velocities[i] * delta) + (rightDir * deltaOffset);
        }
    }
}
