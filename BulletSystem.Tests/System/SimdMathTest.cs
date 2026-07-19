using System;
using System.Numerics;
using Xunit;

namespace BulletSystem.Tests.System;

/// <summary>
/// Unit tests for <see cref="SimdMath"/> (Data Layer).
/// Rule 8: Tests pass fabricated Span&lt;T&gt; buffers directly to the static methods.
/// No Godot nodes or scenes are instantiated.
/// </summary>
public class SimdMathTest
{
    // -------------------------------------------------------------------------
    // AddScalar
    // -------------------------------------------------------------------------

    [Fact]
    public void AddScalar_EmptySpan_DoesNotThrow()
    {
        Span<float> data = Span<float>.Empty;
        SimdMath.AddScalar(data, 1f); // must not throw
    }

    [Fact]
    public void AddScalar_SingleElement_AddsCorrectly()
    {
        Span<float> data = [3.0f];
        SimdMath.AddScalar(data, 1.5f);
        Assert.Equal(4.5f, data[0], precision: 5);
    }

    [Fact]
    public void AddScalar_LargeSpan_AllElementsIncremented()
    {
        const int count = 1024;
        float[] array = new float[count];
        Array.Fill(array, 2.0f);

        SimdMath.AddScalar(array.AsSpan(), 0.016f);

        foreach (float v in array)
            Assert.Equal(2.016f, v, precision: 5);
    }

    [Fact]
    public void AddScalar_NonMultipleOfSimdWidth_HandlesRemainder()
    {
        // Use an odd-length span to exercise the scalar remainder path.
        Span<float> data = [1f, 2f, 3f, 4f, 5f, 6f, 7f];
        SimdMath.AddScalar(data, 10f);

        for (int i = 0; i < data.Length; i++)
            Assert.Equal((float)(i + 1) + 10f, data[i], precision: 5);
    }

    // -------------------------------------------------------------------------
    // MultiplyAdd
    // -------------------------------------------------------------------------

    [Fact]
    public void MultiplyAdd_EmptySpans_DoesNotThrow()
    {
        Span<Vector2> a = Span<Vector2>.Empty;
        ReadOnlySpan<Vector2> b = ReadOnlySpan<Vector2>.Empty;
        SimdMath.MultiplyAdd(a, b, 0.016f); // must not throw
    }

    [Fact]
    public void MultiplyAdd_SingleElement_CorrectResult()
    {
        Span<Vector2> positions = [new Vector2(0f, 0f)];
        ReadOnlySpan<Vector2> velocities = [new Vector2(100f, 200f)];
        float delta = 0.016f;

        SimdMath.MultiplyAdd(positions, velocities, delta);

        Assert.Equal(1.6f, positions[0].X, precision: 4);
        Assert.Equal(3.2f, positions[0].Y, precision: 4);
    }

    [Fact]
    public void MultiplyAdd_LargeSpan_AllElementsUpdated()
    {
        const int count = 512;
        var positions = new Vector2[count];
        var velocities = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            positions[i] = new Vector2(0f, 0f);
            velocities[i] = new Vector2(10f, 20f);
        }

        SimdMath.MultiplyAdd(positions.AsSpan(), (ReadOnlySpan<Vector2>)velocities, 1f);

        foreach (var p in positions)
        {
            Assert.Equal(10f, p.X, precision: 4);
            Assert.Equal(20f, p.Y, precision: 4);
        }
    }

    [Fact]
    public void MultiplyAdd_NonMultipleOfSimdWidth_HandlesRemainder()
    {
        // 3 elements — unlikely to be a multiple of the SIMD width.
        Span<Vector2> positions = [Vector2.Zero, Vector2.Zero, Vector2.Zero];
        ReadOnlySpan<Vector2> velocities = [
            new Vector2(1f, 2f),
            new Vector2(3f, 4f),
            new Vector2(5f, 6f)
        ];
        SimdMath.MultiplyAdd(positions, velocities, 2f);

        Assert.Equal(new Vector2(2f, 4f), positions[0]);
        Assert.Equal(new Vector2(6f, 8f), positions[1]);
        Assert.Equal(new Vector2(10f, 12f), positions[2]);
    }

    // -------------------------------------------------------------------------
    // ApplySpeedAndRotation
    // -------------------------------------------------------------------------

    [Fact]
    public void ApplySpeedAndRotation_EmptySpan_DoesNotThrow()
    {
        SimdMath.ApplySpeedAndRotation(Span<Vector2>.Empty, 200f, 0f);
    }

    [Fact]
    public void ApplySpeedAndRotation_ZeroRotation_OnlyScalesBySpeed()
    {
        Span<Vector2> vels = [new Vector2(1f, 0f), new Vector2(0f, 1f)];
        SimdMath.ApplySpeedAndRotation(vels, 100f, 0f);

        Assert.Equal(new Vector2(100f, 0f), vels[0]);
        Assert.Equal(new Vector2(0f, 100f), vels[1]);
    }

    [Fact]
    public void ApplySpeedAndRotation_90Degrees_RotatesCorrectly()
    {
        // A unit vector pointing right (1,0) rotated 90° CCW → (0, 1)
        Span<Vector2> vels = [new Vector2(1f, 0f)];
        SimdMath.ApplySpeedAndRotation(vels, 1f, 90f);

        Assert.Equal(0f, vels[0].X, precision: 5);
        Assert.Equal(1f, vels[0].Y, precision: 5);
    }

    [Fact]
    public void ApplySpeedAndRotation_180Degrees_FlipsDirection()
    {
        // A unit vector pointing right (1,0) rotated 180° → (-1, 0)
        Span<Vector2> vels = [new Vector2(1f, 0f)];
        SimdMath.ApplySpeedAndRotation(vels, 1f, 180f);

        Assert.Equal(-1f, vels[0].X, precision: 5);
        Assert.Equal(0f, vels[0].Y, precision: 5);
    }

    [Fact]
    public void ApplySpeedAndRotation_SpeedAndRotation_Combined()
    {
        // (1,0) rotated 90° at speed 200 → (0, 200)
        Span<Vector2> vels = [new Vector2(1f, 0f)];
        SimdMath.ApplySpeedAndRotation(vels, 200f, 90f);

        Assert.Equal(0f, vels[0].X, precision: 3);
        Assert.Equal(200f, vels[0].Y, precision: 3);
    }
}
