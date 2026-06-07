using System;
using Godot;

[GlobalClass]
public partial class LinearMovementConfig : MovementConfig
{
    [Export]
    public float Speed { get; set; } = 100f;

    public override IMovementStrategy CreateStrategy() => new LinearMovementStrategy(Speed);
}

public class LinearMovementStrategy(float speed) : IMovementStrategy
{
    private readonly float _speed = speed;

    public System.Numerics.Vector2 Calculate(System.Numerics.Vector2 position, float angle, float lifetime, float delta)
    {
        return new System.Numerics.Vector2(MathF.Cos(angle), MathF.Sin(angle)) * _speed * delta;
    }
}
