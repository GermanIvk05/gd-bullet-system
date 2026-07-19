using System;
using System.Numerics;
using Xunit;

namespace BulletSystem.Tests.Strategies.Motion;

/// <summary>
/// Unit tests for <see cref="OscillateBulletMotion"/> (Pure Game Logic Layer).
/// Rule 8: Tests pass fabricated Span&lt;T&gt; buffers directly; no Godot nodes instantiated.
/// </summary>
public class OscillateBulletMotionTest
{
    private static OscillateBulletMotion CreateStrategy(
        float amplitude = 50f,
        float frequency = 5f,
        float speed = 100f) =>
        new() { Amplitude = amplitude, Frequency = frequency, Speed = speed, DirectionAngle = 0f };

    [Fact]
    public void Execute_EmptySpans_DoesNotThrow()
    {
        var strategy = CreateStrategy();
        strategy.Execute(
            Span<Vector2>.Empty,
            Span<Vector2>.Empty,
            Span<float>.Empty,
            0.016f
        );
    }

    [Fact]
    public void Execute_ZeroAmplitude_BehavesLikeLinearMotion()
    {
        // With amplitude = 0, the oscillation term vanishes → pure forward motion.
        var strategy = CreateStrategy(amplitude: 0f, frequency: 5f);

        Span<Vector2> positions = [Vector2.Zero];
        Span<Vector2> velocities = [new Vector2(100f, 0f)];
        ReadOnlySpan<float> lifetimes = [0f];
        float delta = 0.016f;

        strategy.Execute(positions, velocities, lifetimes, delta);

        // With amplitude 0, lateralOffset delta is 0, so position moves by velocity * delta.
        Assert.Equal(1.6f, positions[0].X, precision: 4);
        Assert.Equal(0f, positions[0].Y, precision: 4);
    }

    [Fact]
    public void Execute_ZeroDelta_PositionsUnchanged()
    {
        // When delta = 0:
        //   forwardComponent = velocities[i] * 0 = (0, 0)
        //   prevOffset = sin(t * freq) * amp  (same as currentOffset)
        //   deltaOffset = 0
        // → position must not change.
        var strategy = CreateStrategy();

        Span<Vector2> positions = [new Vector2(5f, 5f)];
        Span<Vector2> velocities = [new Vector2(100f, 0f)];
        ReadOnlySpan<float> lifetimes = [1f];

        strategy.Execute(positions, velocities, lifetimes, 0f);

        Assert.Equal(new Vector2(5f, 5f), positions[0]);
    }

    [Fact]
    public void Execute_AtHalfPeriod_LateralOffsetIsZeroForSine()
    {
        // sin(0) = 0 and sin(π) = 0, so the lateral delta is also 0 at a full half-period jump.
        // This tests that the formula sin(t) - sin(t - delta) for a specific frame gives 0.
        var strategy = CreateStrategy(amplitude: 50f, frequency: 1f);

        float t = MathF.PI; // half period for sin(t * 1) = 0
        float delta = MathF.PI; // previous t was 0, also 0

        Span<Vector2> positions = [Vector2.Zero];
        Span<Vector2> velocities = [new Vector2(1f, 0f)];
        ReadOnlySpan<float> lifetimes = [t];

        strategy.Execute(positions, velocities, lifetimes, delta);

        // Both sin(π) and sin(0) = 0, so lateral delta = 0.
        // Forward component = vel * delta = (π, 0).
        Assert.Equal(0f, positions[0].Y, precision: 4);
    }

    [Fact]
    public void Execute_DoesNotModifyVelocities()
    {
        var strategy = CreateStrategy();
        var originalVel = new Vector2(100f, 0f);

        Span<Vector2> positions = [Vector2.Zero];
        Span<Vector2> velocities = [originalVel];
        ReadOnlySpan<float> lifetimes = [0.5f];

        strategy.Execute(positions, velocities, lifetimes, 0.016f);

        // OscillateBulletMotion must never modify the velocity buffer.
        Assert.Equal(originalVel, velocities[0]);
    }

    [Fact]
    public void Execute_MultipleBullets_AllUpdated()
    {
        var strategy = CreateStrategy(amplitude: 0f); // zero amplitude = pure forward

        Span<Vector2> positions = [Vector2.Zero, Vector2.Zero];
        Span<Vector2> velocities = [new Vector2(10f, 0f), new Vector2(0f, 10f)];
        ReadOnlySpan<float> lifetimes = [0f, 0f];

        strategy.Execute(positions, velocities, lifetimes, 1f);

        // Forward movement only (amplitude = 0)
        Assert.Equal(10f, positions[0].X, precision: 4);
        Assert.Equal(0f, positions[0].Y, precision: 4);
        Assert.Equal(0f, positions[1].X, precision: 4);
        Assert.Equal(10f, positions[1].Y, precision: 4);
    }
}
