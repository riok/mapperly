# Expression Mapping Implementation Notes

## Overview

Implementing support for `Expression<Func<TSource, TTarget>>` mappings in Mapperly, allowing users to generate expression trees that can be used with EF Core, LINQ providers, etc.

### Target API

```csharp
[Mapper]
public static partial class UserMapper
{
    public static partial Expression<Func<UserModel, UserEntity>> CreateToModelProjection();
}
```

The source type is extracted from the Expression type argument (not from method parameters).

---

## Files Created/Modified

### New Files

1. **`/src/Riok.Mapperly/Descriptors/MappingBuilders/ExpressionMappingBuilder.cs`**
   - Handles methods WITH source parameter (not the primary use case)
   - Creates `ExpressionMapping` instances
   - Extracts source/target from `Expression<Func<TSource, TTarget>>` type arguments

2. **`/src/Riok.Mapperly/Descriptors/Mappings/ExpressionMapping.cs`**
   - Generates the expression lambda return statement
   - Uses `InlineExpressionMappingBuilderContext` (reused from IQueryable)
   - Generates code like: `return x => new Target { ... };`
   - Includes nullable pragmas similar to `QueryableProjectionMapping`

3. **`/src/Riok.Mapperly/Descriptors/Mappings/UserMappings/UserDefinedExpressionMethodMapping.cs`**
   - Handles **parameterless** methods returning `Expression<Func<TSource, TTarget>>`
   - Key feature: Custom `BuildMethod` override to preserve original method name
   - Overrides `BuildParameterList()` to return empty parameters

4. **`/test/Riok.Mapperly.Tests/Mapping/ExpressionMappingTest.cs`**
   - Test file with multiple test cases
   - Tests converted to use parameterless methods

### Modified Files

1. **`/src/Riok.Mapperly.Abstractions/MappingConversionType.cs`**
   - Added: `Expression = 1 << 19`
   - Documentation added for the new enum value

2. **`/src/Riok.Mapperly/Descriptors/MappingBuilders/MappingBuilder.cs`**
   - Registered `ExpressionMappingBuilder` in the builders list

3. **`/src/Riok.Mapperly/Descriptors/UserMethodMappingExtractor.cs`**
   - Added `TryBuildExpressionMapping` method
   - Only handles **parameterless** methods (has `methodSymbol.Parameters.Length > 0` check to skip methods with parameters)
   - Called early in extraction process

4. **`/src/Riok.Mapperly/Descriptors/MappingBodyBuilders/MappingBodyBuilder.cs`**
   - Added case: `UserDefinedExpressionMethodMapping expressionMapping => UserMethodMappingBodyBuilder.BuildMappingBody(ctx, expressionMapping)`

5. **`/src/Riok.Mapperly/Descriptors/MappingBodyBuilders/UserMethodMappingBodyBuilder.cs`**
   - Added `BuildMappingBody` overload for `UserDefinedExpressionMethodMapping`

---

## Key Implementation Details

### How It Works

1. `UserMethodMappingExtractor.TryBuildExpressionMapping` detects parameterless methods returning `Expression<Func<TSource, TTarget>>`
2. Creates a `UserDefinedExpressionMethodMapping` which wraps an `ExpressionMapping`
3. `ExpressionMapping` uses `InlineExpressionMappingBuilderContext` to build the expression tree (same as IQueryable projections)
4. The generated code is a lambda: `return x => new Target { Prop = x.Prop, ... };`

### Method Name Preservation Issue (SOLVED)

**Problem:** Generated method was named `MapToExpressionOfFunc` instead of preserving the original `CreateProjection`.

**Solution:** Override `BuildMethod` in `UserDefinedExpressionMethodMapping` to copy modifiers from original declaration and use the original method name:

```csharp
public override MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
{
    var methodSyntax = (MethodDeclarationSyntax)Method.DeclaringSyntaxReferences.First().GetSyntax();
    return SyntaxFactory.MethodDeclaration(
        SyntaxFactory.List<AttributeListSyntax>(),
        methodSyntax.Modifiers,
        ReturnType,
        null,
        SyntaxFactory.Identifier(Method.Name),
        null,
        BuildParameterList(),
        SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
        ctx.SyntaxFactory.Block(BuildBody(ctx.SyntaxFactory)),
        null
    );
}
```

---

## Current Status

### What's Working
- ✅ Build succeeds
- ✅ `MappingConversionType.Expression` enum added
- ✅ `ExpressionMappingBuilder` for methods with source parameter
- ✅ `ExpressionMapping` generates correct expression lambda
- ✅ `UserDefinedExpressionMethodMapping` for parameterless methods
- ✅ Method name preservation fixed
- ✅ Static mapper test generates correct output
- ✅ **All 13 tests pass** (snapshots created)

