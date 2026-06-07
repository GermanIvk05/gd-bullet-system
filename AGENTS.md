# AGENTS.md — gd-bullet-system

> Operational guide for AI agents working in this codebase.
> This file is the single source of truth for project conventions, architecture, and constraints.

---

## Project Identity

| Field          | Value                                      |
| -------------- | ------------------------------------------ |
| **Name**       | gd-bullet-system                           |
| **Purpose**    | High-performance bullet system for bullet hell-style games |
| **Engine**     | Godot 4.6 (Forward Plus renderer)          |
| **Language**   | C# (.NET 8.0)                              |
| **SDK**        | `Godot.NET.Sdk/4.6.3`                      |
| **License**    | MIT                                        |
| **Namespace**  | `BulletControllerGDScript` (root namespace in `.csproj`) |

---

## Tech Stack & Frameworks

- **Godot 4.6** — game engine; scenes (`.tscn`), resources, and the editor are the primary authoring environment.
- **C# / .NET 8.0** — all gameplay logic is written in C#. No GDScript files exist despite the project name.
- **Godot.NET.Sdk 4.6.3** — the MSBuild SDK that bridges C# ↔ Godot.
- **PhysicsServer2D** — direct server API used for kinematic bullet bodies (no scene-tree nodes in production path).
- **RenderingServer / MultiMeshInstance2D** — batched rendering via `MultimeshSetBuffer` for thousands of bullets in a single draw call.
- **Godot Resource system** — all configuration (`BulletConfig`, `MovementConfig`, `DespawnCondition`, `BulletPattern2D`) is data-driven via exported `Resource` subclasses.
- **System.Numerics** — `Matrix3x2` is used in the pattern system for high-performance, SIMD-friendly 2D affine transforms.

---

## Architecture Overview

```
Scripts/
├── Core/                        ← Runtime controllers, batching, rendering
│   ├── BulletController2D.cs    ← Concrete spawner: PhysicsServer2D + MultiMesh
│   ├── BulletBatch.cs           ← Server-side bullet group (physics bodies + transforms)
│   └── BulletView.cs            ← MultiMeshInstance2D: uploads transform buffer each frame
│
├── Configs/                     ← Data-driven configuration resources
│   ├── BulletConfig.cs          ← Top-level config: Damage, Shape, Movement, DespawnConditions, collision layers
│   ├── Movement/
│   │   ├── MovementConfig.cs    ← Abstract resource → creates IMovementStrategy
│   │   ├── LinearMovementConfig.cs
│   │   ├── CurveMovementConfig.cs
│   │   └── OscillateMovementConfig.cs
│   └── Spawn/
│       ├── DespawnCondition.cs  ← Abstract resource: ShouldDespawn()
│       └── LifetimeDespawnCondition.cs
│
├── Patterns/                    ← Spawn pattern definitions
│   ├── BulletPattern2D.cs       ← Abstract resource: FillBuffer(Span<Matrix3x2>)
│   ├── CirclePattern2D.cs       ← Full-circle pattern
│   ├── ArcPattern2D.cs          ← Arc/fan pattern
│   ├── CompositePattern2D.cs    ← Composite pattern applying transformations/delays
│   └── CompositePatternEntry2D.cs← Resource entry defining child transformations


Assets/
└── bullet_00..04.png            ← Animated bullet sprite frames

main.tscn                       ← Main scene: wires both controllers + UI button
Main.cs                          ← Entry point: fires SpawnPattern on button press
```

The system runs on the `BulletController2D` production path, which manages bullets as raw physics bodies via `PhysicsServer2D` and renders them with `BulletView` (`MultiMeshInstance2D`).



- **Data-Driven Composition** — `BulletConfig` composes a `MovementConfig` + array of `DespawnCondition` resources. Everything is editable in the Godot inspector.
- **Batch Processing** — `BulletBatch` processes all bullets in a tight loop each frame, then uploads a single float buffer to `RenderingServer`.
- **Zero-Allocation Patterns** — `BulletPattern2D.FillBuffer(Span<Matrix3x2>, Matrix3x2)` fills a caller-provided buffer with world-space transforms, avoiding heap allocations on the spawn path. Small patterns (≤128 bullets) use `stackalloc`.

---

## File Conventions

### Naming

| Element              | Convention             | Example                         |
| -------------------- | ---------------------- | ------------------------------- |
| C# files             | PascalCase             | `BulletController2D.cs`         |
| Classes              | PascalCase             | `BulletController2D`            |
| Interfaces           | `I` prefix + PascalCase| `IMovementStrategy`             |
| Godot resources      | PascalCase             | `BulletConfig`, `CirclePattern2D` |
| Scenes               | PascalCase `.tscn`     | `Bullet.tscn`                   |
| Assets               | snake_case             | `bullet_00.png`                 |
| Directories          | PascalCase             | `Scripts/Core/`, `Configs/Movement/` |

### Code Style

- **Charset**: UTF-8 (enforced via `.editorconfig`).
- **Line endings**: LF (enforced via `.gitattributes`: `* text=auto eol=lf`).
- **Indentation**: Tabs in C# files (Godot C# default).
- **`[GlobalClass]`**: Applied to all `Resource` subclasses intended for the Godot inspector.
- **`[Export]`**: Used on all properties meant to be editable in the Godot editor.
- **`partial class`**: Required on all classes extending Godot types (Godot source generator requirement).

### File Organization

