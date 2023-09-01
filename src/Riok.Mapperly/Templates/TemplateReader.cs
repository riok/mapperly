using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Templates;

internal static class TemplateReader
{
    private const string ResourceNamePrefix = "Templates/";
    private const string AssemblyNamePlaceholder = "AssemblyName";
    private const string TypeNamePrefix = "global::Riok.Mapperly.Internal.";

    public static TemplateContent ReadContent(TemplateReference reference, string assemblyName)
    {
        var fileName = FileNameBuilder.BuildForTemplate(reference);
        var content = Read(reference).Replace(AssemblyNamePlaceholder, assemblyName, StringComparison.Ordinal);
        return new TemplateContent(fileName, content);
    }

    /// <summary>
    /// Builds the fully qualified name of the type of the template for a given assembly.
    /// The assembly name is appended to the Mapperly namespace to prevent duplicated symbols,
    /// when one assembly uses Mapperly with a template but has InternalsVisibleTo set to a second assembly
    /// which also uses Mapperly.
    /// </summary>
    /// <param name="reference">The name of the template.</param>
    /// <param name="assemblyName">The name of the assembly.</param>
    /// <returns>The resulting type name of the template.</returns>
    public static string GetTypeName(TemplateReference reference, string assemblyName) => $"{TypeNamePrefix}{assemblyName}.{reference}";

    private static string Read(TemplateReference reference)
    {
        var resourceName = ResourceNamePrefix + reference;
        using var stream =
            typeof(TemplateReference).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Template {reference} not found");
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }
}
