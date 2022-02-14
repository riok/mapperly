using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper;

[Mapper(ImplementationName = "MyClassMapper")]
public abstract class TestMapper
{
    public abstract int DirectInt(int value);

    public abstract long ImplicitCastInt(int value);

    public abstract int ExplicitCastInt(uint value);

    public abstract int? CastIntNullable(int value);

    public abstract Guid ParseableGuid(string id);

    public abstract int ParseableInt(string value);

    public abstract DateTime DirectDateTime(DateTime dateTime);

    [MapperIgnore(nameof(TestObjectDto.IgnoredStringValue))]
    [MapProperty(nameof(TestObject.RenamedStringValue), nameof(TestObjectDto.RenamedStringValue2))]
    public abstract TestObjectDto MapToDto(TestObject testObject);

    [MapperIgnore(nameof(TestObject.IgnoredStringValue))]
    public abstract TestObject MapFromDto(TestObjectDto dto);

    [MapEnum(EnumMappingStrategy.ByName)]
    public abstract TestEnumDtoByName MapToEnumDtoByName(TestEnum v);

    public abstract void UpdateDto(TestObject source, TestObjectDto target);
}
