# Agent Configurations & Rules (`.agents`)

This directory contains the operational rules, architectural guidelines, and methodology standards for AI agents working in Godot C# codebases.

---

## Directory Overview

```
.agents/
├── AGENTS.md                   ← (This file) Overview and index of agent configurations
└── rules/                      ← Modular rule definitions evaluated against matching paths
    ├── godot-architecture.md   ← Architectural standards (4-layer architecture, coupling, performance)
    └── tdd.md                  ← Test-Driven Development workflow (Red-Green-Refactor, test isolation)
```

---

## Agent Rule Files (`.agents/rules/`)

All rule files in `.agents/rules/` include YAML frontmatter specifying file path glob patterns. When working on matching files, AI agents must strictly follow the corresponding rule file:

| Rule File | Target Paths | Description |
| --------- | ------------ | ----------- |
| [godot-architecture.md](file:///.agents/rules/godot-architecture.md) | `**/*.cs`, `**/*.tscn` | Enforces 4-layer architecture (Visual, Visual Game Logic, Pure Game Logic, Data), feature-based file organization, passive view separation, performance batching, and critical Godot C# standards. |
| [tdd.md](file:///.agents/rules/tdd.md) | `**/*.cs`, `**/Tests/**/*.cs`, `**/*Test.cs`, `**/*Tests.cs` | Enforces Red–Green–Refactor TDD cycle, test isolation without Godot scene tree dependencies, AAA test structure, expressive test naming, bug reproduction, and mandatory `dotnet test` empirical verification. |

---

## Maintenance & Extension Guidelines

1. **Keep Rules Modular**: Place specific rules into dedicated `.md` files under `.agents/rules/` rather than bloating root config files.
2. **Specify Path Scopes**: Always include YAML frontmatter `paths` at the top of new rule files so agents know when the rules apply.
3. **Generic & Portable**: Ensure rules remain generic across Godot C# projects so they can be reused across repositories.
