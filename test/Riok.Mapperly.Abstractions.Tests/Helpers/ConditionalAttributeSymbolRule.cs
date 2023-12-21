using System.Diagnostics;
using Mono.Cecil;
using NetArchTest.Rules;

namespace Riok.Mapperly.Abstractions.Tests.Helpers;

internal class ConditionalAttributeSymbolRule(string condition) : ICustomRule
{
    public bool MeetsRule(TypeDefinition type)
    {
        var attr = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(ConditionalAttribute).FullName);
        return attr != null && attr.ConstructorArguments.Count == 1 && condition.Equals(attr.ConstructorArguments[0].Value);
    }
}
