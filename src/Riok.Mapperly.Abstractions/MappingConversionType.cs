namespace Riok.Mapperly.Abstractions;

/// <summary>
/// A <see cref="MappingConversionType"/> represents a type of conversion
/// how one type can be converted into another.
/// </summary>
[Flags]
public enum MappingConversionType
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,

    /// <summary>
    /// Use the constructor of the target type,
    /// which accepts the source type as a single parameter.
    /// </summary>
    Constructor = 1 << 0,

    /// <summary>
    /// An implicit cast from the source type to the target type.
    /// </summary>
    ImplicitCast = 1 << 1,

    /// <summary>
    /// An explicit cast from the source type to the target type.
    /// </summary>
    ExplicitCast = 1 << 2,

    /// <summary>
    /// If the source type is a <see cref="string"/>,
    /// uses a static visible method named `Parse` on the target type
    /// with a return type equal to the target type and a string as single parameter.
    /// </summary>
    ParseMethod = 1 << 3,

    /// <summary>
    /// If the target type is a <see cref="string"/>,
    /// uses the `ToString` method on the source type.
    /// </summary>
    ToStringMethod = 1 << 4,

    /// <summary>
    /// If the target is an <see cref="Enum"/>
    /// and the source is a <see cref="string"/>,
    /// parses the string to match the name of an enum member.
    /// </summary>
    StringToEnum = 1 << 5,

    /// <summary>
    /// If the source is an <see cref="Enum"/>
    /// and the target is a <see cref="string"/>,
    /// uses the name of the enum member to convert it to a string.
    /// </summary>
    EnumToString = 1 << 6,

    /// <summary>
    /// If the source is an <see cref="Enum"/>
    /// and the target is another <see cref="Enum"/>,
    /// map it according to the <see cref="EnumMappingStrategy"/>.
    /// </summary>
    EnumToEnum = 1 << 7,

    /// <summary>
    /// If the source is a <see cref="DateTime"/>
    /// and the target is a <c>DateOnly</c>
    /// uses the `FromDateTime` method on the target type with the source as single parameter.
    /// </summary>
    DateTimeToDateOnly = 1 << 8,

    /// <summary>
    /// If the source is a <see cref="DateTime"/>
    /// and the target is a <c>TimeOnly</c>
    /// uses the `FromDateTime` method on the target type with the source as single parameter.
    /// </summary>
    DateTimeToTimeOnly = 1 << 9,

    /// <summary>
    /// If the source and the target are a <see cref="IQueryable{T}"/>.
    /// Only uses object initializers and inlines the mapping code.
    /// </summary>
    Queryable = 1 << 10,

    /// <summary>
    /// If the source and the target are an <see cref="IEnumerable{T}"/>
    /// Maps each element individually.
    /// </summary>
    Enumerable = 1 << 11,

    /// <summary>
    /// If the source and targets are <see cref="IDictionary{TKey,TValue}"/>
    /// or <see cref="IReadOnlyDictionary{TKey,TValue}"/>.
    /// Maps each <see cref="KeyValuePair{TKey,TValue}"/> individually.
    /// </summary>
    Dictionary = 1 << 12,

    /// <summary>
    /// If the source or target is a Span&lt;T&gt; or ReadOnlySpan&lt;T&gt;
    /// Maps each element individually.
    /// </summary>
    Span = 1 << 13,

    /// <summary>
    /// If the source or target is a Memory&lt;T&gt; or ReadOnlyMemory&lt;T&gt;
    /// Maps each element individually.
    /// </summary>
    Memory = 1 << 14,

    /// <summary>
    /// If the target is a <see cref="ValueTuple{T, U}"/> or tuple expression (A: 10, B: 12).
    /// Supports positional and named mapping.
    /// Only uses <see cref="ValueTuple{T, U}"/> in <see cref="IQueryable{T}"/>.
    /// </summary>
    Tuple = 1 << 15,

    /// <summary>
    /// Allow using the underlying type of enum to map from or to an enum type.
    /// </summary>
    EnumUnderlyingType = 1 << 16,

    /// <summary>
    /// If the source type contains a `ToTarget` method other than `ToString`, use it
    /// </summary>
    ToTargetMethod = 1 << 17,

    /// <summary>
    /// Combination of <see cref="ToStringMethod"/> and <see cref="ToTargetMethod"/>
    /// </summary>
    AllToTargetMethods = ToStringMethod | ToTargetMethod,

    /// <summary>
    /// If the source type contains a static `ToTarget` method
    /// or the target type contains a static methods
    /// `Create(TSource)`,
    /// `CreateFrom(TSource)`
    /// `From(TSource)`
    /// `FromTSource(TSource)`
    /// or similar methods with <langword>params</langword> keyword, use it.
    /// The exception is <see cref="DateTime"/> conversions,
    /// which are enabled by separate options (<seealso cref="DateTimeToTimeOnly"/>, <seealso cref="DateTimeToDateOnly"/>).
    /// </summary>
    StaticConvertMethods = 1 << 18,

    /// <summary>
    /// Combination of <see cref="DateTimeToDateOnly"/>, <see cref="DateTimeToTimeOnly"/> and <see cref="StaticConvertMethods"/>
    /// </summary>
    AllStaticMethods = DateTimeToDateOnly | DateTimeToTimeOnly | StaticConvertMethods,

    /// <summary>
    /// Enables all supported conversions.
    /// </summary>
    All = ~None,
}
