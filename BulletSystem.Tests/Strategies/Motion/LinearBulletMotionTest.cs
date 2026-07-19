using System;
using System.Numerics;
using Xunit;

namespace BulletSystem.Tests.Strategies.Motion;

/// <summary>
/// Unit tests for <see cref="LinearBulletMotion"/> (Pure Game Logic Layer).
/// Rule 8: Tests pass fabricated Span&lt;T&gt; buffers directly; no Godot nodes instantiated.
/// </summary>
public class LinearBulletMotionTest
{
    private static LinearBulletMotion CreateStrategy(float speed = 100f) =>
        new() { Speed = speed, DirectionAngle = 0f };

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
    public void Execute_SingleBullet_MovesAlongVelocity()
    {
        var strategy = CreateStrategy();

        Span<Vector2> positions = [new Vector2(0f, 0f)];
        Span<Vector2> velocities = [new Vector2(100f, 0f)];
        ReadOnlySpan<float> lifetimes = [0.5f];
        float delta = 0.016f;

        strategy.Execute(positions, velocities, lifetimes, delta);

        Assert.Equal(1.6f, positions[0].X, precision: 4);
        Assert.Equal(0f, positions[0].Y, precision: 4);
    }

    [Fact]
    public void Execute_MultipleBullets_AllMovedCorrectly()
    {
        var strategy = CreateStrategy();

        Span<Vector2> positions = [Vector2.Zero, Vector2.Zero, Vector2.Zero];
        Span<Vector2> velocities = [
            new Vector2(10f, 0f),
            new Vector2(0f, 20f),
            new Vector2(-5f, 5f)
        ];
        ReadOnlySpan<float> lifetimes = [0f, 0f, 0f];
        float delta = 1f; // 1 second for easy math

        strategy.Execute(positions, velocities, lifetimes, delta);

        Assert.Equal(new Vector2(10f, 0f), positions[0]);
        Assert.Equal(new Vector2(0f, 20f), positions[1]);
        Assert.Equal(new Vector2(-5f, 5f), positions[2]);
    }

    [Fact]
    public void Execute_DoesNotModifyVelocities()
    {
        var strategy = CreateStrategy();
        var originalVel = new Vector2(100f, 50f);

        Span<Vector2> positions = [Vector2.Zero];
        Span<Vector2> velocities = [originalVel];
        ReadOnlySpan<float> lifetimes = [0f];

        strategy.Execute(positions, velocities, lifetimes, 0.016f);

        // LinearBulletMotion must never modify the velocity buffer.
        Assert.Equal(originalVel, velocities[0]);
    }

    [Fact]
    public void Execute_ZeroDelta_PositionsUnchanged()
    {
        var strategy = CreateStrategy();

        Span<Vector2> positions = [new Vector2(5f, 10f)];
        Span<Vector2> velocities = [new Vector2(100f, 200f)];
        ReadOnlySpan<float> lifetimes = [1f];

        strategy.Execute(positions, velocities, lifetimes, 0f);

        Assert.Equal(new Vector2(5f, 10f), positions[0]);
    }
}
