using System;
using System.Numerics;
using Godot;

/// <summary>
/// Pure Game Logic Layer — spawn pattern that distributes bullets evenly across
/// an arc of <see cref="SpreadAngle"/> degrees, centred on the spawner's facing
/// direction and offset from it by <see cref="Radius"/> world units.
/// </summary>
/// <remarks>
/// When <see cref="SpawnPattern2D.BulletCount"/> is 1 the single bullet fires
/// exactly along the spawner's forward axis.<br/><br/>
/// Rule 5.1: stateless — Execute is a pure function over its Span inputs.<br/>
/// Rule 5.2: no Godot Node references; uses only <c>System.Numerics</c> types.<br/>
/// The velocities written are unit direction vectors.  Speed is applied by the
/// caller via <c>SimdMath.ApplySpeedAndRotation</c>.
/// </remarks>
[GlobalClass]
public partial class ArcSpawnPattern2D : SpawnPattern2D
{
    /// <summary>The spawn offset radius from the spawner origin (world units).</summary>
    [Export]
    public float Radius { get; set; } = 50f;

    /// <summary>The total angular spread of the arc in degrees (e.g. 90 = quarter circle).</summary>
    [Export]
    public float SpreadAngle { get; set; } = 90f;

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

        // Derive the spawner's facing direction from the world-matrix rotation column.
        float targetAngle = MathF.Atan2(worldMatrix.M12, worldMatrix.M11);

        if (count == 1)
        {
            var dir = new Vector2(MathF.Cos(targetAngle), MathF.Sin(targetAngle));
            positions[0] = Vector2.Transform(dir * Radius, worldMatrix);
            velocities[0] = dir; // unit vector — caller applies speed
            return 1;
        }

        float spreadRad = SpreadAngle * (MathF.PI / 180f);
        float halfSpread = spreadRad * 0.5f;
        float angleStep = spreadRad / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = targetAngle - halfSpread + i * angleStep;
            var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

            positions[i] = Vector2.Transform(dir * Radius, worldMatrix);
            velocities[i] = dir; // unit vector — caller applies speed
        }
        return count;
    }
}
