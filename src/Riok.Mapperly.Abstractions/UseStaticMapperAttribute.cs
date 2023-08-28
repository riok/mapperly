namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Considers all static mapping methods provided by the type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class UseStaticMapperAttribute : Attribute
{
    /// <summary>
    /// Considers all static mapping methods provided by the <paramref name="mapperType"/>.
    /// </summary>
    /// <param name="mapperType">The type of which mapping methods will be included.</param>
    public UseStaticMapperAttribute(Type mapperType) { }
}

/// <summary>
/// Considers all static mapping methods provided by the generic type.
/// </summary>
/// <typeparam name="T">The type of which mapping methods will be included.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class UseStaticMapperAttribute<T> : Attribute { }
