namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Defines the strategy to use when cloning a <see cref="System.Collections.Generic.Stack{T}"/>.
/// </summary>
public enum StackCloningStrategy
{
    /// <summary>
    /// Preserves the order of the elements in the stack.
    /// This is the default behavior.
    /// </summary>
    PreserveOrder,

    /// <summary>
    /// Reverses the order of the elements in the stack.
    /// This corresponds to the behavior of <c>new Stack&lt;T&gt;(IEnumerable&lt;T&gt;)</c>.
    /// </summary>
    ReverseOrder,
}
