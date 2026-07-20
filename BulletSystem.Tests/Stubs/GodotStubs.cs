// =============================================================================
// Minimal Godot stub types — used ONLY by the test project.
//
// These stubs satisfy the compiler when compiling Pure Game Logic layer sources
// that reference Godot attributes ([GlobalClass], [Export]) and abstract Godot
// types (Resource).  They carry no runtime behaviour.
//
// Rule 8: never deserialise scenes or instantiate Godot nodes in unit tests.
// =============================================================================

using System;

// ReSharper disable once CheckNamespace
namespace Godot
{
    // -------------------------------------------------------------------------
    // Attribute stubs
    // -------------------------------------------------------------------------

    /// <summary>Stub for Godot.GlobalClassAttribute.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GlobalClassAttribute : Attribute { }

    /// <summary>Stub for Godot.ExportAttribute.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ExportAttribute : Attribute
    {
        public ExportAttribute() { }
        public ExportAttribute(PropertyHint hint, string hintString = "") { }
    }

    /// <summary>Stub for Godot.PropertyHint.</summary>
    public enum PropertyHint { None = 0, Range = 1 }

    // -------------------------------------------------------------------------
    // Type stubs
    // -------------------------------------------------------------------------

    /// <summary>
    /// Stub for Godot.Resource — the base type for all strategy resources.
    /// The test project compiles these classes without Godot SDK, so this stub
    /// prevents linker errors.  No Godot lifecycle methods are called in tests.
    /// </summary>
    public class Resource { }

    /// <summary>
    /// Stub for Godot.Curve — referenced by CurveBulletMotion.
    /// Tests pass <c>null</c> for SpeedCurve, exercising the fallback path.
    /// </summary>
    public class Curve
    {
        public float SampleBaked(float t) => t; // identity — not used in tests
    }
}

// All source files in the test project include `using Godot;`, so [GlobalClass]
// and [Export] resolve to the Godot-namespace stubs above without any top-level
// alias being needed.
