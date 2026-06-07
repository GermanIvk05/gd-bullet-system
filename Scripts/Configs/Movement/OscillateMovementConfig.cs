using System;
using Godot;

[GlobalClass]
public partial class OscillateMovementConfig : MovementConfig
{
    [Export]
    public float ForwardSpeed { get; set; } = 100f;

    [Export]
    public float OscillateSpeed { get; set; } = 2f;

    [Export]
    public float Amplitude { get; set; } = 50f;

    public override IMovementStrategy CreateStrategy() =>
        new OscillateMovementStrategy(ForwardSpeed, OscillateSpeed, Amplitude);
}

public class OscillateMovementStrategy(float forwardSpeed, float oscillateSpeed, float amplitude)
    : IMovementStrategy
{
    private float _forwardSpeed = forwardSpeed;
    private float _oscillateSpeed = oscillateSpeed;
    private float _amplitude = amplitude;

    public System.Numerics.Vector2 Calculate(System.Numerics.Vector2 position, float angle, float lifetime, float delta)
    {
        var forward = new System.Numerics.Vector2(MathF.Cos(angle), MathF.Sin(angle));
        var perpendicular = new System.Numerics.Vector2(-forward.Y, forward.X);

        float previousSin = MathF.Sin((lifetime - delta) * _oscillateSpeed) * _amplitude;
        float currentSin = MathF.Sin(lifetime * _oscillateSpeed) * _amplitude;
        return forward * _forwardSpeed * delta + perpendicular * (currentSin - previousSin);
    }
}
