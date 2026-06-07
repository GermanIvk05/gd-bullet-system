using System;
using System.Numerics;
using Godot;

public abstract partial class BulletController : Node2D
{
    [Export]
    public BulletConfig Config;

    public abstract void SpawnPattern(
        BulletPattern2D pattern,
        Godot.Vector2 position,
        float rotation
    );

    public override void _ExitTree()
    {
        Cleanup();
    }

    protected abstract void Cleanup();

    /// <summary>
    /// Builds a <see cref="Matrix3x2"/> world matrix from the Godot position and rotation.
    /// </summary>
    protected static Matrix3x2 BuildWorldMatrix(Godot.Vector2 position, float rotation)
    {
        return Matrix3x2.CreateRotation(rotation)
            * Matrix3x2.CreateTranslation(position.X, position.Y);
    }
}
