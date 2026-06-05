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
- **Godot Resource system** — all configuration (`BulletConfig`, `MovementConfig`, `DespawnCondition`, `BulletPattern`) is data-driven via exported `Resource` subclasses.

---

## Architecture Overview

```
Scripts/
├── Core/                        ← Runtime controllers, batching, rendering
│   ├── BulletController.cs      ← Abstract base (Node2D): SpawnPattern()
│   ├── ServerBulletController.cs← Production path: PhysicsServer2D + MultiMesh
│   ├── NodeBulletController.cs  ← Debug/editor path: scene-tree nodes
│   ├── BulletBatch.cs           ← Server-side bullet group (physics bodies + transforms)
│   ├── NodeBulletBatch.cs       ← Node-side bullet group (BulletNode instances)
│   ├── BulletView.cs            ← MultiMeshInstance2D: uploads transform buffer each frame
│   └── BulletNode.cs            ← Single bullet as a Node2D (debug path only)
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
│   ├── BulletPattern.cs         ← Abstract resource: GetSpawnData()
│   ├── CirclePattern.cs
│   └── ArcPattern.cs

Scenes/
└── Bullet.tscn                  ← PackedScene for NodeBulletController (debug path)

Assets/
└── bullet_00..04.png            ← Animated bullet sprite frames

main.tscn                       ← Main scene: wires both controllers + UI button
Main.cs                          ← Entry point: fires SpawnPattern on button press
```

### Dual-Controller Design

The system provides **two interchangeable controllers** behind the shared abstract `BulletController`:

| Controller                 | Path        | Bullets Managed As          | Rendering                |
| -------------------------- | ----------- | --------------------------- | ------------------------ |
| `ServerBulletController`   | Production  | Raw `PhysicsServer2D` bodies| `BulletView` (MultiMesh) |
| `NodeBulletController`     | Debug/Editor| `BulletNode` (Node2D) instances | Godot scene tree       |

Both accept the same `BulletConfig`, `BulletPattern`, and movement/despawn strategies.

### Key Design Patterns

- **Strategy** — `MovementConfig.CreateStrategy()` returns an `IMovementStrategy` used per-frame for bullet movement. Implementations: `LinearMovementStrategy`, `CurveMovementStrategy`, `OscillateMovementStrategy`.
- **Template Method** — `BulletController` defines the abstract `SpawnPattern()` contract; subclasses implement the actual spawning.
- **Data-Driven Composition** — `BulletConfig` composes a `MovementConfig` + array of `DespawnCondition` resources. Everything is editable in the Godot inspector.
- **Batch Processing** — `BulletBatch` processes all bullets in a tight loop each frame, then uploads a single float buffer to `RenderingServer`.

---

## File Conventions

### Naming

| Element              | Convention             | Example                         |
| -------------------- | ---------------------- | ------------------------------- |
| C# files             | PascalCase             | `BulletController.cs`           |
| Classes              | PascalCase             | `ServerBulletController`        |
| Interfaces           | `I` prefix + PascalCase| `IMovementStrategy`             |
| Godot resources      | PascalCase             | `BulletConfig`, `CirclePattern` |
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

1. Create `Scripts/Patterns/YourPattern.cs`:
   - Extend `BulletPattern` (abstract `Resource`)
   - Add `[GlobalClass]` attribute
   - Override `GetSpawnData(float targetAngle) → SpawnData[]`
   - `SpawnData` is a struct with `Position` (Vector2) and `Angle` (float)

---

## Critical Rules

> **These rules must be followed in every change. Violations will break the project.**

1. **Never edit `.tscn` or `.tres` files by hand** — use the Godot editor. These files contain UIDs and sub-resource references that are fragile. Scene/resource edits should be described in instructions, not applied as code changes.

2. **Never edit `.uid` files** — these are auto-generated by Godot 4.6 and must not be manually modified.

3. **Never edit `.import` files** — these are generated by Godot's import system.

4. **All classes extending Godot types must be `partial`** — the Godot source generator requires this. Missing `partial` will cause build failures.

5. **All `Resource` subclasses for the inspector must have `[GlobalClass]`** — otherwise they won't appear in the Godot editor's resource picker.

6. **Use `PhysicsServer2D` and `RenderingServer` APIs in the server path** — do not add scene-tree nodes in `ServerBulletController` or `BulletBatch`. The entire point of the server path is zero scene-tree overhead.

7. **Preserve the dual-controller symmetry** — any feature added to `ServerBulletController` should have a corresponding implementation in `NodeBulletController` (and vice versa), maintaining the shared `BulletController` interface.

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

- **Main scene**: `main.tscn` — contains both controllers, a `BulletView`, and a UI button that fires `SpawnPattern`.
- **No unit test framework is configured** — testing is done via the Godot editor's play mode.
- **vsync is disabled** (`window/vsync/vsync_mode=0`) and FPS printing is enabled (`settings/stdout/print_fps=true`) for performance profiling.

---

## Key Interfaces

### `IMovementStrategy`

```csharp
Vector2 Calculate(Vector2 position, float angle, float lifetime, float delta)
```

Returns the **displacement vector** for a single frame. Called once per bullet per frame.

### `BulletController` (abstract)

```csharp
abstract void SpawnPattern(BulletPattern pattern, Vector2 position, float rotation)
```

Spawns a batch of bullets according to the given pattern at the specified origin.

### `BulletPattern` (abstract)

```csharp
abstract SpawnData[] GetSpawnData(float targetAngle = 0f)
```

Returns an array of spawn positions and angles relative to the pattern origin.

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
| `Scripts/Patterns/`          | `BulletPattern` hierarchy               |
| `Scenes/`                    | Reusable `.tscn` scenes (Bullet node)   |
| `Assets/`                    | Sprite textures (bullet animation frames) |
| `.godot/`                    | Godot editor cache (gitignored)         |
