---
paths:
  - "**/*.cs"
  - "**/*.tscn"
---
# Godot C# Architecture Rules

These rules dictate the architectural standards for Godot C# projects. You must adhere to these directives when generating, modifying, or reviewing code. They are based on the [Chickensoft Enjoyable Game Architecture](https://chickensoft.games/blog/game-architecture) methodology, enforcing clean separation of concerns, testability, maintainability, and high performance.

---

## 1. File Organization & Feature-Based Structure

* **Group by Feature**: Organize files (scripts, scenes, resources, assets) by feature or domain (e.g., `Scripts/Player/`, `Scripts/Combat/`, `Scripts/Inventory/`) rather than strictly by artifact type. Each feature directory should be cohesive and self-contained.
* **Loose Coupling via Interfaces**: Features must remain isolated. When two features must interact, do so via C# interfaces, abstract Godot Resources, event channels, or signals. Never create direct hard dependencies between sibling features.
* **Shared Abstracts & Contracts**: Place shared interfaces, domain abstractions, and contract classes in dedicated core/shared directories or feature roots.
* **Consistent Scaffolding**: Follow consistent patterns across features (e.g., state machines, strategy resources, view nodes, and domain logic processors).

---

## 2. Strict Abstraction Layers & Data Flow

Categorize every object in the codebase into exactly one of four layers:

```
Visual Layer            (Passive Node/Node2D/Node3D views, UI components)
    ↓  strongly coupled downward only
Visual Game Logic Layer (Controllers, Scene Coordinators, Godot-dependent logic)
    ↓
Pure Game Logic Layer   (Plain C# classes, State Machines, Domain Models, Math calculations)
    ↓
Data Layer              (Godot Resources, Static Services, Configurations, DTOs)
```

* **Strict Downward Coupling**: An object in a higher layer may only be *strongly coupled* with objects in the layer *directly beneath it*. Never skip a layer. Avoid horizontal coupling between sibling components in the same layer.
* **Loose Upward Communication**: Information bubbles *upward* using reactive mechanisms — C# events, observables, or Godot signals — never via direct method calls on a higher layer.
* **Directed, Acyclic Graph (DAG)**: The overall architecture forms a clean DAG. Cyclic dependencies indicate an architectural boundary violation.

---

## 3. Visual Components (Visual Layer)

Visual components are Godot `Node` subclass scripts attached to scene nodes (e.g., `Node2D`, `Node3D`, `Control`). They are responsible **only** for presentation and raw player input capture.

* **Passive & Stateless Visuals**: Visual components must be devoid of complex game logic and business rules. Treat them like passive view components.
* **Forward Inputs, Emit Signals**: When player input or UI interactions occur, the visual component's job is to emit a Godot `[Signal]` or call a method on its underlying controller. It makes no state decisions of its own.
* **No Authority Over Game State**: Visual components must not own authority over domain data or duplicate state tracked by controllers/domain logic. They only render state provided to them.
* **Lifecycle Cleanup in `_ExitTree`**: Always unsubscribe from C# events, signals, and observers in `_ExitTree()` or `OnExitTree()` to prevent memory leaks and stale event handles.

---

## 4. State Management & Controllers (Visual Game Logic Layer)

Controllers, state coordinators, and system nodes sit in the Visual Game Logic layer. They bridge Godot scene lifecycles with pure game domain logic.

* **Owns Runtime Coordination**: Controllers manage entity lifecycles, tree integration, and scene transitions.
* **Delegates to Pure Logic**: Delegate business rules, math, combat calculations, and state transitions to Pure Game Logic classes or Resource strategies.
* **No Ad-Hoc Branching**: Avoid bloated `if/else` or `switch` chains for variations in behavior. Encapsulate strategies and state variations into state machines or resource-driven strategies.
* **Lifecycle Management in `_ExitTree`**: Stop processing loops, dispose resources, and unbind listeners cleanly in `_ExitTree()`.

---

## 5. Domain Logic & Strategies (Pure Game Logic Layer)

Pure Game Logic classes are plain C# classes (or Godot `Resource` subclasses where editor authoring is needed). They implement domain rules, algorithms, and state logic.

* **Decoupled from Scene Tree**: Pure Game Logic classes must not hold direct references to Godot `Node` scene objects. They receive pure data inputs (or `Span<T>` buffers) and return computed results.
* **Stateless or Enclosed Execution**: Strategy algorithms and calculations should operate deterministically over their inputs without side effects on engine infrastructure.
* **Hardware Intrinsics & Math**: Use `System.Numerics` vectors and memory abstractions (`Span<T>`, `ReadOnlySpan<T>`) for performance-critical calculation loops.
* **100% Testable**: Because pure game logic has no Godot scene tree dependency, it can be unit-tested without instantiating scene nodes or launching engine loops.

---

## 6. Data Layer & Configuration (Data Layer)

The Data Layer contains static configurations, Godot `Resource` definitions, data transfer objects, and static helper services.

* **Immutable & Declarative Data**: Store game settings, balance values, and strategy blueprints in Godot Resources (`[GlobalClass]`).
* **No Side Effects**: Data objects hold configuration and pure definitions; they do not manipulate scene nodes or trigger side effects.
* **Flat & Access-Optimized**: Store performance-sensitive data in flat structures or contiguous arrays for efficient batch processing.

---

## 7. Performance & Batching Guidelines

* **Batch Draw Calls**: For mass entities (bullets, particles, tiles, foliage), use batched rendering primitives (such as `MultiMeshInstance2D` / `MultiMeshInstance3D` or `RenderingServer` buffer updates) rather than spawning individual scene nodes.
* **Memory Management**: Minimize heap allocations in per-frame process loops (`_Process`, `_PhysicsProcess`). Prefer struct data, `Span<T>`, or pooled memory allocations.
* **Vector Math SIMD**: Prefer `System.Numerics.Vector2` / `Vector3` in bulk math calculations to benefit from hardware SIMD acceleration.

---

## 8. Critical Godot C# Rules

> **These rules must be followed in every change. Violations will break Godot C# projects.**

1. **Never edit `.tscn` or `.tres` files by hand** — use the Godot editor. These files contain UIDs and sub-resource references that are fragile.
2. **Never edit `.uid` or `.import` files**.
3. **All classes extending Godot types must be `partial`** — required for Godot C# source generators.
4. **All `Resource` subclasses for the inspector must have `[GlobalClass]`**.
5. **Keep Architecture Flat & Avoid Unnecessary Abstraction Layers** — Do not introduce intermediate wrapper classes, proxy pools, or redundant abstractions unless they add clear value.
6. **Explicit Cleanup**: Always unbind event subscriptions and clean up resources in `_ExitTree()`.

---

## 9. Testing Standards

* **Test in Isolation**: Domain logic, state machines, and pure math algorithms must be testable independently without instantiating Godot nodes or scenes.
* **Mirror Source Directory**: Store unit tests in a directory structure that mirrors the source directory (e.g., `Scripts/Combat/DamageCalculator.cs` → `test/Combat/DamageCalculatorTest.cs`).
* **Never Deserialize Scenes in Unit Tests**: Avoid `GD.Load<PackedScene>(...)` or `ResourceLoader.Load(...)` in pure unit tests.
* **Use Mockable Interfaces**: Use mockable node interfaces (e.g., Chickensoft `GodotNodeInterfaces`) or dependency injection (`AutoInject`) when testing node controllers.
