[assembly: System.Runtime.Versioning.TargetFramework(".NETStandard,Version=v2.0", FrameworkDisplayName=".NET Standard 2.0")]
namespace Riok.Mapperly.Abstractions
{
    public enum EnumMappingStrategy
    {
        ByValue = 0,
        ByName = 1,
        ByValueCheckDefined = 2,
    }
    public enum EnumNamingStrategy
    {
        MemberName = 0,
        CamelCase = 1,
        PascalCase = 2,
        SnakeCase = 3,
        UpperSnakeCase = 4,
        KebabCase = 5,
        UpperKebabCase = 6,
        ComponentModelDescriptionAttribute = 7,
        SerializationEnumMemberAttribute = 8,
    }
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class FormatProviderAttribute : System.Attribute
    {
        public FormatProviderAttribute() { }
        public bool Default { get; set; }
    }
    [System.Flags]
    public enum IgnoreObsoleteMembersStrategy
    {
        None = 0,
        Both = -1,
        Source = 1,
        Target = 2,
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapDerivedTypeAttribute : System.Attribute
    {
        public MapDerivedTypeAttribute(System.Type sourceType, System.Type targetType) { }
        public System.Type SourceType { get; }
        public System.Type TargetType { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapDerivedTypeAttribute<TSource, TTarget> : System.Attribute
    {
        public MapDerivedTypeAttribute() { }
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapEnumAttribute : System.Attribute
    {
        public MapEnumAttribute(Riok.Mapperly.Abstractions.EnumMappingStrategy strategy) { }
        public object? FallbackValue { get; set; }
        public bool IgnoreCase { get; set; }
        public Riok.Mapperly.Abstractions.EnumNamingStrategy NamingStrategy { get; set; }
        public Riok.Mapperly.Abstractions.EnumMappingStrategy Strategy { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapEnumValueAttribute : System.Attribute
    {
        public MapEnumValueAttribute(object source, object target) { }
        public object Source { get; }
        public object Target { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapNestedPropertiesAttribute : System.Attribute
    {
        public MapNestedPropertiesAttribute(string source) { }
        public MapNestedPropertiesAttribute(string[] source) { }
        public System.Collections.Generic.IReadOnlyCollection<string> Source { get; }
        public string SourceFullName { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapPropertyAttribute : System.Attribute
    {
        public MapPropertyAttribute(string source, string target) { }
        public MapPropertyAttribute(string[] source, string target) { }
        public MapPropertyAttribute(string source, string[] target) { }
        public MapPropertyAttribute(string[] source, string[] target) { }
        public string? FormatProvider { get; set; }
        public System.Collections.Generic.IReadOnlyCollection<string> Source { get; }
        public string SourceFullName { get; }
        public string? StringFormat { get; set; }
        public System.Collections.Generic.IReadOnlyCollection<string> Target { get; }
        public string TargetFullName { get; }
        public string? Use { get; set; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapPropertyFromSourceAttribute : System.Attribute
    {
        public MapPropertyFromSourceAttribute(string target) { }
        public MapPropertyFromSourceAttribute(string[] target) { }
        public string? FormatProvider { get; set; }
        public string? StringFormat { get; set; }
        public System.Collections.Generic.IReadOnlyCollection<string> Target { get; }
        public string TargetFullName { get; }
        public string? Use { get; set; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapValueAttribute : System.Attribute
    {
        public MapValueAttribute(string target) { }
        public MapValueAttribute(string[] target) { }
        public MapValueAttribute(string target, object? value) { }
        public MapValueAttribute(string[] target, object? value) { }
        public System.Collections.Generic.IReadOnlyCollection<string> Target { get; }
        public string TargetFullName { get; }
        public string? Use { get; set; }
        public object? Value { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Class)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public class MapperAttribute : System.Attribute
    {
        public MapperAttribute() { }
        public bool AllowNullPropertyAssignment { get; set; }
        public bool AutoUserMappings { get; set; }
        public Riok.Mapperly.Abstractions.MappingConversionType EnabledConversions { get; set; }
        public bool EnumMappingIgnoreCase { get; set; }
        public Riok.Mapperly.Abstractions.EnumMappingStrategy EnumMappingStrategy { get; set; }
        public Riok.Mapperly.Abstractions.EnumNamingStrategy EnumNamingStrategy { get; set; }
        public Riok.Mapperly.Abstractions.IgnoreObsoleteMembersStrategy IgnoreObsoleteMembersStrategy { get; set; }
        public Riok.Mapperly.Abstractions.MemberVisibility IncludedConstructors { get; set; }
        public Riok.Mapperly.Abstractions.MemberVisibility IncludedMembers { get; set; }
        public bool PreferParameterlessConstructors { get; set; }
        public Riok.Mapperly.Abstractions.PropertyNameMappingStrategy PropertyNameMappingStrategy { get; set; }
        public Riok.Mapperly.Abstractions.RequiredMappingStrategy RequiredEnumMappingStrategy { get; set; }
        public Riok.Mapperly.Abstractions.RequiredMappingStrategy RequiredMappingStrategy { get; set; }
        public bool ThrowOnMappingNullMismatch { get; set; }
        public bool ThrowOnPropertyMappingNullMismatch { get; set; }
        public bool UseDeepCloning { get; set; }
        public bool UseReferenceHandling { get; set; }
    }
    [System.AttributeUsage(System.AttributeTargets.Constructor)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapperConstructorAttribute : System.Attribute
    {
        public MapperConstructorAttribute() { }
    }
    [System.AttributeUsage(System.AttributeTargets.Assembly)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapperDefaultsAttribute : Riok.Mapperly.Abstractions.MapperAttribute
    {
        public MapperDefaultsAttribute() { }
    }
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapperIgnoreAttribute : System.Attribute
    {
        public MapperIgnoreAttribute() { }
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapperIgnoreObsoleteMembersAttribute : System.Attribute
    {
        public MapperIgnoreObsoleteMembersAttribute(Riok.Mapperly.Abstractions.IgnoreObsoleteMembersStrategy ignoreObsoleteStrategy = -1) { }
        public Riok.Mapperly.Abstractions.IgnoreObsoleteMembersStrategy IgnoreObsoleteStrategy { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapperIgnoreSourceAttribute : System.Attribute
    {
        public MapperIgnoreSourceAttribute(string source) { }
        public string Source { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapperIgnoreSourceValueAttribute : System.Attribute
    {
        public MapperIgnoreSourceValueAttribute(object source) { }
        public System.Enum? SourceValue { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapperIgnoreTargetAttribute : System.Attribute
    {
        public MapperIgnoreTargetAttribute(string target) { }
        public string Target { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapperIgnoreTargetValueAttribute : System.Attribute
    {
        public MapperIgnoreTargetValueAttribute(object target) { }
        public System.Enum? TargetValue { get; }
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MapperRequiredMappingAttribute : System.Attribute
    {
        public MapperRequiredMappingAttribute(Riok.Mapperly.Abstractions.RequiredMappingStrategy requiredMappingStrategy) { }
        public Riok.Mapperly.Abstractions.RequiredMappingStrategy RequiredMappingStrategy { get; }
    }
    [System.Flags]
    public enum MappingConversionType
    {
        None = 0,
        Constructor = 1,
        ImplicitCast = 2,
        ExplicitCast = 4,
        ParseMethod = 8,
        ToStringMethod = 16,
        StringToEnum = 32,
        EnumToString = 64,
        EnumToEnum = 128,
        DateTimeToDateOnly = 256,
        DateTimeToTimeOnly = 512,
        Queryable = 1024,
        Enumerable = 2048,
        Dictionary = 4096,
        Span = 8192,
        Memory = 16384,
        Tuple = 32768,
        EnumUnderlyingType = 65536,
        All = -1,
    }
    [System.AttributeUsage(System.AttributeTargets.Parameter)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class MappingTargetAttribute : System.Attribute
    {
        public MappingTargetAttribute() { }
    }
    [System.Flags]
    public enum MemberVisibility
    {
        AllAccessible = 31,
        All = 30,
        Accessible = 1,
        Public = 2,
        Internal = 4,
        Protected = 8,
        Private = 16,
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class ObjectFactoryAttribute : System.Attribute
    {
        public ObjectFactoryAttribute() { }
    }
    public enum PropertyNameMappingStrategy
    {
        CaseSensitive = 0,
        CaseInsensitive = 1,
    }
    [System.Flags]
    public enum RequiredMappingStrategy
    {
        None = 0,
        Both = -1,
        Source = 1,
        Target = 2,
    }
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class UseMapperAttribute : System.Attribute
    {
        public UseMapperAttribute() { }
    }
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class UseStaticMapperAttribute : System.Attribute
    {
        public UseStaticMapperAttribute(System.Type mapperType) { }
    }
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=true)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class UseStaticMapperAttribute<T> : System.Attribute
    {
        public UseStaticMapperAttribute() { }
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class UserMappingAttribute : System.Attribute
    {
        public UserMappingAttribute() { }
        public bool Default { get; set; }
        public bool Ignore { get; set; }
    }
}
namespace Riok.Mapperly.Abstractions.ReferenceHandling
{
    public interface IReferenceHandler
    {
        void SetReference<TSource, TTarget>(TSource source, TTarget target)
            where TSource :  notnull
            where TTarget :  notnull;
        bool TryGetReference<TSource, TTarget>(TSource source, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TTarget? target)
            where TSource :  notnull
            where TTarget :  notnull;
    }
    public sealed class PreserveReferenceHandler : Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler
    {
        public PreserveReferenceHandler() { }
        public void SetReference<TSource, TTarget>(TSource source, TTarget target)
            where TSource :  notnull
            where TTarget :  notnull { }
        public bool TryGetReference<TSource, TTarget>(TSource source, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TTarget? target)
            where TSource :  notnull
            where TTarget :  notnull { }
    }
    [System.AttributeUsage(System.AttributeTargets.Parameter)]
    [System.Diagnostics.Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
    public sealed class ReferenceHandlerAttribute : System.Attribute
    {
        public ReferenceHandlerAttribute() { }
    }
}