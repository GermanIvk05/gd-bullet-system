using System;
using System.Numerics;
using Xunit;

namespace BulletSystem.Tests.Strategies.Spawning;

/// <summary>
/// Unit tests for <see cref="CircleSpawnPattern2D"/> (Pure Game Logic Layer).
/// Rule 8: Tests pass fabricated Span&lt;T&gt; buffers directly; no Godot nodes instantiated.
/// </summary>
public class CircleSpawnPattern2DTest
{
    /// <summary>Identity world matrix (no rotation, no translation).</summary>
    private static readonly Matrix3x2 Identity = Matrix3x2.Identity;

    private static CircleSpawnPattern2D CreatePattern(int count = 4, float radius = 50f) =>
        new() { BulletCount = count, Radius = radius };

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
        var pattern = CreatePattern(count: 8);
        var positions = new Vector2[8];
        var velocities = new Vector2[8];

        int result = pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        Assert.Equal(8, result);
    }

    [Fact]
    public void Execute_VelocitiesAreUnitVectors()
    {
        var pattern = CreatePattern(count: 8);
        var positions = new Vector2[8];
        var velocities = new Vector2[8];

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        foreach (var vel in velocities)
        {
            float length = vel.Length();
            Assert.Equal(1f, length, precision: 5);
        }
    }

    [Fact]
    public void Execute_PositionsAreOnRadius()
    {
        const float radius = 100f;
        var pattern = CreatePattern(count: 6, radius: radius);
        var positions = new Vector2[6];
        var velocities = new Vector2[6];

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        // With identity world matrix, all positions should be exactly at radius distance.
        foreach (var pos in positions)
        {
            Assert.Equal(radius, pos.Length(), precision: 4);
        }
    }

    [Fact]
    public void Execute_SingleBullet_PointsRight()
    {
        // One bullet → angle = 0 → direction (1, 0)
        var pattern = CreatePattern(count: 1, radius: 50f);
        var positions = new Vector2[1];
        var velocities = new Vector2[1];

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        Assert.Equal(1f, velocities[0].X, precision: 5);
        Assert.Equal(0f, velocities[0].Y, precision: 5);
    }

    // -------------------------------------------------------------------------
    // World matrix
    // -------------------------------------------------------------------------

    [Fact]
    public void Execute_WithTranslation_PositionsAreOffset()
    {
        var pattern = CreatePattern(count: 4, radius: 0f); // radius=0 → all at origin
        var positions = new Vector2[4];
        var velocities = new Vector2[4];
        var translation = Matrix3x2.CreateTranslation(100f, 200f);

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), translation);

        // With radius=0, all positions should be at the translation offset.
        foreach (var pos in positions)
        {
            Assert.Equal(100f, pos.X, precision: 4);
            Assert.Equal(200f, pos.Y, precision: 4);
        }
    }

    // -------------------------------------------------------------------------
    // Symmetry
    // -------------------------------------------------------------------------

    [Fact]
    public void Execute_FourBullets_SymmetricPositions()
    {
        // 4 bullets → positions at 0°, 90°, 180°, 270° → should be (r,0), (0,r), (-r,0), (0,-r)
        const float radius = 50f;
        var pattern = CreatePattern(count: 4, radius: radius);
        var positions = new Vector2[4];
        var velocities = new Vector2[4];

        pattern.Execute(positions.AsSpan(), velocities.AsSpan(), Identity);

        Assert.Equal(radius, positions[0].X, precision: 4);
        Assert.Equal(0f, positions[0].Y, precision: 4);

        Assert.Equal(0f, positions[1].X, precision: 4);
        Assert.Equal(radius, positions[1].Y, precision: 4);

        Assert.Equal(-radius, positions[2].X, precision: 4);
        Assert.Equal(0f, positions[2].Y, precision: 4);

        Assert.Equal(0f, positions[3].X, precision: 4);
        Assert.Equal(-radius, positions[3].Y, precision: 4);
    }
}
