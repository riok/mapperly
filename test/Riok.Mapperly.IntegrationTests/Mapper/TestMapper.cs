using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper;

[Mapper]
public partial class TestMapper
{
    public partial int DirectInt(int value);

    public partial long ImplicitCastInt(int value);

    public partial int ExplicitCastInt(uint value);

    public partial int? CastIntNullable(int value);

    public partial Guid ParseableGuid(string id);

    public partial int ParseableInt(string value);

    public partial DateTime DirectDateTime(DateTime dateTime);

    public partial IEnumerable<TestObjectDto> MapAllDtos(IEnumerable<TestObject> objects);

    public TestObjectDto MapToDto(TestObject src)
    {
        var target = MapToDtoInternal(src);
        target.StringValue += "+after-map";
        return target;
    }

    [MapperIgnore(nameof(TestObjectDto.IgnoredStringValue))]
    [MapProperty(nameof(TestObject.RenamedStringValue), nameof(TestObjectDto.RenamedStringValue2))]
    private partial TestObjectDto MapToDtoInternal(TestObject testObject);

    [MapperIgnore(nameof(TestObject.IgnoredStringValue))]
    public partial TestObject MapFromDto(TestObjectDto dto);

    [MapEnum(EnumMappingStrategy.ByName)]
    public partial TestEnumDtoByName MapToEnumDtoByName(TestEnum v);

    public partial void UpdateDto(TestObject source, TestObjectDto target);

    private partial int PrivateDirectInt(int value);
}
