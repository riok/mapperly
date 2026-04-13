using System.Diagnostics.CodeAnalysis;

namespace Riok.Mapperly.IntegrationTests.Models
{
    public static class Optional
    {
        public static Optional<T> Of<T>(T value)
            where T : notnull => new(value);

        public static Optional<T> Empty<T>()
            where T : notnull => new(default);
    }

    public class Optional<T>
        where T : notnull
    {
        public Optional(T? value)
        {
            HasValue = value is not null;
            Value = value;
        }

#if NET5_0_OR_GREATER
        [MemberNotNullWhen(true, nameof(Value))]
#endif
        public bool HasValue { get; }

        public T? Value { get; }
    }
}
