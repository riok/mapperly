using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public partial class BaseMapper
    {
        public virtual partial long IntToLong(int value);
    }

    [Mapper(EnabledConversions = MappingConversionType.All)]
    public partial class DerivedMapper : BaseMapper
    {
        public override partial long IntToLong(int value);
    }

    [Mapper]
    public partial class DerivedMapper2 : BaseMapper
    {
        public sealed override partial long IntToLong(int value);
    }
}
