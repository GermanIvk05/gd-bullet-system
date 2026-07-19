using System;
using System.Numerics;

/// <summary>
/// Pure Game Logic Layer — spawn pattern that distributes bullets uniformly in
/// a full circle of radius <see cref="Radius"/> centred on the spawner.
/// </summary>
/// <remarks>
/// Rule 5.1: stateless — Execute is a pure function over its Span inputs.<br/>
/// Rule 5.2: no Godot Node references; uses only <c>System.Numerics</c> types.<br/>
/// The velocities written are unit direction vectors pointing outward from the
/// centre.  Speed is applied by the caller via <c>SimdMath.ApplySpeedAndRotation</c>.
/// </remarks>
[Godot.GlobalClass]
public partial class CircleSpawnPattern2D : SpawnPattern2D
{
    /// <summary>The radius of the spawn circle in world units.</summary>
    [Godot.Export]
    public float Radius { get; set; } = 50f;

    /// <inheritdoc/>
    public override int Execute(
        Span<Vector2> positions,
        Span<Vector2> velocities,
        Matrix3x2 worldMatrix
    )
    {
        int count = positions.Length;
        if (count == 0)
            return 0;

        float angleStep = MathF.Tau / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

            positions[i] = Vector2.Transform(dir * Radius, worldMatrix);
            velocities[i] = dir; // unit vector — caller applies speed
        }
        return count;
    }
}
