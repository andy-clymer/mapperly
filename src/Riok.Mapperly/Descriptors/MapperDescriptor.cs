using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.UnsafeAccess;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public class MapperDescriptor
{
    private const string AccessorClassName = "UnsafeAccessor";

    private readonly MapperDeclaration _declaration;
    private readonly List<MethodMapping> _methodMappings = [];
    private readonly List<IUnsafeAccessor> _unsafeAccessors = [];

    public bool Static { get; set; }

    public MapperDescriptor(MapperDeclaration declaration, UniqueNameBuilder nameBuilder, SupportedFeatures supportedFeatures)
    {
        _declaration = declaration;
        NameBuilder = nameBuilder;
        SupportedFeatures = supportedFeatures;
        Name = BuildName(declaration.Symbol);
        UnsafeAccessorName = nameBuilder.New(AccessorClassName);

        if (!Symbol.ContainingNamespace.IsGlobalNamespace)
        {
            Namespace = Symbol.ContainingNamespace.ToDisplayString();
        }
    }

    public SupportedFeatures SupportedFeatures { get; }

    public string Name { get; }

    public string? Namespace { get; }

    public string UnsafeAccessorName { get; }

    public ClassDeclarationSyntax Syntax => _declaration.Syntax;

    public INamedTypeSymbol Symbol => _declaration.Symbol;

    public UniqueNameBuilder NameBuilder { get; }

    public IReadOnlyCollection<MethodMapping> MethodMappings => _methodMappings;

    public IReadOnlyCollection<IUnsafeAccessor> UnsafeAccessors => _unsafeAccessors;

    public void AddMethodMappings(IReadOnlyCollection<MethodMapping> mappings) => _methodMappings.AddRange(mappings);

    public void AddUnsafeAccessors(IReadOnlyCollection<IUnsafeAccessor> accessors) => _unsafeAccessors.AddRange(accessors);

    private string BuildName(INamedTypeSymbol symbol)
    {
        if (symbol.ContainingType == null)
            return symbol.Name;

        var sb = new StringBuilder(symbol.Name);
        var containingType = symbol.ContainingType;
        while (containingType != null)
        {
            sb.Insert(0, '.').Insert(0, containingType.Name);
            containingType = containingType.ContainingType;
        }

        return sb.ToString();
    }
}