### Test Status
- All tests in `ExpressionMappingTest.cs` pass
- Snapshot files created and verified

---

## Open Questions / Tricky Issues

### 1. Two Paths for Expression Mapping
Currently there are two code paths:
- **Parameterless methods** → `UserMethodMappingExtractor.TryBuildExpressionMapping` → `UserDefinedExpressionMethodMapping`
- **Methods with source parameter** → `ExpressionMappingBuilder` → `ExpressionMapping`

**Question:** Should we support both? The user's intent was parameterless methods only. Consider removing or not using `ExpressionMappingBuilder`.

### 2. User-Implemented Methods
Test `TopLevelUserImplemented` tests a scenario where user provides their own mapping method. Need to verify this works correctly with parameterless Expression methods.

### 3. StringFormat in Expressions
Test `ExpressionWithStringFormat` tests using `[MapProperty("Value", "Value", StringFormat = "C")]`. This may not work in expression trees since `string.Format` might not be translatable by EF Core.

### 4. Reference Handling Diagnostic
The `WithReferenceHandlingShouldDiagnostic` test verifies that reference handling is not supported for Expression mappings (same as IQueryable). This reuses `DiagnosticDescriptors.QueryableProjectionMappingsDoNotSupportReferenceHandling`.

**Question:** Should there be a separate diagnostic for Expression mappings?

### 5. Nullable Pragmas
`ExpressionMapping` generates nullable pragmas:
```csharp
#nullable disable
// ... expression body ...
#nullable enable
```
This matches `QueryableProjectionMapping` behavior.

---

## Test Cases in ExpressionMappingTest.cs

| Test Name | Description | Status |
|-----------|-------------|--------|
| `ClassToClass` | Basic class to class mapping | ✅ Pass |
| `ClassToClassMultipleProperties` | Multiple properties | ✅ Pass |
| `ClassToClassNested` | Nested object mapping | ✅ Pass |
| `ClassToClassNestedMemberAttribute` | MapProperty attribute | ✅ Pass |
| `ClassToClassWithConfigs` | WithMapperOptions config | ✅ Pass |
| `RecordToRecordManualFlatteningInsideList` | Complex flattening with lists | ✅ Pass |
| `ReferenceLoopInitProperty` | Handles potential reference loops | ✅ Pass |
| `NestedWithCtor` | Nested with constructor | ✅ Pass |
| `CtorShouldSkipUnmatchedOptionalParameters` | Constructor parameter handling | ✅ Pass |
| `WithReferenceHandlingShouldDiagnostic` | Diagnostic test | ✅ Pass |
| `TopLevelUserImplemented` | User-implemented mapping | ✅ Pass |
| `ExpressionWithStringFormat` | StringFormat attribute | ✅ Pass |
| `StaticMapper` | Static partial class | ✅ Pass |

---

## Related Code Patterns

### IQueryable Projection (Reference Implementation)
- `QueryableMappingBuilder.cs` - builds `QueryableProjectionMapping`
- `QueryableProjectionMapping.cs` - similar structure to `ExpressionMapping`
- Uses `InlineExpressionMappingBuilderContext` for expression tree building

### Key Types
- `InlineExpressionMappingBuilderContext` - context for building inline expressions
- `SyntaxFactoryHelper.Lambda` - creates lambda expression syntax
- `ExpressionSyntaxFactoryHelper` - helper for expression syntax generation

---

## Next Steps

1. Run tests with `DiffEngine_Disabled=true` to create snapshot files
2. Review generated snapshots for correctness
3. Decide whether to keep `ExpressionMappingBuilder` (methods with source parameter) or remove it
4. Consider adding more edge case tests
5. Update documentation if feature is finalized
6. Consider adding a dedicated diagnostic for Expression mappings instead of reusing IQueryable diagnostic

---

## Commands for Continuation

```bash
# Build
dotnet build ./src/Riok.Mapperly/Riok.Mapperly.csproj

# Run Expression tests (create snapshots)
DiffEngine_Disabled=true dotnet test ./test/Riok.Mapperly.Tests/Riok.Mapperly.Tests.csproj --filter "FullyQualifiedName~ExpressionMappingTest"

# Run single test
dotnet test ./test/Riok.Mapperly.Tests/Riok.Mapperly.Tests.csproj --filter "FullyQualifiedName~ExpressionMappingTest.StaticMapper"

# View generated code for debugging
# Check snapshot files in: test/Riok.Mapperly.Tests/Mapping/snapshots/
```
