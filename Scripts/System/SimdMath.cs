using System;
using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// Data Layer — static class providing SIMD-accelerated math helpers used by
/// the bullet simulation system.
/// </summary>
/// <remarks>
/// All methods in this class are pure functions: they only read their input
/// parameters and write to the output/in-place Span arguments.  No state is
/// cached, no Godot types are used.<br/><br/>
/// Rule 5.3: uses <c>System.Numerics.Vector&lt;float&gt;</c> hardware intrinsics
/// (AVX2 / SSE2 / NEON depending on the runtime) for the vectorised inner loops.
/// </remarks>
public static class SimdMath
{
    /// <summary>
    /// SIMD-accelerated: adds <paramref name="scalar"/> to every element of
    /// <paramref name="data"/> in-place.
    /// Equivalent to: <c>data[i] += scalar</c> for all i.
    /// </summary>
    /// <param name="data">The span of floats to modify in-place.</param>
    /// <param name="scalar">The value to add to each element.</param>
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

        // Scalar remainder for elements that don't fill a full SIMD register.
        for (; i < data.Length; i++)
        {
            data[i] += scalar;
        }
    }

    /// <summary>
    /// SIMD-accelerated: adds <c>b[i] * scalar</c> to <c>a[i]</c> for all i
    /// (fused multiply-add over Vector2 spans).
    /// Equivalent to: <c>a[i] += b[i] * scalar</c> for all i.
    /// </summary>
    /// <remarks>
    /// The Vector2 spans are reinterpreted as flat float spans so the SIMD
    /// register can process two components at a time without extra packing.
    /// </remarks>
    /// <param name="a">Destination span modified in-place (positions).</param>
    /// <param name="b">Source span (velocities).</param>
    /// <param name="scalar">Scale factor (delta time).</param>
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

        // Scalar remainder.
        for (; i < aFloat.Length; i++)
        {
            aFloat[i] += bFloat[i] * scalar;
        }
    }

    /// <summary>
    /// Rotates a span of velocity direction vectors by
    /// <paramref name="directionDegrees"/> degrees and scales them by
    /// <paramref name="speed"/>.  Used once per spawn call to bake the
    /// spawner's intended direction and speed into the velocity buffer.
    /// </summary>
    /// <param name="velocities">
    /// Unit direction vectors written by a <see cref="SpawnPattern2D"/>;
    /// modified in-place to become the final velocity vectors.
    /// </param>
    /// <param name="speed">The bullet speed (units/second).</param>
    /// <param name="directionDegrees">
    /// Additional rotational offset in degrees (0 = no extra rotation).
    /// </param>
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
            // Fast path: no rotation, just apply speed.
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
