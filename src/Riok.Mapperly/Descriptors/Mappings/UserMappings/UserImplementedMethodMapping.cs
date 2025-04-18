using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a mapping method on the mapper which is implemented by the user.
/// </summary>
public class UserImplementedMethodMapping(
    string? receiver,
    IMethodSymbol method,
    bool? isDefault,
    MethodParameter sourceParameter,
    ITypeSymbol targetType,
    MethodParameter? referenceHandlerParameter,
    bool isExternal,
    UserImplementedMethodMapping.TargetNullability targetNullability
) : NewInstanceMapping(sourceParameter.Type, targetType), INewInstanceUserMapping
{
    public enum TargetNullability
    {
        NeverNull,
        NotNullIfSourceNotNull,
        Nullable,
    }

    public IMethodSymbol Method { get; } = method;

    public bool? Default { get; } = isDefault;

    public bool IsExternal { get; } = isExternal;

    public override IEnumerable<TypeMappingKey> BuildAdditionalMappingKeys(TypeMappingConfiguration config)
    {
        var keys = base.BuildAdditionalMappingKeys(config);
        switch (targetNullability)
        {
            case TargetNullability.NeverNull when TargetType.IsNullable():
                keys = keys.Append(new TypeMappingKey(SourceType, TargetType.NonNullable()));
                goto case TargetNullability.NotNullIfSourceNotNull;
            case TargetNullability.NotNullIfSourceNotNull:
                keys = keys.Append(new TypeMappingKey(SourceType.NonNullable(), TargetType.NonNullable()));
                break;
        }

        return keys;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // if the user implemented method is on an interface,
        // we explicitly cast to be able to use the default interface implementation or explicit implementations
        if (Method.ReceiverType?.TypeKind != TypeKind.Interface)
        {
            return ctx.SyntaxFactory.Invocation(
                receiver == null ? IdentifierName(Method.Name) : MemberAccess(receiver, Method.Name),
                sourceParameter.WithArgument(ctx.Source),
                referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
            );
        }

        var castedReceiver = CastExpression(
            FullyQualifiedIdentifier(Method.ReceiverType!),
            receiver == null ? ThisExpression() : IdentifierName(receiver)
        );
        var methodExpr = MemberAccess(ParenthesizedExpression(castedReceiver), Method.Name);
        return ctx.SyntaxFactory.Invocation(
            methodExpr,
            sourceParameter.WithArgument(ctx.Source),
            referenceHandlerParameter?.WithArgument(ctx.ReferenceHandler)
        );
    }
}
