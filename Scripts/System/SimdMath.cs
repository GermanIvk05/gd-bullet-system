using System;
using System.Numerics;
using System.Runtime.InteropServices;

public static class SimdMath
{
    /// <summary>
    /// SIMD Accelerated: Adds a scalar value to every element in the span.
    /// Equivalent to: data[i] += scalar
    /// </summary>
    public static void AddScalar(Span<float> data, float scalar)
    {
        int vectorSize = Vector<float>.Count;
        var scalarVec = new Vector<float>(scalar);

        int i = 0;
        for (; i <= data.Length - vectorSize; i += vectorSize)
        {
            var slice = data.Slice(i, vectorSize);
            var vec = new Vector<float>(slice);
            (vec + scalarVec).CopyTo(slice);
        }

        // Remainder
        for (; i < data.Length; i++)
        {
            data[i] += scalar;
        }
    }

    /// <summary>
    /// SIMD Accelerated: Adds (b * scalar) to a.
    /// Equivalent to: a[i] += b[i] * scalar
    /// Note: This natively handles Vector2 spans by casting them to flat float spans.
    /// </summary>
    public static void MultiplyAdd(Span<Vector2> a, ReadOnlySpan<Vector2> b, float scalar)
    {
        var aFloat = MemoryMarshal.Cast<Vector2, float>(a);
        var bFloat = MemoryMarshal.Cast<Vector2, float>(b);

        int vectorSize = Vector<float>.Count;
        var scalarVec = new Vector<float>(scalar);

        int i = 0;
        for (; i <= aFloat.Length - vectorSize; i += vectorSize)
        {
            var aSlice = aFloat.Slice(i, vectorSize);
            var bSlice = bFloat.Slice(i, vectorSize);

            var aVec = new Vector<float>(aSlice);
            var bVec = new Vector<float>(bSlice);

            (aVec + bVec * scalarVec).CopyTo(aSlice);
        }

        // Remainder
        for (; i < aFloat.Length; i++)
        {
            aFloat[i] += bFloat[i] * scalar;
        }
    }

    /// <summary>
    /// Rotates a span of velocity direction vectors by an angle (in degrees) and applies a speed multiplier.
    /// Used during bullet spawning.
    /// </summary>
    public static void ApplySpeedAndRotation(
        Span<Vector2> velocities,
        float speed,
        float directionDegrees
    )
    {
        if (velocities.IsEmpty)
            return;

        if (directionDegrees == 0f)
        {
            for (int i = 0; i < velocities.Length; i++)
            {
                velocities[i] *= speed;
            }
            return;
        }

        float rad = directionDegrees * MathF.PI / 180f;
        float cos = MathF.Cos(rad);
        float sin = MathF.Sin(rad);

        for (int i = 0; i < velocities.Length; i++)
        {
            var dir = velocities[i];
            velocities[i] =
                new Vector2(dir.X * cos - dir.Y * sin, dir.X * sin + dir.Y * cos) * speed;
        }
    }
}
