using System;
using System.Numerics;
using Godot;

/// <summary>
/// Pure Game Logic Layer — motion strategy that modulates each bullet's speed
/// over time according to a <see cref="Godot.Curve"/> sampled from 0 → 1 over
/// <see cref="CurveDuration"/> seconds.
/// </summary>
/// <remarks>
/// Rule 5.1: stateless — the Curve resource is read-only; no per-frame state is cached.<br/>
/// Rule 5.2: no Godot Node references — <c>Godot.Curve</c> is a Resource, not a Node.<br/>
/// Rule 5.3: the inner loop is scalar because curve sampling is a non-vectorisable
/// transcendental lookup; the forward-movement step uses direct Vector2 arithmetic.
/// </remarks>
[GlobalClass]
public partial class CurveBulletMotion : BulletMotion
{
    /// <summary>
    /// Animation curve that controls the speed multiplier over the bullet's
    /// lifetime.  The horizontal axis is normalised to [0, 1] over
    /// <see cref="CurveDuration"/> seconds.  Falls back to a multiplier of 1
    /// when null.
    /// </summary>
    [Export]
    public Curve? SpeedCurve { get; set; }

    /// <summary>The number of seconds over which the full curve is evaluated.</summary>
    [Export]
    public float CurveDuration { get; set; } = 1.0f;

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
            float t = lifetimes[i] / CurveDuration;
            float multiplier = SpeedCurve != null ? SpeedCurve.SampleBaked(t) : 1f;

            positions[i] += velocities[i] * (multiplier * delta);
        }
    }
}
