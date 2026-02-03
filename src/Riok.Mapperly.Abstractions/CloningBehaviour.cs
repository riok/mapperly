namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Specifies whether and how to copy objects of the same type and complex types like collections and spans.
/// </summary>
public enum CloningBehaviour
{
    /// <summary>
    /// Default behaviour, the original instance will be returned
    /// </summary>
    None,

    /// <summary>
    /// Always deep copy objects.
    /// Eg. when the type <c>Person[]</c> should be mapped to the same type <c>Person[]</c>,
    /// the array and each person is cloned.
    /// </summary>
    DeepCloning,

    /// <summary>
    /// Always shallow copy objects.
    /// Eg. when the type <c>Person</c> should be mapped to the same type <c>Person</c>,
    /// a new instance will be returned with the same values for all properties.
    /// References will be kept.
    /// </summary>
    ShallowCloning,
}
