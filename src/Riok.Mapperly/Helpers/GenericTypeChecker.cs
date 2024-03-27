using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Helpers;

/// <summary>
/// Type checker to check generic type parameters.
/// </summary>
public class GenericTypeChecker(SymbolAccessor accessor, WellKnownTypes types)
{
    /// <summary>
    /// Tries to infer the actual types for the type parameters
    /// and checks whether these types conform to the type parameter constraints.
    /// </summary>
    /// <param name="typeParameters">The type parameters.</param>
    /// <param name="parameterArguments">The parameters and the argument for each parameter.</param>
    /// <returns>The result of the type checking.</returns>
    public GenericTypeCheckerResult InferAndCheckTypes(
        IEnumerable<ITypeParameterSymbol> typeParameters,
        params (ITypeSymbol Parameter, ITypeSymbol Argument)[] parameterArguments
    )
    {
        var state = new InferState(typeParameters);
        return InferAndCheckTypes(state, parameterArguments);
    }

    /// <summary>
    /// Checks whether the given types can be assigned to the type parameters.
    /// </summary>
    /// <param name="typeArguments">Type parameters with a type for each parameter which should be assigned to the type parameter.</param>
    /// <returns>Whether the types can be bound to the type parameters.</returns>
    public bool CheckTypes(params (ITypeParameterSymbol, ITypeSymbol)[] typeArguments)
    {
        var state = new InferState(typeArguments.Select(x => x.Item1));
        var parameterArguments = typeArguments.Select<(ITypeParameterSymbol, ITypeSymbol), (ITypeSymbol, ITypeSymbol)>(x =>
            (x.Item1, x.Item2)
        );
        return InferAndCheckTypes(state, parameterArguments).Success;
    }

    private GenericTypeCheckerResult InferAndCheckTypes(
        InferState state,
        IEnumerable<(ITypeSymbol Parameter, ITypeSymbol Argument)> parameterArguments
    )
    {
        var idx = 0;
        foreach (var (param, arg) in parameterArguments)
        {
            var inferredParamType = InferAndCheckTypes(state, param, arg);
            if (inferredParamType == null || !accessor.CanAssign(arg, inferredParamType))
                return GenericTypeCheckerResult.Failure(state.InferredTypes, idx, param, arg);

            idx++;
        }

        return state.AllTypeParametersInferred
            ? GenericTypeCheckerResult.Successful(state.InferredTypes)
            : GenericTypeCheckerResult.Failure(state.InferredTypes);
    }

    private ITypeSymbol? InferAndCheckTypes(InferState state, ITypeSymbol param, ITypeSymbol arg)
    {
        return param switch
        {
            ITypeParameterSymbol typeParam => InferAndCheckTypes(state, typeParam, arg),
            IArrayTypeSymbol paramArray => InferAndCheckTypes(state, paramArray, arg),
            INamedTypeSymbol { IsGenericType: true } paramNamedType => InferAndCheckTypes(state, paramNamedType, arg),
            _ => param,
        };
    }

    private ITypeSymbol? InferAndCheckTypes(InferState state, ITypeParameterSymbol typeParameter, ITypeSymbol arg)
    {
        if (state.IsTypeParameterInferred(typeParameter, out var boundType))
        {
            if (!SymbolEqualityComparer.Default.Equals(boundType, arg))
                return null;

            if (arg.NullableAnnotation == boundType.NullableAnnotation)
                return boundType;

            if (typeParameter.IsNullable() != false || !arg.IsNullable())
                return arg;

            return null;
        }

        state.SetInferredType(typeParameter, arg);
        if (typeParameter.HasConstructorConstraint && !accessor.HasDirectlyAccessibleParameterlessConstructor(arg))
            return null;

        if (typeParameter.IsNullable() == false && arg.NullableAnnotation == NullableAnnotation.Annotated)
            return null;

        if (typeParameter.HasValueTypeConstraint && !arg.IsValueType)
            return null;

        if (typeParameter.HasReferenceTypeConstraint && !arg.IsReferenceType)
            return null;

        foreach (var constraintType in typeParameter.ConstraintTypes)
        {
            var inferredType = InferAndCheckTypes(state, constraintType, arg);
            if (inferredType == null)
                return null;

            if (!accessor.CanAssign(arg, inferredType))
                return null;
        }

        return arg;
    }

