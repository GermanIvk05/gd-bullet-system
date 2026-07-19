---
paths:
  - "**/*.cs"
  - "**/*.tscn"
---
# Bullet System Architecture Rules

These rules dictate the architectural standards for the gd-bullet-system project. You must adhere to these directives when generating, modifying, or reviewing code. They are based on the [Chickensoft Enjoyable Game Architecture](https://chickensoft.games/blog/game-architecture) methodology, adapted for a high-performance kinematic bullet system.

---

## 1. File Organization & Feature-Based Structure

* **Group by Feature**: Organize files (scripts, scenes, strategies) by feature (e.g., `Scripts/System/`, `Scripts/Strategies/Motion/`) rather than by type. Each folder should be self-contained.
* **Loose Coupling via Interfaces**: Features must remain isolated. When two features must interact, they do so via an interface or abstract Godot Resource. Never create a hard dependency between sibling features.
* **Shared Abstracts**: Place all shared abstract classes and interfaces in `Scripts/Strategies/` (e.g., `BulletMotion`, `SpawnPattern2D`). These are the seams between layers.
* **Consistent Scaffolding**: Every new strategy (motion or spawn) should follow the same pattern: inherit the abstract Resource, override `Execute(...)` with Span-based parameters, and apply `[GlobalClass]`.

---

## 2. Strict Abstraction Layers & Data Flow

Categorize every object in the codebase into exactly one of four layers:

```
Game Visuals (Visual Layer)
    ↓  strongly coupled downward only
Visual Game Logic Layer  (BulletSystem2D node, demo scenes)
    ↓
Pure Game Logic Layer    (BulletMotion, SpawnPattern2D strategies)
    ↓
Data Layer               (BulletMotion resources, config, SimdMath helpers)
```

* **Strict Downward Dependency**: An object in one layer may only be *strongly coupled* with objects in the layer *directly beneath it*. Never skip a layer. Never couple horizontally (two objects in the same layer).
* **Loose Upward Communication**: Information bubbles *upward* using reactive mechanisms — C# events or Godot signals — never via direct method calls on a higher layer.
* **Directed, Acyclic Graph**: When loose coupling is eliminated, the dependency graph of the project should be a DAG. Cycles indicate a design error.

---

## 3. Visual Components (Visual Layer)

Visual components are Godot `Node` subclass scripts attached to scene nodes. They are responsible **only** for what the player sees.

* **Stateless Visuals**: Visual components must be completely devoid of game logic and conditional branching. Treat them like a Flutter `StatelessWidget`.
* **Forward Inputs, Emit Signals**: When an input event occurs, the visual component's only job is to emit a Godot `[Signal]` or call a method on its underlying system node. It makes no decisions of its own.
* **No Game State Ownership**: A visual component must not own or duplicate state that is already tracked by `BulletSystem2D`. It only renders what it is told.
* **Cleanup in `_ExitTree`**: Always unsubscribe from C# events and Godot signals in `_ExitTree()` or `OnExitTree()` to prevent memory leaks and stale-handler errors.

---

## 4. State Management (Visual Game Logic Layer)

`BulletSystem2D` is the primary Visual Game Logic node. It owns and drives the bullet simulation.

* **Owns the Data Arrays**: `BulletSystem2D` owns all flat arrays (`_positions`, `_velocities`, `_lifetimes`). No other class may hold references to these arrays.
* **Delegates to Strategies**: The process loop delegates motion and spawn logic entirely to injected `BulletMotion` and `SpawnPattern2D` resources via their `Execute(Span<T>...)` signatures.
* **No Ad-Hoc Branching**: Do not add `if/else` chains for different bullet types inside the process loop. Every variation in behavior must be encapsulated in a new strategy resource.
* **Cleanup in `_ExitTree`**: Stop all processing and release array references in `_ExitTree()`.

---

## 5. Strategy Resources (Pure Game Logic Layer)

Strategy resources are plain C# classes inheriting from Godot `Resource`. They implement the domain rules of the bullet system.

* **Stateless Execution**: Strategy `Execute(...)` methods must be pure functions over their `Span<T>` inputs. They must not cache state between frames.
* **No Node Dependencies**: Strategy resources must not hold references to any Godot `Node`. They receive raw `Span<T>` buffers and return results.
* **SIMD-First Math**: Use `System.Numerics.Vector2` and `Vector<float>` hardware intrinsics for all core math loops. **Never** use `Godot.Vector2` inside `Execute(...)`.
* **Easily Testable**: Because strategy resources have no Godot node dependency, they can be unit-tested without instantiating any scene.

---

## 6. Rendering (Visual Layer)

* **Single Draw Call**: All bullets must be rendered via a single `MultiMeshInstance2D` draw call using `MultimeshSetBuffer`. Never use individual `Sprite2D` or `Node2D` instances per bullet.
* **`BulletRenderer2D` is the only renderer**: All rendering logic must live in `BulletRenderer2D`. `BulletSystem2D` passes position data to it; it does not render directly.
* **Buffer Writes are the Only Output**: `BulletRenderer2D` accepts a `ReadOnlySpan<float>` transform buffer and pushes it to the GPU. No other operations are permitted in the render path.

---

## 7. Critical Rules

> **These rules must be followed in every change. Violations will break the project.**

1. **Never edit `.tscn` or `.tres` files by hand** — use the Godot editor. These files contain UIDs and sub-resource references that are fragile.
2. **Never edit `.uid` or `.import` files**.
3. **All classes extending Godot types must be `partial`**.
4. **All `Resource` subclasses for the inspector must have `[GlobalClass]`**.
5. **DO NOT introduce intermediate classes** — Keep the architecture flat. If a bullet needs a new property (e.g., `Color`), add a new array directly to `BulletSystem2D` and pass it to the renderer.
6. **Use `System.Numerics.Vector2`** for all core math loops — NOT `Godot.Vector2` — to retain SIMD benefits.

---

## 8. Testing Standards

* **Test in Isolation**: Strategy resources and `SimdMath` helpers must each be testable completely independently, without instantiating any Godot node or scene.
* **Mirror Source Directory**: Store unit tests in a directory structure that mirrors the source. Append `Test` to each test file name (e.g., `Scripts/Strategies/Motion/LinearBulletMotion.cs` → `test/Strategies/Motion/LinearBulletMotionTest.cs`).
* **Never Deserialize Scenes in Unit Tests**: Never call `GD.Load<PackedScene>(...)` or `ResourceLoader.Load(...)` in a unit test.
* **Test Spans Directly**: Pass fabricated `Span<T>` buffers directly to strategy `Execute(...)` methods. Assert on the resulting buffer values.
