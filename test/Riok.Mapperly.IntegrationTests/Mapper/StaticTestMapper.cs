using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper;

[Mapper]
public static partial class StaticTestMapper
{
    public static partial int DirectInt(int value);

    public static partial long ImplicitCastInt(int value);

    public static partial int ExplicitCastInt(uint value);

    public static partial int? CastIntNullable(int value);

    public static partial Guid ParseableGuid(string id);

    public static partial int ParseableInt(string value);

    public static partial DateTime DirectDateTime(DateTime dateTime);

    public static partial IEnumerable<TestObjectDto> MapAllDtos(IEnumerable<TestObject> objects);

    public static partial TestObjectDto MapToDtoExt(this TestObject src);

    public static TestObjectDto MapToDto(TestObject src)
    {
        var target = MapToDtoInternal(src);
        target.StringValue += "+after-map";
        return target;
    }

    [MapperIgnore(nameof(TestObjectDto.IgnoredStringValue))]
    [MapProperty(nameof(TestObject.RenamedStringValue), nameof(TestObjectDto.RenamedStringValue2))]
    [MapProperty(
        new[] { nameof(TestObject.UnflatteningIdValue) },
        new[] { nameof(TestObjectDto.Unflattening), nameof(TestObjectDto.Unflattening.IdValue) })]
    [MapProperty(
        nameof(TestObject.NullableUnflatteningIdValue),
        $"{nameof(TestObjectDto.NullableUnflattening)}.{nameof(TestObjectDto.NullableUnflattening.IdValue)}")]
    private static partial TestObjectDto MapToDtoInternal(TestObject testObject);

    [MapperIgnore(nameof(TestObject.IgnoredStringValue))]
    public static partial TestObject MapFromDto(TestObjectDto dto);

    [MapEnum(EnumMappingStrategy.ByName)]
    public static partial TestEnumDtoByName MapToEnumDtoByName(TestEnum v);

    public static partial void UpdateDto(TestObject source, TestObjectDto target);

    private static partial int PrivateDirectInt(int value);
}