    private ITypeSymbol? InferAndCheckTypes(InferState state, INamedTypeSymbol param, ITypeSymbol arg)
    {
        if (!arg.ExtendsOrImplementsGeneric(param.OriginalDefinition, out var genericTypedArgument))
            return null;

        var inferredParamTypeArgs = new ITypeSymbol[param.TypeArguments.Length];
        var inferredParamsHasChanges = false;
        for (var i = 0; i < param.TypeArguments.Length; i++)
        {
            var paramTypeArg = param.TypeArguments[i];
            var argTypeArg = genericTypedArgument.TypeArguments[i];
            var inferredParamTypeArg = InferAndCheckTypes(state, paramTypeArg, argTypeArg);
            if (inferredParamTypeArg == null)
                return null;

            inferredParamTypeArgs[i] = inferredParamTypeArg;
            inferredParamsHasChanges |= !ReferenceEquals(inferredParamTypeArg, paramTypeArg);
        }

        return inferredParamsHasChanges ? param.OriginalDefinition.Construct(inferredParamTypeArgs) : param;
    }

    private ITypeSymbol? InferAndCheckTypes(InferState state, IArrayTypeSymbol param, ITypeSymbol arg)
    {
        if (arg is not IArrayTypeSymbol argArray)
            return null;

        var elementType = InferAndCheckTypes(
            state,
            param.ElementType.WithNullableAnnotation(param.ElementNullableAnnotation),
            argArray.ElementType.WithNullableAnnotation(argArray.ElementNullableAnnotation)
        );
        if (elementType == null)
            return null;

        if (ReferenceEquals(elementType, param.ElementType))
            return param;

        return types.GetArrayType(elementType, argArray.Rank, argArray.ElementNullableAnnotation);
    }

    private readonly struct InferState
    {
        private readonly HashSet<ITypeParameterSymbol> _unboundTypeParameters;
        private readonly Dictionary<ITypeParameterSymbol, ITypeSymbol> _inferredTypes;

        public bool AllTypeParametersInferred => _unboundTypeParameters.Count == 0;

        public IReadOnlyDictionary<ITypeParameterSymbol, ITypeSymbol> InferredTypes => _inferredTypes;

        internal InferState(IEnumerable<ITypeParameterSymbol> typeParameters)
        {
            _unboundTypeParameters = new(typeParameters, SymbolEqualityComparer.Default);
            _inferredTypes = new(SymbolEqualityComparer.Default);
        }

        internal bool IsTypeParameterInferred(ITypeParameterSymbol typeParameter, [NotNullWhen(true)] out ITypeSymbol? o) =>
            _inferredTypes.TryGetValue(typeParameter, out o);

        internal void SetInferredType(ITypeParameterSymbol typeParameter, ITypeSymbol argumentType)
        {
            _unboundTypeParameters.Remove(typeParameter);
            _inferredTypes.Add(typeParameter, argumentType);
        }
    }

    /// <summary>
    /// The result of a type check.
    /// </summary>
    /// <param name="Success">Whether the type check succeeded.</param>
    /// <param name="InferredTypes">The inferred type for each type parameter.</param>
    /// <param name="FailedIndex">The index on which the type check/inferrence failed (-1 if <paramref name="Success"/> is true)</param>
    /// <param name="FailedParameter">The parameter on which the type check/inferrence failed (null if <paramref name="Success"/> is true)</param>
    /// <param name="FailedArgument">The argument on which the type check/inferrence failed (null if <paramref name="Success"/> is true)</param>
    public readonly record struct GenericTypeCheckerResult(
        bool Success,
        IReadOnlyDictionary<ITypeParameterSymbol, ITypeSymbol> InferredTypes,
        int FailedIndex = -1,
        ITypeSymbol? FailedParameter = null,
        ITypeSymbol? FailedArgument = null
    )
    {
        internal static GenericTypeCheckerResult Successful(IReadOnlyDictionary<ITypeParameterSymbol, ITypeSymbol> inferredTypes) =>
            new(true, inferredTypes);

        internal static GenericTypeCheckerResult Failure(
            IReadOnlyDictionary<ITypeParameterSymbol, ITypeSymbol> inferredTypes,
            int failedIndex = -1,
            ITypeSymbol? failedParameter = null,
            ITypeSymbol? failedArgument = null
        )
        {
            return new(false, inferredTypes, failedIndex, failedParameter, failedArgument);
        }
    }
}
