using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class EnumerableTest
{
    [Fact]
    public void ArrayToArrayOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void NullableArrayToNonNullableArray()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]?",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source == null ? throw new System.ArgumentNullException(nameof(source)) : source;");
    }

    [Fact]
    public void ArrayOfNullablePrimitiveTypesToNonNullableArray()
    {
        var source = TestSourceBuilder.Mapping(
            "int?[]",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new int[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = source[i] == null ? throw new System.ArgumentNullException(nameof(source[i])) : source[i].Value;
                }

                return target;
                """);
    }

    [Fact]
    public void ArrayOfPrimitiveTypesToNullablePrimitiveTypesArray()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "int?[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new int? [source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (int? )source[i];
                }

                return target;
                """);
    }

    [Fact]
    public void ArrayToArrayOfPrimitiveTypesDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "int[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (int[])source.Clone();");
    }

    [Fact]
    public void ArrayOfNullablePrimitiveTypesToNonNullableArrayDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "int?[]",
            "int[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new int[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = source[i] == null ? throw new System.ArgumentNullException(nameof(source[i])) : source[i].Value;
                }

                return target;
                """);
    }

    [Fact]
    public void ArrayOfPrimitiveTypesToNullablePrimitiveTypesArrayDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "int?[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new int? [source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (int? )source[i];
                }

                return target;
                """);
    }

    [Fact]
    public void ArrayCustomClassToArrayCustomClass()
    {
        var source = TestSourceBuilder.Mapping(
            "B[]",
            "B[]",
            "class B { public int Value {get; set; }}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayCustomClassNullableToArrayCustomClassNonNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "B?[]",
            "B[]",
            "class B { public int Value {get; set; }}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = source[i] == null ? throw new System.ArgumentNullException(nameof(source[i])) : source[i];
                }

                return target;
                """);
    }

    [Fact]
    public void ArrayCustomClassNonNullableToArrayCustomClassNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "B[]",
            "B?[]",
            "class B { public int Value {get; set; }}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::B? [])source;");
    }

    [Fact]
    public void ArrayCustomClassToArrayCustomClassDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "B[]",
            "B[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "class B { public int Value { get; set; }}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = MapToB(source[i]);
                }

                return target;
                """);
    }

    [Fact]
    public void ArrayToArrayOfString()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "string[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayToArrayOfNullableString()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "string?[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string? [])source;");
    }

    [Fact]
    public void ArrayToArrayOfStringDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "string[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string[])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfNullableStringDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "string?[]",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string? [])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfReadonlyStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "A[]",
            "readonly struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayToArrayOfReadonlyStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "A[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "readonly struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::A[])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfMutableStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "A[]",
            "struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayToArrayOfMutableStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "A[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::A[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = MapToA(source[i]);
                }

                return target;
                """);
    }

    [Fact]
    public void ArrayToArrayOfMutableStructDeepCloningLoopNameTaken()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial A[] Map(A[] i);",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::A[i.Length];
                for (var i1 = 0; i1 < i.Length; i1++)
                {
                    target[i1] = MapToA(i[i1]);
                }

                return target;
                """);
    }

    [Fact]
    public void ArrayToArrayOfMutableStructDeepCloningTargetNameTaken()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial A[] Map(A[] target);",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A{}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target1 = new global::A[target.Length];
                for (var i = 0; i < target.Length; i++)
                {
                    target1[i] = MapToA(target[i]);
                }

                return target1;
                """);
    }

    [Fact]
    public void ArrayToArrayOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "long[]",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new int[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (int)source[i];
                }

                return target;
                """);
    }

    [Fact]
    public void EnumerableToArrayOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "int[]");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void EnumerableToEnumerableOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "IEnumerable<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void EnumerableToICollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "ICollection<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "IReadOnlyCollection<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfImplicitTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "IReadOnlyCollection<long>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (long)x));");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "IReadOnlyCollection<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToIListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "IList<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "List<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToIReadOnlyListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "IReadOnlyList<int>");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (int)x));");
    }

    [Fact]
    public void EnumerableToImmutableList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableList<int> Value { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableList.ToImmutableList(source.Value);
                return target;
                """);
    }

    [Fact]
    public void EnumerableToImmutableListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableList<long> Value { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableList.ToImmutableList(global::System.Linq.Enumerable.Select(source.Value, x => (long)x));
                return target;
                """);
    }

    [Fact]
    public void EnumerableToImmutableArray()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableArray<int> Value { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableArray.ToImmutableArray(source.Value);
                return target;
                """);
    }

    [Fact]
    public void EnumerableToImmutableHashSet()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableHashSet<int> Value { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableHashSet.ToImmutableHashSet(source.Value);
                return target;
                """);
    }

    [Fact]
    public void EnumerableToImmutableQueue()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableQueue<int> Value { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableQueue.CreateRange(source.Value);
                return target;
                """);
    }

    [Fact]
    public void EnumerableToImmutableStack()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableStack<int> Value { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableStack.CreateRange(source.Value);
                return target;
                """);
    }

    [Fact]
    public void EnumerableToImmutableSortedSet()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableSortedSet<int> Value { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableSortedSet.ToImmutableSortedSet(source.Value);
                return target;
                """);
    }

    [Fact]
    public void EnumerableToCustomCollection()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "B",
            "class B : ICollection<int> { public void Add(int item) {} }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                foreach (var item in source)
                {
                    target.Add((int)item);
                }

                return target;
                """);
    }

    [Fact]
    public void EnumerableToExplicitAddCustomCollection()
    {
        // should not create a mapping using a looped add inside a foreach loop when th add method is explicit
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "B",
            "class B : ICollection<int> { void ICollection<int>.Add(int item) {} }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """);
    }

    [Fact]
    public void MapToExistingExplicitAddCustomCollection()
    {
        // should not create a mapping using a looped add inside a foreach loop when th add method is explicit
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(IEnumerable<int> source, B target);",
            "class B : ICollection<int> { void ICollection<int>.Add(int item) {} }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("");
    }

    [Fact]
    public void EnumerableToStack()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Stack<long> Value { get; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                foreach (var item in source.Value)
                {
                    target.Value.Push((long)item);
                }

                return target;
                """);
    }

    [Fact]
    public void EnumerableToQueue()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Queue<long> Value { get; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                foreach (var item in source.Value)
                {
                    target.Value.Enqueue((long)item);
                }

                return target;
                """);
    }

    [Fact]
    public void EnumerableToCreatedStack()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Stack<long> Value { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMethodBody("MapToStack",
                """
                var target = new global::System.Collections.Generic.Stack<long>();
                foreach (var item in source)
                {
                    target.Push((long)item);
                }

                return target;
                """);
    }

    [Fact]
    public void EnumerableToCreatedQueue()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Queue<long> Value { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveMethodBody("MapToQueue",
                """
                var target = new global::System.Collections.Generic.Queue<long>();
                foreach (var item in source)
                {
                    target.Enqueue((long)item);
                }

                return target;
                """);
    }

    [Fact]
    public void EnumerableToCustomCollectionWithObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB() => new();"
            + "partial B Map(IEnumerable<long> source);",
            "class B : ICollection<int> { public void Add(int item) {} }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB();
                foreach (var item in source)
                {
                    target.Add((int)item);
                }

                return target;
                """);
    }

    [Fact]
    public Task ShouldUpgradeNullabilityInDisabledNullableContextInSelectClause()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C[] Value { get; set;} }",
            "class B { public D[] Value { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }");

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task MapToExistingCollectionShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(List<A>? source, RepeatedField<B> target);",
            "class RepeatedField<T> : IList<T> { public void Add(T item) {} }",
            "class A { public string Value { get; set; } }",
            "class B { public string Value { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToExistingStackShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(List<A>? source, Stack<B> target);",
            "class A { public string Value { get; set; } }",
            "class B { public string Value { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToExistingQueueShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(List<A>? source, Queue<B> target);",
            "class A { public string Value { get; set; } }",
            "class B { public string Value { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToReadOnlyNullableCollectionProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int> Value { get; } }",
            "class B { public ICollection<long>? Value { get; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToReadOnlyNullableICollectionProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int> Value { get; } }",
            "class B { public ICollection<long>? Value { get; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToReadOnlyNullableCollectionPropertyFromNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int>? Value { get; } }",
            "class B { public ICollection<long>? Value { get; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToReadOnlyCollectionPropertyFromNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int>? Value { get; } }",
            "class B { public ICollection<long> Value { get; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToReadOnlyCollectionProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int> Value { get; } }",
            "class B { public ICollection<long> Value { get; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void EnumerableToReadonlyImmutableListShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableList<int> Value { get; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.SourceMemberNotFound));
    }

    [Fact]
    public void EnumerableToReadonlyImmutableList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableList<int> Value { get; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.SourceMemberNotFound));
    }

    [Fact]
    public void EnumerableToReadonlyImmutableArray()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableArray<int> Value { get; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.SourceMemberNotFound));
    }

    [Fact]
    public void EnumerableToReadonlyImmutableHashSet()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int? Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableHashSet<int> Value { get; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics).Should().HaveDiagnostic(new(DiagnosticDescriptors.SourceMemberNotFound));
    }

    [Fact]
    public void EnumerableToReadonlyImmutableQueue()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableQueue<int> Value { get; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.SourceMemberNotFound));
    }

    [Fact]
    public void EnumerableToReadonlyImmutableStack()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableStack<int> Value { get; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.SourceMemberNotFound));
    }

    [Fact]
    public void EnumerableToReadonlyImmutableSortedSet()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableSortedSet<int> Value { get; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.SourceMemberNotFound));
    }

    [Fact]
    public void EnumerableMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "IEnumerable<int>",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Enumerable));
        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.CouldNotCreateMapping));
    }

    [Fact]
    public void EnumerableToReadOnlyArrayPropertyShouldDiagnostic()
    {
        // should not create a mapping that maps to an array by adding to it in a foreach loop
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int> Value { get; } }",
            "class B { public int[] Value { get; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
            """
            var target = new global::B();
            return target;
            """)
            .HaveDiagnostic(new(DiagnosticDescriptors.CannotMapToReadOnlyMember));
    }
}
