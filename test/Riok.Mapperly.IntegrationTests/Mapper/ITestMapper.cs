using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper;

[Mapper(ImplementationName = "MyInterfaceMapper")]
public interface ITestMapper
{
    int DirectInt(int value);

    long ImplicitCastInt(int value);

    int ExplicitCastInt(uint value);

    int? CastIntNullable(int value);

    Guid ParseableGuid(string id);

    int ParseableInt(string value);

    DateTime DirectDateTime(DateTime dateTime);

    [MapperIgnore(nameof(TestObjectDto.IgnoredStringValue))]
    [MapProperty(nameof(TestObject.RenamedStringValue), nameof(TestObjectDto.RenamedStringValue2))]
    TestObjectDto MapToDto(TestObject testObject);

    [MapperIgnore(nameof(TestObject.IgnoredStringValue))]
    TestObject MapFromDto(TestObjectDto dto);

    [MapEnum(EnumMappingStrategy.ByName)]
    TestEnumDtoByName MapToEnumDtoByName(TestEnum v);

    void UpdateDto(TestObject source, TestObjectDto target);
}
