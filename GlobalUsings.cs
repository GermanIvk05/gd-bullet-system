// =============================================================================
// Project-wide global type aliases — resolve System.Numerics vs Godot ambiguity.
//
// Both `using Godot;` (Godot.Vector2, Godot.Vector3, …) and
// `using System.Numerics;` (System.Numerics.Vector2, Matrix3x2, …) are needed
// in many files.  Without explicit aliases the compiler cannot resolve bare
// `Vector2` or `Matrix3x2`.
//
// Rule 7.6 / AGENTS.md: "Use System.Numerics.Vector2 for core math loops —
// NOT Godot.Vector2 — to retain SIMD benefits."
//
// These global aliases make that intent the unambiguous project default.
// Any file that legitimately needs Godot.Vector2 must qualify it explicitly
// as `Godot.Vector2`.
// =============================================================================

global using Vector2   = System.Numerics.Vector2;
global using Matrix3x2 = System.Numerics.Matrix3x2;
