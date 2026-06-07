using System;
using Godot;

[GlobalClass]
public partial class CurveMovementConfig : MovementConfig
{
    [Export]
    public Curve SpeedCurve { get; set; }

    public override IMovementStrategy CreateStrategy() => new CurveMovementStrategy(SpeedCurve);
}

public class CurveMovementStrategy(Curve speedCurve) : IMovementStrategy
{
    private readonly Curve _speedCurve = speedCurve;

    public System.Numerics.Vector2 Calculate(
        System.Numerics.Vector2 position,
        float angle,
        float lifetime,
        float delta
    )
    {
        if (_speedCurve == null)
        {
            return System.Numerics.Vector2.Zero;
        }
        float currentSpeed = _speedCurve.SampleBaked(lifetime);
        return new System.Numerics.Vector2(MathF.Cos(angle), MathF.Sin(angle))
            * currentSpeed
            * delta;
    }
}