- **One class per file** — each `.cs` file contains exactly one primary type.
- **Directory = concern** — `Core/` for runtime, `Configs/` for data resources, `Patterns/` for spawn patterns.
- **`.uid` files** — auto-generated by Godot 4.6; tracked in git but never hand-edited.
- **`.import` files** — Godot asset import metadata; listed in `.gitignore`.

---

## Extension Points

When adding new functionality, follow these patterns:

### New Movement Type

1. Create `Scripts/Configs/Movement/YourMovementConfig.cs`:
   - Extend `MovementConfig` (abstract `Resource`)
   - Add `[GlobalClass]` attribute
   - Implement `CreateStrategy()` → return a new `IMovementStrategy`
2. Create the corresponding strategy class (can be in the same file or separate):
   - Implement `IMovementStrategy.Calculate(Vector2 position, float angle, float lifetime, float delta) → Vector2`

### New Despawn Condition

1. Create `Scripts/Configs/Spawn/YourDespawnCondition.cs`:
   - Extend `DespawnCondition` (abstract `Resource`)
   - Add `[GlobalClass]` attribute
   - Override `ShouldDespawn(Vector2 position, float angle, float lifetime) → bool`

### New Spawn Pattern

1. Create `Scripts/Patterns/YourPattern2D.cs`:
   - Extend `BulletPattern2D` (abstract `Resource`)
   - Add `[GlobalClass]` attribute
   - Override `FillBuffer(Span<Matrix3x2> buffer, Matrix3x2 worldMatrix) → int`
   - Use `System.Numerics.Vector2` and `Matrix3x2` — not `Godot.Vector2`
   - Each entry in `buffer` is a world-space 2D affine transform: position in `(M31, M32)`, rotation via `Matrix3x2.CreateRotation()`
   - Return the number of bullets actually written (may be ≤ `buffer.Length`)
   - The inherited `[Export] BulletCount` property controls the buffer size callers allocate


---

## Critical Rules

> **These rules must be followed in every change. Violations will break the project.**

1. **Never edit `.tscn` or `.tres` files by hand** — use the Godot editor. These files contain UIDs and sub-resource references that are fragile. Scene/resource edits should be described in instructions, not applied as code changes.

2. **Never edit `.uid` files** — these are auto-generated by Godot 4.6 and must not be manually modified.

3. **Never edit `.import` files** — these are generated by Godot's import system.

4. **All classes extending Godot types must be `partial`** — the Godot source generator requires this. Missing `partial` will cause build failures.

5. **All `Resource` subclasses for the inspector must have `[GlobalClass]`** — otherwise they won't appear in the Godot editor's resource picker.

6. **Use `PhysicsServer2D` and `RenderingServer` APIs** — do not add scene-tree nodes in `BulletController2D` or `BulletBatch`. The entire point is zero scene-tree overhead.


8. **Movement strategies must be stateless or per-bullet** — `IMovementStrategy.Calculate()` receives all state as parameters. Do not store mutable global state in strategy instances.

9. **Keep `BulletConfig` composable** — movement and despawn are separate, swappable resources. Do not collapse them into a single monolithic config.

10. **Line endings must be LF** — enforced by `.gitattributes`. Do not commit files with CRLF.

---

## Build & Run

```bash
# Build the C# project (requires .NET 8 SDK + Godot 4.6)
dotnet build "Bullet Controller (GDScript).csproj"

# Run from Godot editor
# Open project.godot in Godot 4.6, press F5 (main scene: main.tscn)
```

- **Main scene**: `main.tscn` — contains the `BulletController2D` node, a `BulletView`, and a UI button that fires `SpawnPattern`.
- **No unit test framework is configured** — testing is done via the Godot editor's play mode.
- **vsync is disabled** (`window/vsync/vsync_mode=0`) and FPS printing is enabled (`settings/stdout/print_fps=true`) for performance profiling.

---

## Key Interfaces

### `IMovementStrategy`

```csharp
Vector2 Calculate(Vector2 position, float angle, float lifetime, float delta)
```

Returns the **displacement vector** for a single frame. Called once per bullet per frame.

### `BulletController2D`

```csharp
void SpawnPattern(BulletPattern2D pattern, Vector2 position, float rotation)
```

Spawns a batch of bullets according to the given pattern at the specified origin. Builds a `Matrix3x2` world matrix internally and delegates to `FillBuffer`.

### `BulletPattern2D` (abstract)

```csharp
abstract int FillBuffer(Span<Matrix3x2> buffer, Matrix3x2 worldMatrix)
```

Fills the provided buffer with world-space `Matrix3x2` bullet transforms. Returns the number of bullets written. Each matrix encodes rotation and translation for one bullet.

### `DespawnCondition` (abstract)

```csharp
abstract bool ShouldDespawn(Vector2 position, float angle, float lifetime)
```

Returns `true` when a bullet should be removed. Multiple conditions can be composed on a single `BulletConfig`.

---

## Directory Quick Reference

| Path                         | Contents                                |
| ---------------------------- | --------------------------------------- |
| `/`                          | Project root: Godot config, solution, main scene |
| `Scripts/Core/`              | Controllers, batching, rendering        |
| `Scripts/Configs/`           | `BulletConfig` + movement/despawn resources |
| `Scripts/Configs/Movement/`  | `MovementConfig` hierarchy + strategies |
| `Scripts/Configs/Spawn/`     | `DespawnCondition` hierarchy            |
| `Scripts/Patterns/`          | `BulletPattern2D` hierarchy            |
| `Assets/`                    | Sprite textures (bullet animation frames) |
| `.godot/`                    | Godot editor cache (gitignored)         |
