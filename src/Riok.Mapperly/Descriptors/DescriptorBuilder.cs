using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Descriptors.ExternalMappings;
using Riok.Mapperly.Descriptors.FormatProviders;
using Riok.Mapperly.Descriptors.MappingBodyBuilders;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Descriptors.UnsafeAccess;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public class DescriptorBuilder
{
    private readonly MapperDescriptor _mapperDescriptor;
    private readonly SymbolAccessor _symbolAccessor;

    private readonly MappingCollection _mappings = new();
    private readonly InlinedExpressionMappingCollection _inlineMappings = new();

    private readonly MethodNameBuilder _methodNameBuilder = new();
    private readonly MappingBodyBuilder _mappingBodyBuilder;
    private readonly SimpleMappingBuilderContext _builderContext;
    private readonly DiagnosticCollection _diagnostics;
    private readonly UnsafeAccessorContext _unsafeAccessorContext;
    private readonly AttributeDataAccessor _attributeAccessor;

    public DescriptorBuilder(
        CompilationContext compilationContext,
        MapperDeclaration mapperDeclaration,
        SymbolAccessor symbolAccessor,
        MapperConfiguration defaultMapperConfiguration
    )
    {
        var supportedFeatures = SupportedFeatures.Build(compilationContext.Types, symbolAccessor, compilationContext.ParseLanguageVersion);
        _mapperDescriptor = new MapperDescriptor(mapperDeclaration, _methodNameBuilder, supportedFeatures);
        _symbolAccessor = symbolAccessor;
        _mappingBodyBuilder = new MappingBodyBuilder(_mappings);
        _unsafeAccessorContext = new UnsafeAccessorContext(_methodNameBuilder, symbolAccessor);
        _diagnostics = new DiagnosticCollection(mapperDeclaration.Syntax.GetLocation());
        _attributeAccessor = new AttributeDataAccessor(symbolAccessor);

        var genericTypeChecker = new GenericTypeChecker(_symbolAccessor, compilationContext.Types);
        var configurationReader = new MapperConfigurationReader(
            _attributeAccessor,
            _mappings,
            genericTypeChecker,
            _diagnostics,
            compilationContext.Types,
            mapperDeclaration.Symbol,
            defaultMapperConfiguration,
            supportedFeatures
        );

        _builderContext = new SimpleMappingBuilderContext(
            compilationContext,
            mapperDeclaration,
            configurationReader,
            _symbolAccessor,
            genericTypeChecker,
            _attributeAccessor,
            _unsafeAccessorContext,
            _diagnostics,
            new MappingBuilder(_mappings, mapperDeclaration),
            new ExistingTargetMappingBuilder(_mappings, mapperDeclaration),
            _inlineMappings,
            mapperDeclaration.Syntax.GetLocation()
        );
    }

    public (MapperDescriptor descriptor, DiagnosticCollection diagnostics) Build(CancellationToken cancellationToken)
    {
        DetectMapperServiceProviderMember();
        // Propagate SP member name to builder context so mapping builders can use it
        _builderContext.ServiceProviderMemberName = _mapperDescriptor.ServiceProviderMemberName;
        ConfigureMemberVisibility();
        ReserveMethodNames();
        ExtractUserMappings();

        // ExtractObjectFactories needs to be called after ExtractUserMappings due to configuring mapperDescriptor.Static
        var objectFactories = ExtractObjectFactories();
        var constructorFactory = new InstanceConstructorFactory(objectFactories, _symbolAccessor, _unsafeAccessorContext);
        var formatProviders = ExtractFormatProviders();
        EnqueueUserMappings(constructorFactory, formatProviders);
        ExtractExternalMappings();
        _mappingBodyBuilder.BuildMappingBodies(cancellationToken);
        AddUserMappingDiagnostics();
        BuildMappingMethodNames();
        BuildReferenceHandlingParameters();
        AddMappingsToDescriptor();
        AddAccessorsToDescriptor();
        return (_mapperDescriptor, _diagnostics);
    }

    private void DetectMapperServiceProviderMember()
    {
        // Only instance mappers can use DI
        if (_mapperDescriptor.Symbol.IsStatic)
            return;

        var spAttrMembers = _symbolAccessor
            .GetAllMembers(_mapperDescriptor.Symbol)
            .Where(m => _attributeAccessor.HasAttribute<MapperServiceProviderAttribute>(m))
            .ToList();

        // Prefer a readable property/field with type IServiceProvider
        foreach (var m in spAttrMembers)
        {
            var t = m switch
            {
                IPropertySymbol { GetMethod: not null } p => p.Type,
                IFieldSymbol f => f.Type,
                _ => null,
            };

            if (t == null)
                continue;

            if (!SymbolEqualityComparer.Default.Equals(t, _builderContext.Types.IServiceProvider))
            {
                continue;
            }

            _mapperDescriptor.ServiceProviderMemberName = m.Name;
            break;
        }

        // Fallback: if no attribute is found, try to detect a single readable IServiceProvider member
        if (_mapperDescriptor.ServiceProviderMemberName is not null)
        {
            return;
        }

        var spMembersByType = _symbolAccessor
            .GetAllMembers(_mapperDescriptor.Symbol)
            .Where(m =>
            {
                if (m is not (IPropertySymbol { GetMethod: not null } or IFieldSymbol))
                {
                    return false;
                }
                var t = m switch
                {
                    IPropertySymbol p => p.Type,
                    IFieldSymbol f => f.Type,
                    _ => null,
                };
                return t != null && SymbolEqualityComparer.Default.Equals(t, _builderContext.Types.IServiceProvider);
            })
            .ToList();

        if (spMembersByType.Count == 1)
        {
            _mapperDescriptor.ServiceProviderMemberName = spMembersByType[0].Name;
        }
    }

    /// <summary>
    /// Sets the member and constructor visibility filter on the <see cref="_symbolAccessor"/> after validation.
    /// If <see cref="MemberVisibility.Accessible"/> is not set and the compilation does not have UnsafeAccessors,
    /// emit a diagnostic and update the <see cref="MemberVisibility"/> to include <see cref="MemberVisibility.Accessible"/>.
    /// </summary>
    private void ConfigureMemberVisibility()
    {
        var includedMembers = _builderContext.Configuration.Mapper.IncludedMembers;
        var includedConstructors = _builderContext.Configuration.Mapper.IncludedConstructors;

        if (_mapperDescriptor.SupportedFeatures.UnsafeAccessors)
        {
            _symbolAccessor.SetMemberVisibility(includedMembers);
            _symbolAccessor.SetConstructorVisibility(includedConstructors);
            return;
        }

        if (includedMembers.HasFlag(MemberVisibility.Accessible) && includedConstructors.HasFlag(MemberVisibility.Accessible))
        {
            return;
        }

        _diagnostics.ReportDiagnostic(DiagnosticDescriptors.UnsafeAccessorNotAvailable);
        _symbolAccessor.SetMemberVisibility(includedMembers | MemberVisibility.Accessible);
        _symbolAccessor.SetConstructorVisibility(includedConstructors | MemberVisibility.Accessible);
    }

    private void ReserveMethodNames()
    {
        foreach (var methodSymbol in _symbolAccessor.GetAllMembers(_mapperDescriptor.Symbol))
        {
            _methodNameBuilder.Reserve(methodSymbol.Name);
        }
    }

    private void ExtractUserMappings()
    {
        _mapperDescriptor.Static = _mapperDescriptor.Symbol.IsStatic;
        IMethodSymbol? firstNonStaticUserMapping = null;

        foreach (var userMapping in UserMethodMappingExtractor.ExtractUserMappings(_builderContext, _mapperDescriptor.Symbol))
        {
            // if a user defined mapping method is static, all of them need to be static to avoid confusion for mapping method resolution
            // however, user implemented mapping methods are allowed to be static in a non-static context.
            // Therefore, we are only interested in partial method definitions here.
            if (userMapping.Method is { IsStatic: true, IsPartialDefinition: true })
            {
                _mapperDescriptor.Static = true;
            }
            else if (firstNonStaticUserMapping == null && !userMapping.Method.IsStatic)
            {
                firstNonStaticUserMapping = userMapping.Method;
            }

            AddUserMapping(userMapping, false, true);
        }

        if (_mapperDescriptor.Static && firstNonStaticUserMapping is not null)
        {
            _diagnostics.ReportDiagnostic(
                DiagnosticDescriptors.MixingStaticPartialWithInstanceMethod,
                firstNonStaticUserMapping,
                _mapperDescriptor.Symbol.ToDisplayString()
            );
        }
    }

    private ObjectFactoryCollection ExtractObjectFactories()
    {
        return ObjectFactoryBuilder.ExtractObjectFactories(_builderContext, _mapperDescriptor.Symbol, _mapperDescriptor.Static);
    }

    private void EnqueueUserMappings(InstanceConstructorFactory constructorFactory, FormatProviderCollection formatProviders)
    {
        foreach (var userMapping in _mappings.UserMappings)
        {
            var ctx = new MappingBuilderContext(
                _builderContext,
                constructorFactory,
                formatProviders,
                userMapping,
                new TypeMappingKey(userMapping.SourceType, userMapping.TargetType)
            );

            _mappings.EnqueueToBuildBody(userMapping, ctx);
        }
    }

    private void ExtractExternalMappings()
    {
        foreach (var externalMapping in ExternalMappingsExtractor.ExtractExternalMappings(_builderContext, _mapperDescriptor.Symbol))
        {
            AddUserMapping(externalMapping, true, false);
        }
    }

    private FormatProviderCollection ExtractFormatProviders()
    {
        return FormatProviderBuilder.ExtractFormatProviders(_builderContext, _mapperDescriptor.Symbol, _mapperDescriptor.Static);
    }

    private void BuildMappingMethodNames()
    {
        foreach (var methodMapping in _mappings.MethodMappings)
        {
            methodMapping.SetMethodNameIfNeeded(_methodNameBuilder.Build);
        }
    }

    private void BuildReferenceHandlingParameters()
    {
        if (!_builderContext.Configuration.Mapper.UseReferenceHandling)
            return;

        foreach (var methodMapping in _mappings.MethodMappings)
        {
            methodMapping.EnableReferenceHandling(_builderContext.Types.Get<IReferenceHandler>());
        }
    }

    private void AddMappingsToDescriptor()
    {
        // add generated mappings to the mapper
        _mapperDescriptor.AddMethodMappings(_mappings.MethodMappings);
        // add any DI cache fields + helpers
        EmitDiHelpers();
    }

    private void EmitDiHelpers()
    {
        foreach (var (fieldName, mapperIface) in _builderContext.RequestedDiMapperCacheFields)
        {
            EmitDiCacheField(fieldName, mapperIface);
            EmitDiHelperMethod(fieldName, mapperIface);
        }
    }

    private void EmitDiCacheField(string fieldName, INamedTypeSymbol mapperIface)
    {
        var mapperTypeNullable = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NullableType(
            SyntaxFactoryHelper.FullyQualifiedIdentifier(mapperIface)
        );
        var lazyGeneric = LazyGeneric(GlobalSystemAlias(), mapperTypeNullable);
        var lazyNullable = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NullableType(lazyGeneric).AddLeadingSpace().AddTrailingSpace();

        var variable = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(fieldName);
        var declaration = Microsoft
            .CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(lazyNullable)
            .WithVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList([variable]));
        var fieldDecl = Microsoft
            .CodeAnalysis.CSharp.SyntaxFactory.FieldDeclaration(declaration)
            .WithModifiers(
                Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TokenList(
                    Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)
                )
            );
        _mapperDescriptor.AddAdditionalMember(fieldDecl);
    }

    private void EmitDiHelperMethod(string fieldName, INamedTypeSymbol mapperIface)
    {
        var mapperTypeNullable = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NullableType(
            SyntaxFactoryHelper.FullyQualifiedIdentifier(mapperIface)
        );
        var methodName = "GetOrNull_" + fieldName.TrimStart('_');
        var returnType = Microsoft
            .CodeAnalysis.CSharp.SyntaxFactory.NullableType(SyntaxFactoryHelper.FullyQualifiedIdentifier(mapperIface))
            .AddLeadingSpace()
            .AddTrailingSpace();

        var thisExpr = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression();
        var fieldAccess = SyntaxFactoryHelper.MemberAccess(thisExpr, fieldName);
        var lazyTypeForNew = LazyGeneric(GlobalSystemAlias(), mapperTypeNullable).AddLeadingSpace();

        Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax spAccess =
            _mapperDescriptor.ServiceProviderMemberName == null
                ? thisExpr
                : SyntaxFactoryHelper.MemberAccess(thisExpr, _mapperDescriptor.ServiceProviderMemberName);
        var invoker = new SyntaxFactoryHelper(_mapperDescriptor.SupportedFeatures);
        var getService = BuildGetService(spAccess, mapperIface);
        var asIface = AsIface(getService, mapperIface);
        var lambda = NoArgLambda(asIface);
        var newLazy = Microsoft
            .CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(lazyTypeForNew)
            .WithArgumentList(
                Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(
                    Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(
                        Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(lambda)
                    )
                )
            );
        var coalesceAssign = SyntaxFactoryHelper.CoalesceAssignment(fieldAccess, newLazy);
        var localDecl = invoker
            .AddIndentation()
            .AddIndentation()
            .AddIndentation()
            .DeclareLocalVariable("cached", Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(coalesceAssign));
        var ret = invoker
            .AddIndentation()
            .AddIndentation()
            .AddIndentation()
            .Return(
                SyntaxFactoryHelper.MemberAccess(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("cached"), nameof(Lazy<>.Value))
            );
        var method = Microsoft
            .CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(returnType, methodName)
            .WithModifiers(
                Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TokenList(
                    Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)
                )
            )
            .WithBody(invoker.AddIndentation().AddIndentation().Block([localDecl, ret]));
        _mapperDescriptor.AddAdditionalMember(method);
    }

    private static Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax GlobalSystemAlias()
    {
        return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AliasQualifiedName(
            Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(
                Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GlobalKeyword)
            ),
            Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("System")
        );
    }

    private static Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax LazyGeneric(
        Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax globalSystem,
        Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax typeArg
    )
    {
        return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.QualifiedName(
            globalSystem,
            Microsoft
                .CodeAnalysis.CSharp.SyntaxFactory.GenericName("Lazy")
                .WithTypeArgumentList(
                    Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList(
                        Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax>(
                            typeArg
                        )
                    )
                )
        );
    }

    private static Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax BuildGetService(
        Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax spAccess,
        INamedTypeSymbol iface
    )
    {
        var typeofExpr = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeOfExpression(SyntaxFactoryHelper.FullyQualifiedIdentifier(iface));
        return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(
            SyntaxFactoryHelper.MemberAccess(spAccess, nameof(IServiceProvider.GetService)),
            Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(
                Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(
                    Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(typeofExpr)
                )
            )
        );
    }

    private static Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax AsIface(
        Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expr,
        INamedTypeSymbol iface
    )
    {
        return Microsoft
            .CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(
                Microsoft.CodeAnalysis.CSharp.SyntaxKind.AsExpression,
                expr,
                SyntaxFactoryHelper.FullyQualifiedIdentifier(iface)
            )
            .WithOperatorToken(
                Microsoft
                    .CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AsKeyword)
                    .WithLeadingTrivia(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Space)
                    .WithTrailingTrivia(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Space)
            );
    }

    private static Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax NoArgLambda(
        Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax body
    )
    {
        return Microsoft
            .CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(body)
            .WithParameterList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParameterList())
            .WithArrowToken(
                Microsoft
                    .CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsGreaterThanToken)
                    .WithLeadingTrivia(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Space)
                    .WithTrailingTrivia(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Space)
            );
    }

    private void AddAccessorsToDescriptor()
    {
        _mapperDescriptor.UnsafeAccessors = _unsafeAccessorContext;
    }

    private void AddUserMapping(IUserMapping mapping, bool ignoreDuplicates, bool named)
    {
        var name = named ? _attributeAccessor.GetMethodName(mapping.Method) : null;
        var result = _mappings.AddUserMapping(mapping, name);
        if (!ignoreDuplicates && mapping.Default == true && result == MappingCollectionAddResult.NotAddedDuplicated)
        {
            _diagnostics.ReportDiagnostic(
                DiagnosticDescriptors.MultipleDefaultUserMappings,
                mapping.Method,
                mapping.SourceType.ToDisplayString(),
                mapping.TargetType.ToDisplayString()
            );
        }

        _inlineMappings.AddUserMapping(mapping, name);
    }

    private void AddUserMappingDiagnostics()
    {
        foreach (var mapping in _mappings.UsedDuplicatedNonDefaultNonReferencedUserMappings)
        {
            _diagnostics.ReportDiagnostic(
                DiagnosticDescriptors.MultipleUserMappingsWithoutDefault,
                mapping.Method,
                mapping.SourceType.ToDisplayString(),
                mapping.TargetType.ToDisplayString()
            );
        }
    }
}
