# AGENTS.md — gd-bullet-system

> Operational guide for AI agents working in this codebase.
> This file is the single source of truth for project conventions, architecture, and constraints.

---

## Project Identity

| Field          | Value                                      |
| -------------- | ------------------------------------------ |
| **Name**       | gd-bullet-system                           |
| **Purpose**    | High-performance kinematic/visual bullet system |
| **Engine**     | Godot 4.6 (Forward Plus renderer)          |
| **Language**   | C# (.NET 8.0)                              |
| **SDK**        | `Godot.NET.Sdk/4.6.3`                      |
| **License**    | MIT                                        |
| **Namespace**  | `BulletControllerGDScript` (root namespace in `.csproj`) |

---

## Tech Stack & Frameworks

- **Godot 4.6** — game engine; scenes (`.tscn`), resources, and the editor are the primary authoring environment.
- **C# / .NET 8.0** — all gameplay logic is written in C#.
- **No Godot Physics** — The system is a pure mathematical kinematic simulation. `PhysicsServer2D` is NOT used to maximize performance.
- **RenderingServer / MultiMeshInstance2D** — batched rendering via `MultimeshSetBuffer` for thousands of bullets in a single draw call.
- **Godot Resource system** — all strategy behaviors (`BulletMotion`, `SpawnPattern2D`) are injected as Godot Resources.
- **System.Numerics** — Vector math uses `System.Numerics.Vector2` natively for automatic SIMD acceleration. `SimdMath.cs` uses `Vector<float>` hardware intrinsics.

---

## Architecture Overview

```
Scripts/
├── System/
│   ├── BulletSystem2D.cs        ← Core Node: owns Vector2 arrays, runs process loop
│   ├── BulletRenderer2D.cs      ← MultiMeshInstance2D helper
│   └── SimdMath.cs              ← AVX2 intrinsic helper functions
│
├── Strategies/
│   ├── Motion/
│   │   ├── BulletMotion.cs      ← Abstract resource: Execute(Spans...)
│   │   ├── LinearBulletMotion.cs
│   │   ├── CurveBulletMotion.cs
│   │   └── OscillateBulletMotion.cs
│   └── Spawning/
│       ├── SpawnPattern2D.cs    ← Abstract resource: Execute(Spans...)
│       ├── CircleSpawnPattern2D.cs
│       └── ArcSpawnPattern2D.cs
```

The system operates on an extreme KISS (Keep It Simple, Stupid) philosophy. Data is stored directly as flat arrays (`_positions`, `_velocities`, `_lifetimes`) inside `BulletSystem2D`. There are no intermediate simulators, pools, or data slice structs. The logic is defined by Godot Resource strategies that accept raw `Span<T>` buffers.

---

## File Conventions

### Naming

| Element              | Convention             | Example                         |
| -------------------- | ---------------------- | ------------------------------- |
| C# files             | PascalCase             | `BulletSystem2D.cs`             |
| Classes              | PascalCase             | `BulletSystem2D`                |
| Godot resources      | PascalCase             | `LinearBulletMotion`            |
| Scenes               | PascalCase `.tscn`     | `main.tscn`                     |

### Code Style

- **Charset**: UTF-8.
- **Line endings**: LF (`* text=auto eol=lf`).
- **`[GlobalClass]`**: Applied to all `Resource` subclasses intended for the Godot inspector.
- **`[Export]`**: Used on properties meant to be editable in Godot.
- **Primary Constructors**: Prefer C# 12 primary constructors for standard C# classes (though Godot Nodes/Resources must have parameterless constructors).

---

## Extension Points

### New Motion Strategy
Create a file in `Scripts/Strategies/Motion/` extending `BulletMotion`.
Override:
`public override void Execute(Span<System.Numerics.Vector2> positions, ReadOnlySpan<System.Numerics.Vector2> velocities, ReadOnlySpan<float> lifetimes, float delta)`

### New Spawn Pattern
Create a file in `Scripts/Strategies/Spawning/` extending `SpawnPattern2D`.
Override:
`public override int Execute(Span<System.Numerics.Vector2> positions, Span<System.Numerics.Vector2> velocities, System.Numerics.Matrix3x2 worldMatrix)`

---

## Critical Rules

> **These rules must be followed in every change. Violations will break the project.**

1. **Never edit `.tscn` or `.tres` files by hand** — use the Godot editor. These files contain UIDs and sub-resource references that are fragile. Scene/resource edits should be described in instructions, not applied as code changes.
2. **Never edit `.uid` or `.import` files**.
3. **All classes extending Godot types must be `partial`**.
4. **All `Resource` subclasses for the inspector must have `[GlobalClass]`**.
5. **DO NOT introduce intermediate classes** — Keep the architecture flat. If a bullet needs a new property (e.g. `Color`), add a new array directly to `BulletSystem2D` and pass it to the renderer.
6. **Use `System.Numerics.Vector2`** for core math loops, NOT `Godot.Vector2`, to retain SIMD benefits.

---

## Build & Run

```bash
dotnet build "Bullet Controller (GDScript).csproj"
```
Run `project.godot` in Godot 4.6.
