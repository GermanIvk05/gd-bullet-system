# gd-bullet-system

A high-performance bullet system for **Godot 4 (C#)** designed for bullet hell-style games. It separates bullet logic, physics, and rendering into distinct layers, and supports thousands of bullets via batched `MultiMeshInstance2D` rendering and direct `PhysicsServer2D` integration.

## Architecture

The system consists of:

**`BulletController2D`** — manages bullets as raw physics bodies via `PhysicsServer2D` with no scene tree overhead. Renders using `BulletView` (`MultiMeshInstance2D`) with a single batched `RenderingServer` buffer upload per frame.

```
BulletController2D           ← physics server + multimesh rendering

BulletBatch                  ← manages a group of bullets (server path)

BulletConfig                 ← exported resource: shape, damage, movement, despawn, collision
BulletPattern2D (abstract)   ← defines zero-allocation spawn transform buffers
MovementConfig (abstract)    ← defines per-frame movement behaviour
DespawnCondition (abstract)  ← defines when a bullet should be removed
```

## Features

- **Batched rendering** via `RenderingServer.MultimeshSetBuffer` — one native call per frame regardless of bullet count
- **PhysicsServer2D** kinematic bodies for collision without scene tree nodes
- **Strategy pattern** for movement — swap between `LinearMovementConfig`, `OscillateMovementConfig`, or write your own
- **Composable despawn conditions** — combine multiple conditions per bullet type (e.g. lifetime + out-of-bounds)
- **Pluggable spawn patterns** — `CirclePattern2D`, `ArcPattern2D`, or implement `BulletPattern2D` for custom layouts
- **Data-driven configuration** — everything is a Godot `Resource`, editable in the inspector

## Getting Started

### Requirements

- Godot 4.x with C# (.NET) support

### Setup

1. Add a `BulletController2D` node to your scene.
2. Create a `BulletConfig` resource and assign it to the controller's `Config` export:
   - Set a `Shape2D` for collision
   - Assign a `MovementConfig` (e.g. `LinearMovementConfig`)
   - Add one or more `DespawnCondition` resources (e.g. `LifetimeDespawnCondition`)
   - Set collision layer and mask as needed
3. Assign a `BulletView` (`MultiMeshInstance2D`) to the `View` export.
4. Create a `BulletPattern2D` resource (e.g. `CirclePattern2D` or `ArcPattern2D`).
5. Call `SpawnPattern` to fire:

```csharp
BulletController2D.SpawnPattern(pattern, GlobalPosition, GlobalRotation);
```

## Extending

### Custom movement

```csharp
[GlobalClass]
public partial class HomingMovementConfig : MovementConfig
{
    [Export] public float Speed { get; set; } = 200f;

    public override IMovementStrategy CreateStrategy() => new HomingMovementStrategy(Speed);
}

public class HomingMovementStrategy : IMovementStrategy
{
    private float _speed;
    public HomingMovementStrategy(float speed) => _speed = speed;

    public Vector2 Calculate(Vector2 position, float angle, float lifetime, float delta)
    {
        // custom logic here
        return Vector2.FromAngle(angle) * _speed * delta;
    }
}
```

### Custom despawn condition

```csharp
[GlobalClass]
public partial class OutOfBoundsDespawnCondition : DespawnCondition
{
    [Export] public Rect2 Bounds { get; set; }

    public override bool ShouldDespawn(Vector2 position, float angle, float lifetime)
        => !Bounds.HasPoint(position);
}
```

### Custom pattern

```csharp
using System;
using System.Numerics;

[Godot.GlobalClass]
public partial class SpiralPattern2D : BulletPattern2D
{
    [Godot.Export] public float Radius { get; set; } = 10f;

    public override int FillBuffer(Span<Matrix3x2> buffer, Matrix3x2 worldMatrix)
    {
        int count = Math.Min(BulletCount, buffer.Length);
        if (count == 0) return 0;

        float step = MathF.Tau / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * step;
            var localPos = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Radius * (i / (float)count));
            var worldPos = Vector2.Transform(localPos, worldMatrix);
            buffer[i] = Matrix3x2.CreateRotation(angle) * Matrix3x2.CreateTranslation(worldPos);
        }
        return count;
    }
}
```

## License

MIT
