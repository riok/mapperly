using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests;

[UsesVerify]
public class MapperTest
{
    [Fact]
    public Task WithInterfaceMapper()
    {
        var model = NewTestObj();
        var dto = MyInterfaceMapper.Instance.MapToDto(model);
        return Verify(dto);
    }

    [Fact]
    public Task WithAbstractClassMapper()
    {
        var model = NewTestObj();
        var dto = MyClassMapper.Instance.MapToDto(model);
        return Verify(dto);
    }

    private TestObject NewTestObj()
    {
        return new TestObject
        {
            IntValue = 10,
            EnumName = TestEnum.Value10,
            EnumValue = TestEnum.Value10,
            NestedNullable = new TestObjectNested { IntValue = 100, },
            StringValue = "fooBar",
            SubObject = new InheritanceSubObject { BaseIntValue = 1, SubIntValue = 2, },
            EnumRawValue = TestEnum.Value20,
            EnumStringValue = TestEnum.Value30,
            IgnoredStringValue = "ignored",
            RenamedStringValue = "fooBar2",
            StringNullableTargetNotNullable = "fooBar3",
            EnumReverseStringValue = nameof(TestEnumDtoByValue.DtoValue3),
            NestedNullableTargetNotNullable = new(),
            RecursiveObject =
                new()
                {
                    EnumValue = TestEnum.Value10,
                    EnumName = TestEnum.Value30,
                    EnumReverseStringValue = nameof(TestEnumDtoByValue.DtoValue3)
                },
            NullableReadOnlyObjectCollection =
                new[] { new TestObjectNested { IntValue = 10 }, new TestObjectNested { IntValue = 20 }, },
            SourceTargetSameObjectType = new TestObject { IntValue = 99, }
        };
    }
}
