# Project Tech Stack

This document defines the technology stack and tooling guidelines for AI agents working on this project.

## Core Technologies

* **Game Engine**: Godot 4.x (Mono C# Edition)
* **Programming Language**: C# 12 / .NET 8.0
* **Build System**: SCons (Godot internal) / MSBuild (`dotnet build`)

## Architecture & Framework Suite

We rely heavily on the **Chickensoft** ecosystem to enforce clean, testable, and decoupled code patterns in Godot C#:

* **Dependency Injection (`Chickensoft.AutoInject`)**
  * Implements tree-based ancestor injection. Nodes provide dependencies down the tree using `this.Provide()`, and child nodes resolve them via `[Dependency]` and `this.DependOn<T>()`.
  * Mixin-based lookup activated via `[Meta(typeof(IAutoNode))]` and overriding `_Notification` with `this.Notify(what)`.

* **Metadata & Mixins (`Chickensoft.Introspection`)**
  * A source-generator-based compile-time alternative to dynamic reflection. Essential for AutoInject mixins and mockable setups.

* **Mockable Node Interfaces (`Chickensoft.GodotNodeInterfaces`)**
  * Auto-generated interfaces for native Godot nodes (e.g., `ISprite2D`, `INode2D`). Allows us to unit test Visual and Visual Game Logic layer scripts without instantiating actual engine objects or scene files.

## Project Abstraction Layers

We strictly adhere to a 4-layer architecture defined in [.agents/rules/bullet-architecture.md](file:///.agents/rules/bullet-architecture.md):

1. **Visual Layer** (Passive `Node2D`, `MultiMeshInstance2D`, etc. - no logic, reacts to inputs/outputs).
2. **Visual Game Logic Layer** (Demo scenes, `BulletSystem2D` node - coordinates visual elements, forwards events).
3. **Pure Game Logic Layer** (Plain C# classes like strategy resources - implements bullet rules, motion calculations, and spawn domain logic).
4. **Data Layer** (Static services, configurations like `BulletMotion` and `SpawnPattern2D` resources carrying shared state).
