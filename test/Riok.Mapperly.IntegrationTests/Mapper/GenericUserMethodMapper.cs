using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public partial class GenericUserMethodMapper
    {
        public partial DocumentDto MapDocument(Document source);

        private Optional<TTarget> MapOptional<TSource, TTarget>(Optional<TSource> source)
            where TSource : notnull
            where TTarget : notnull => source.HasValue ? Optional.Of(Map<TSource, TTarget>(source.Value)) : Optional.Empty<TTarget>();

        private partial TTarget Map<TSource, TTarget>(TSource source)
            where TSource : notnull
            where TTarget : notnull;

        private partial UserDto MapUser(User source);
    }
}
