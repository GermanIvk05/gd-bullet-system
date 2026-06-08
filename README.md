# gd-bullet-system

A high-performance bullet system for **Godot 4 (C#)** designed for bullet hell-style games. It features an extreme KISS (Keep It Simple, Stupid) architecture focused purely on **kinematic simulation and visuals**. By completely dropping traditional Godot physics integration, it achieves massive scale and performance via pure C# Array-of-Structs (AoS)/Struct-of-Arrays (SoA) layout and SIMD acceleration.

## Architecture

The system consists of a completely flattened data pipeline:

**`BulletSystem2D`** — The core engine node. It directly allocates and owns the primitive C# arrays (`Vector2[] _positions`, `Vector2[] _velocities`, `float[] _lifetimes`). It processes the simulation in tight loops and pushes the position data directly to a `BulletRenderer2D`.

```
BulletSystem2D               ← the core Godot Node; owns memory arrays and _PhysicsProcess loop
BulletRenderer2D             ← MultiMeshInstance2D: uploads Vector2 span directly to rendering server

BulletMotion (abstract)      ← Godot Resource: calculates positions using Span data and SIMD intrinsics
SpawnPattern2D (abstract)    ← Godot Resource: writes spawn locations and velocities into Spans
```

## Features

- **Pure Mathematical Kinematics** — Zero Godot physics overhead (no `Rid` allocations, no `PhysicsServer2D` syncing)
- **SIMD Acceleration** — Leverages native `System.Numerics.Vector<T>` and AVX2 hardware intrinsics via `SimdMath.cs`
- **Batched rendering** via `RenderingServer.MultimeshSetBuffer` — one native call per frame regardless of bullet count
- **Zero-Allocation Strategies** — `BulletMotion` and `SpawnPattern2D` implementations accept raw `Span<T>` for maximum performance
- **Data-driven configuration** — Swappable movement algorithms (`LinearBulletMotion`, `OscillateBulletMotion`) configured in the inspector

## Getting Started

### Requirements

- Godot 4.x with C# (.NET) support
- .NET 8.0 SDK

### Setup

1. Add a `BulletSystem2D` node to your scene.
2. In the inspector, assign your configuration directly to the `BulletSystem2D`:
   - Assign a `Movement` resource (e.g. `LinearBulletMotion`)
   - Set the `MaxLifetime`
3. Assign a `BulletRenderer2D` (`MultiMeshInstance2D` subclass) to the `View` export.
4. Assign a `SpawnPattern2D` resource to the `Pattern` export.
5. Call `SpawnPattern` to fire:

```csharp
BulletSystem2D.SpawnPattern(GlobalPosition, GlobalRotation);
```

## Extending

### Custom movement

Create a new Godot Resource inheriting from `BulletMotion`. You receive direct access to the memory spans:

```csharp
using System;
using Godot;

[GlobalClass]
public partial class CustomHomingMotion : BulletMotion
{
    [Export] public float TurnSpeed { get; set; } = 5f;

    public override void Execute(Span<System.Numerics.Vector2> positions, ReadOnlySpan<System.Numerics.Vector2> velocities, ReadOnlySpan<float> lifetimes, float delta)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            // Perform custom homing math directly on the vectors here
            positions[i] += velocities[i] * Speed * delta;
        }
    }
}
```

### Custom pattern

Create a new pattern inheriting from `SpawnPattern2D`:

```csharp
using System;
using System.Numerics;

[Godot.GlobalClass]
public partial class CustomSpiralPattern : SpawnPattern2D
{
    [Godot.Export] public float Radius { get; set; } = 10f;

    public override int Execute(Span<Vector2> positions, Span<Vector2> velocities, Matrix3x2 worldMatrix)
    {
        int count = positions.Length;
        if (count == 0) return 0;

        float step = MathF.Tau / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * step;
            var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            
            positions[i] = Vector2.Transform(dir * Radius, worldMatrix);
            velocities[i] = dir;
        }
        return count;
    }
}
```

## License

MIT
