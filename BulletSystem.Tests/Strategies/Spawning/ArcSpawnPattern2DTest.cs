using System;
using System.Numerics;
using Xunit;

namespace BulletSystem.Tests.Strategies.Spawning;

/// <summary>
/// Unit tests for <see cref="ArcSpawnPattern2D"/> (Pure Game Logic Layer).
/// Rule 8: Tests pass fabricated Span&lt;T&gt; buffers directly; no Godot nodes instantiated.
/// </summary>
public class ArcSpawnPattern2DTest
{
    /// <summary>Identity world matrix (no rotation, no translation, facing right).</summary>
    private static readonly Matrix3x2 Identity = Matrix3x2.Identity;

    private static ArcSpawnPattern2D CreatePattern(
        int count = 3,
        float radius = 50f,
        float spreadAngle = 90f) =>
        new() { BulletCount = count, Radius = radius, SpreadAngle = spreadAngle };

    // -------------------------------------------------------------------------
    // Basic correctness
    // -------------------------------------------------------------------------

    [Fact]
    public void Execute_EmptySpan_ReturnsZero()
    {
        var pattern = CreatePattern();
        int result = pattern.Execute(
            Span<Vector2>.Empty,
            Span<Vector2>.Empty,
            Identity
        );
        Assert.Equal(0, result);
    }

    [Fact]
    public void Execute_ReturnsBulletCount()
    {
        var pattern = CreatePattern(count: 5);
        var positions = new Vector2[5];
        var velocities = new Vector2[5];

        int result = pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        Assert.Equal(5, result);
    }

    [Fact]
    public void Execute_VelocitiesAreUnitVectors()
    {
        var pattern = CreatePattern(count: 7);
        var positions = new Vector2[7];
        var velocities = new Vector2[7];

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        foreach (var vel in velocities)
            Assert.Equal(1f, vel.Length(), precision: 5);
    }

    // -------------------------------------------------------------------------
    // Single-bullet special case
    // -------------------------------------------------------------------------

    [Fact]
    public void Execute_OneBullet_FiresAlongForwardAxis()
    {
        // Identity matrix → facing direction is (1, 0)
        var pattern = CreatePattern(count: 1, radius: 50f, spreadAngle: 90f);
        var positions = new Vector2[1];
        var velocities = new Vector2[1];

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        // Direction should be forward (1, 0) for the identity matrix.
        Assert.Equal(1f, velocities[0].X, precision: 5);
        Assert.Equal(0f, velocities[0].Y, precision: 5);
    }

    // -------------------------------------------------------------------------
    // Spread symmetry
    // -------------------------------------------------------------------------

    [Fact]
    public void Execute_ThreeBullets_ZeroAndEdgesSymmetric()
    {
        // With 3 bullets and identity matrix (facing right = 0°),
        // and a spread of 90°, the angles should be: -45°, 0°, +45°.
        var pattern = CreatePattern(count: 3, radius: 50f, spreadAngle: 90f);
        var positions = new Vector2[3];
        var velocities = new Vector2[3];

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        // Centre bullet (index 1) should face exactly forward (1, 0)
        Assert.Equal(1f, velocities[1].X, precision: 4);
        Assert.Equal(0f, velocities[1].Y, precision: 4);

        // Left and right bullets should be symmetric about the forward axis.
        Assert.Equal(velocities[0].X, velocities[2].X, precision: 5);
        Assert.Equal(-velocities[0].Y, velocities[2].Y, precision: 5);
    }

    [Fact]
    public void Execute_ZeroSpread_AllBulletsPointSameDirection()
    {
        var pattern = CreatePattern(count: 4, spreadAngle: 0f);
        var positions = new Vector2[4];
        var velocities = new Vector2[4];

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        for (int i = 1; i < velocities.Length; i++)
        {
            Assert.Equal(velocities[0].X, velocities[i].X, precision: 5);
            Assert.Equal(velocities[0].Y, velocities[i].Y, precision: 5);
        }
    }

    // -------------------------------------------------------------------------
    // World matrix
    // -------------------------------------------------------------------------

    [Fact]
    public void Execute_With90DegreeRotation_DirectionRotated()
    {
        // Rotate the world matrix 90° CCW.  A single bullet should then fire
        // upward (0, 1) instead of rightward (1, 0).
        var rotation90 = Matrix3x2.CreateRotation(MathF.PI / 2f);
        var pattern = CreatePattern(count: 1, radius: 0f, spreadAngle: 0f);
        var positions = new Vector2[1];
        var velocities = new Vector2[1];

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), rotation90);

        Assert.Equal(0f, velocities[0].X, precision: 5);
        Assert.Equal(1f, velocities[0].Y, precision: 5);
    }

    [Fact]
    public void Execute_WithTranslation_PositionsAreOffset()
    {
        var pattern = CreatePattern(count: 1, radius: 0f);
        var positions = new Vector2[1];
        var velocities = new Vector2[1];
        var translation = Matrix3x2.CreateTranslation(50f, 75f);

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), translation);

        // radius=0 → position should be exactly at the translation offset.
        Assert.Equal(50f, positions[0].X, precision: 4);
        Assert.Equal(75f, positions[0].Y, precision: 4);
    }
}
