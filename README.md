# gd-bullet-system

A high-performance bullet system for **Godot 4 (C#)** designed for bullet hell-style games. It separates bullet logic, physics, and rendering into distinct layers, and supports thousands of bullets via batched `MultiMeshInstance2D` rendering and direct `PhysicsServer2D` integration.

## Architecture

The system consists of:

**`ServerBulletController`** — manages bullets as raw physics bodies via `PhysicsServer2D` with no scene tree overhead. Renders using `BulletView` (`MultiMeshInstance2D`) with a single batched `RenderingServer` buffer upload per frame.

```
BulletController (abstract)
└── ServerBulletController   ← physics server + multimesh rendering

BulletBatch                  ← manages a group of bullets (server path)

BulletConfig                 ← exported resource: shape, damage, movement, despawn, collision
BulletPattern (abstract)     ← defines spawn positions and angles
MovementConfig (abstract)    ← defines per-frame movement behaviour
DespawnCondition (abstract)  ← defines when a bullet should be removed
```

## Features

- **Batched rendering** via `RenderingServer.MultimeshSetBuffer` — one native call per frame regardless of bullet count
- **PhysicsServer2D** kinematic bodies for collision without scene tree nodes
- **Strategy pattern** for movement — swap between `LinearMovementConfig`, `OscillateMovementConfig`, or write your own
- **Composable despawn conditions** — combine multiple conditions per bullet type (e.g. lifetime + out-of-bounds)
- **Pluggable spawn patterns** — `CirclePattern`, `ArcPattern`, or implement `BulletPattern` for custom layouts
- **Data-driven configuration** — everything is a Godot `Resource`, editable in the inspector

## Getting Started

### Requirements

- Godot 4.x with C# (.NET) support

### Setup

1. Add a `ServerBulletController` node to your scene.
2. Create a `BulletConfig` resource and assign it to the controller's `Config` export:
   - Set a `Shape2D` for collision
   - Assign a `MovementConfig` (e.g. `LinearMovementConfig`)
   - Add one or more `DespawnCondition` resources (e.g. `LifetimeDespawnCondition`)
   - Set collision layer and mask as needed
3. Assign a `BulletView` (`MultiMeshInstance2D`) to the `View` export.
4. Create a `BulletPattern` resource (e.g. `CirclePattern` or `ArcPattern`).
5. Call `SpawnPattern` to fire:

```csharp
BulletController.SpawnPattern(pattern, GlobalPosition, GlobalRotation);
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
[GlobalClass]
public partial class SpiralPattern : BulletPattern
{
    [Export] public int BulletCount { get; set; } = 12;

    public override SpawnData[] GetSpawnData(float targetAngle = 0f)
    {
        var spawns = new SpawnData[BulletCount];
        float step = Mathf.Tau / BulletCount;
        for (int i = 0; i < BulletCount; i++)
        {
            float angle = targetAngle + i * step;
            spawns[i] = new SpawnData { Position = Vector2.Zero, Angle = angle };
        }
        return spawns;
    }
}
```

## License

MIT
