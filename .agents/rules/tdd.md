---
paths:
  - "**/*.cs"
  - "**/Tests/**/*.cs"
  - "**/*Test.cs"
  - "**/*Tests.cs"
---
# Test-Driven Development (TDD) Rules for AI Agents

These rules dictate mandatory directives and workflows for applying Test-Driven Development (TDD) when adding features, fixing bugs, or refactoring code in Godot C# projects. AI agents MUST follow this methodology to ensure code quality, isolated testability, maintainability, and regression prevention.

---

## 1. The Core TDD Cycle (Red – Green – Refactor)

AI agents must strictly execute changes following the classic 3-phase TDD workflow:

```
    ┌─────────────────────────┐
    │ 1. RED: Write Test First│  <-- Write failing test specifying behavior
    └────────────┬────────────┘
                 │ (Run test & confirm expected failure)
                 ▼
    ┌─────────────────────────┐
    │ 2. GREEN: Minimal Code  │  <-- Implement minimal production code to pass
    └────────────┬────────────┘
                 │ (Run test & confirm green pass)
                 ▼
    ┌─────────────────────────┐
    │ 3. REFACTOR: Clean Up   │  <-- Refactor code & tests for structure/performance
    └─────────────────────────┘
```

1. **Phase 1: RED (Write a Failing Test First)**
   - Before writing or modifying any implementation code, write a unit test that clearly defines the expected behavior, contract, or bug fix.
   - Run `dotnet test` (or the project test runner) to **verify that the test fails** for the expected reason (e.g., missing method, wrong return value, failing assertion).
   - *Never skip verifying the test failure!* A test that passes before code is written is invalid or redundant.

2. **Phase 2: GREEN (Make It Pass with Minimal Code)**
   - Write the simplest, most direct code necessary to make the failing test pass.
   - Do not write speculative code or unrequested features at this stage.
   - Run `dotnet test` again and confirm that all tests pass cleanly.

3. **Phase 3: REFACTOR (Improve Code Quality Safely)**
   - Clean up code formatting, eliminate duplication, improve naming, optimize math/performance, and align with architectural rules.
   - Re-run all tests to guarantee no regression occurred during refactoring.

---

## 2. Test Architecture & Isolation Principles

In Godot C# projects (aligned with the 4-Layer Architecture):

* **Pure Logic First**: Focus unit tests primarily on the **Pure Game Logic Layer** (domain strategies, state machines, math helpers, algorithms, calculators) and **Data Layer** (DTOs, configuration validators).
* **Zero Godot Scene Dependencies in Unit Tests**:
  - Never load `.tscn` scene files (`GD.Load<PackedScene>()`) or `.tres` resource files inside pure unit tests.
  - Never instantiate native scene tree nodes requiring engine loops unless executing dedicated integration test suites.
  - Pass raw data primitives, fabricate `Span<T>` / `ReadOnlySpan<T>` memory buffers directly, or inject interface mocks.
* **Test Directory Hierarchy**:
  - Mirror the source code structure inside the test project (e.g., `BulletSystem.Tests`).
  - Name test classes by appending `Test` or `Tests` to the target class name (e.g., `LinearBulletMotion` → `LinearBulletMotionTest`).

---

## 3. Test Structure & Naming Conventions

* **AAA Pattern (Arrange, Act, Assert)**: Every test method must clearly follow AAA structure:
  ```csharp
  [Fact]
  public void Execute_SingleBullet_UpdatesPositionAlongVelocity()
  {
      // 1. Arrange: Setup test objects, parameters, and expected values
      var strategy = new LinearBulletMotion();
      Span<Vector2> positions = [Vector2.Zero];
      Span<Vector2> velocities = [new Vector2(100f, 0f)];
      ReadOnlySpan<float> lifetimes = [0.5f];
      float delta = 0.016f;

      // 2. Act: Call the method under test
      strategy.Execute(positions, velocities, lifetimes, delta);

      // 3. Assert: Verify post-conditions
      Assert.Equal(1.6f, positions[0].X, precision: 4);
      Assert.Equal(0f, positions[0].Y, precision: 4);
  }
  ```
* **Expressive Test Names**: Method names must state the method under test, scenario, and expected outcome using the format `[MethodUnderTest]_[Scenario]_[ExpectedOutcome]`:
  - Good: `Execute_ZeroDelta_PositionsRemainUnchanged`
  - Good: `CalculateDamage_CriticalHit_ReturnsDoubledDamage`
  - Bad: `Test1`, `TestExecute`

---

## 4. Bug Fixing with TDD

When fixing a bug or regression reported in the codebase:

1. **Reproduce via Test First**: Write a unit test that isolates and reproduces the reported bug.
2. **Observe Failure**: Run `dotnet test` to confirm it fails, proving the existence of the defect.
3. **Fix the Defect**: Apply the fix in implementation code until the test passes.
4. **Verify Full Suite**: Run the full test suite (`dotnet test`) to ensure no existing functionality was broken.

---

## 5. Mocking & Dependency Injection

* **Mock Interfaces, Not Concrete Nodes**:
  - Use interfaces (e.g., Chickensoft `GodotNodeInterfaces` or custom contracts) to abstract Godot engine nodes when testing node controllers.
  - Inject dependencies via constructors or tree-based DI (`AutoInject`) so logic can be tested with mock/stub implementations.
* **Minimal Fake Implementations**:
  - Prefer simple stub classes or lightweight fakes over heavy mocking frameworks where possible to keep test runs fast and compile-time safe.

---

## 6. Execution & Verification Commands

AI agents MUST run test execution commands to empirically verify every step of the TDD cycle:

```bash
dotnet test
```

* **Never declare success without running tests**: Editing files alone is not completing a task. You MUST run `dotnet test` and obtain a 100% clean test execution report before declaring victory.
* **Inspect Failing Output Immediately**: If a test fails unexpectedly during refactoring or green phases, inspect line numbers and stack traces to fix the root cause immediately.

---

## 7. Mandatory Rules for AI Agents

> **Violations of these rules break TDD discipline.**

1. **NEVER write production implementation code without a pre-existing failing test** (unless generating initial boilerplate stubs required for compilation).
2. **NEVER comment out or delete failing tests to make the build pass**.
3. **NEVER add assertion swallowers or dummy fallbacks** (`try { ... } catch {}`) just to hide a failing test assertion.
4. **ALWAYS verify test execution with `dotnet test` after every iteration**.
5. **Keep unit tests fast & deterministic**: Unit tests must execute in milliseconds without external side effects (file I/O, network calls, frame delays).
