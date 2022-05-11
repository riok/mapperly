using System.Runtime.CompilerServices;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests;

public abstract class BaseMapperTest
{
    protected string GetGeneratedMapperFilePath(string name, [CallerFilePath] string filePath = "")
    {
        return Path.Join(
            Path.GetDirectoryName(filePath),
            "obj",
            "GeneratedFiles",
            "Riok.Mapperly",
            "Riok.Mapperly.MapperGenerator",
            name + ".g.cs");
    }

    protected TestObject NewTestObj()
    {
        return new TestObject(7)
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
            Flattening = new() { IdValue = 10 },
            NullableFlattening = new() { IdValue = 100 },
            UnflatteningIdValue = 20,
            NullableUnflatteningIdValue = 200,
            RecursiveObject =
                new(5)
                {
                    EnumValue = TestEnum.Value10,
                    EnumName = TestEnum.Value30,
                    EnumReverseStringValue = nameof(TestEnumDtoByValue.DtoValue3)
                },
            NullableReadOnlyObjectCollection =
                new[] { new TestObjectNested { IntValue = 10 }, new TestObjectNested { IntValue = 20 }, },
            SourceTargetSameObjectType = new TestObject(8) { IntValue = 99, }
        };
    }
}
