using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeCLI.Extensions;

public static class Extensions
{
    public static bool HasAnyAttribute<T0>(this IReadOnlyList<AttributeSyntax> attributeList)
            where T0 : Attribute
    {
        return attributeList.Any(attribute => attribute.Name.EndsWithOneOf(AttributeExtensions.GetAttributeNamesForComparison<T0>()));
    }

    public static bool HasAnyAttribute<T0, T1>(this IReadOnlyList<AttributeSyntax> attributeList)
            where T0 : Attribute
            where T1 : Attribute
    {
        return attributeList.Any(attribute => attribute.Name.EndsWithOneOf(AttributeExtensions.GetAttributeNamesForComparison<T0, T1>()));
    }

    public static bool HasAnyAttribute<T0, T1, T2>(this IReadOnlyList<AttributeSyntax> attributeList)
            where T0 : Attribute
            where T1 : Attribute
            where T2 : Attribute
    {
        return attributeList.Any(attribute => attribute.Name.EndsWithOneOf(AttributeExtensions.GetAttributeNamesForComparison<T0, T1, T2>()));
    }

    public static bool EndsWithOneOf(this NameSyntax name, params string[] values)
    {
        return name.ToString().EndsWithOneOf(values);
    }

    public static bool EndsWithOneOf(this string nameString, params string[] values)
    {
        return values.Any(x => nameString.EndsWith(x, StringComparison.Ordinal));
    }

    public static IEnumerable<TMemberType> GetMembersWithAttribute<TMemberType, TAttribute>(this INamedTypeSymbol classSymbol)
        where TMemberType : ISymbol
        where TAttribute : Attribute
    {
        return classSymbol.GetMembers().OfType<TMemberType>()?.Where(x => x.HasAttribute<TAttribute>()) ?? [];
    }

    public static IEnumerable<IPropertySymbol> GetPropertiesWithAttribute<TAttribute>(this INamedTypeSymbol classSymbol)
        where TAttribute : Attribute
    {
        return classSymbol.GetMembers().OfType<IPropertySymbol>()?.Where(x => x.HasAttribute<TAttribute>()) ?? [];
    }

    public static bool HasAttribute<TAttribute>(this ISymbol symbol)
        where TAttribute : Attribute
    {
        return symbol.GetAttributes().Any(x => x.AttributeClass?.Name.EndsWithOneOf(AttributeExtensions.GetAttributeNamesForComparison<TAttribute>()) ?? false);
    }

    public static bool TryGetAttribute<TAttribute>(this ISymbol symbol, out AttributeData? attributeData)
        where TAttribute : Attribute
    {
        attributeData = symbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Name == typeof(TAttribute).Name);

        return attributeData != null;
    }

    public static AttributeData? GetAttribute<TAttribute>(this ISymbol symbol)
        where TAttribute : Attribute
    {
        return symbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Name == typeof(TAttribute).Name);
    }

    public static string FormatCode(this string source)
    {
        return CSharpSyntaxTree.ParseText(source).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
    }


    public static bool HasAttribute<TAttribute>(this MemberDeclarationSyntax symbol)
        where TAttribute : Attribute
    {
        return symbol
                .AttributeLists
                .Any(al =>
                    al
                        .Attributes
                        .Any(x => x.Name.EndsWithOneOf(AttributeExtensions.GetAttributeNamesForComparison<TAttribute>())));
    }

    public static AttributeSyntax? GetAttribute<TAttribute>(this MemberDeclarationSyntax symbol)
        where TAttribute : Attribute
    {
        return symbol
                .AttributeLists
                .Select(al =>
                    al
                        .Attributes
                        .FirstOrDefault(x => x.Name.EndsWithOneOf(AttributeExtensions.GetAttributeNamesForComparison<TAttribute>())))
                .FirstOrDefault();
    }

    public static string? GetNamespace(this GeneratorExecutionContext context, MemberDeclarationSyntax syntax)
    {
        var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);

        var classSymbol = semanticModel.GetDeclaredSymbol(syntax) as INamedTypeSymbol;

        if (classSymbol == null)
        {
            return null; // or handle this case appropriately
        }

        return classSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static string? GetNamespace(this Compilation compilation, MemberDeclarationSyntax syntax)
    {
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

        var classSymbol = semanticModel.GetDeclaredSymbol(syntax) as INamedTypeSymbol;

        if (classSymbol == null)
        {
            return null; // or handle this case appropriately
        }

        return classSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static string? GetFullyQualifiedName(this GeneratorExecutionContext context, MemberDeclarationSyntax syntax)
    {
        var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);

        var classSymbol = semanticModel.GetDeclaredSymbol(syntax) as INamedTypeSymbol;

        if (classSymbol == null)
        {
            return null; // or handle this case appropriately
        }

        return classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static string? GetFullyQualifiedName(this Compilation compilation, MemberDeclarationSyntax syntax)
    {
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

        var classSymbol = semanticModel.GetDeclaredSymbol(syntax) as INamedTypeSymbol;

        if (classSymbol == null)
        {
            return null; // or handle this case appropriately
        }

        return classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static string? GetFullNamespace(this INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol == null)
        {
            return null;
        }

        // Get the namespace symbol
        var namespaceSymbol = typeSymbol.ContainingNamespace;
        if (namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace)
        {
            return string.Empty; // No namespace or global namespace
        }

        // Return the fully qualified namespace
        return namespaceSymbol.ToDisplayString();
    }

    public static TSyntax? GetSyntax<TSyntax>(this ISymbol symbol)
        where TSyntax : SyntaxNode
    {
        return symbol.DeclaringSyntaxReferences.Select(s => s?.GetSyntax()).FirstOrDefault(s => s is TSyntax) as TSyntax;
    }

    public static IEnumerable<IPropertySymbol> GetPropertiesOfParameterType(this IParameterSymbol parameterSymbol)
    {
        // Check if the parameter's type is a named type (class, struct, etc.)
        if (parameterSymbol.Type is INamedTypeSymbol namedTypeSymbol)
        {
            // Retrieve all properties of this type
            foreach (var member in namedTypeSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                yield return member;
            }
        }
    }

    public static IEnumerable<IPropertySymbol> GetPropertiesOfParameterType(this IPropertySymbol parameterSymbol)
    {
        // Check if the parameter's type is a named type (class, struct, etc.)
        if (parameterSymbol.Type is INamedTypeSymbol namedTypeSymbol)
        {
            // Retrieve all properties of this type
            foreach (var member in namedTypeSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                yield return member;
            }
        }
    }

    public static bool IsValidOptionType(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Boolean => true,
            SpecialType.System_Char => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_String => true,
            _ => false
        };
    }

    public static string? GetBuildProperty(this GeneratorExecutionContext context, string buildPropertyName)
    {
        var configOptions = context.AnalyzerConfigOptions.GetOptions(context.Compilation.SyntaxTrees.First());

        configOptions.TryGetValue($"build_property.{buildPropertyName}", out var propValue);

        return propValue;
    }

    public static bool HasTaskLikeReturnType(this IMethodSymbol methodSymbol)
    {
        var returnType = methodSymbol.ReturnType;

        // Check for Task, Task<T>, ValueTask, or ValueTask<T> by checking the namespace and type name
        var typeNamespace = returnType.ContainingNamespace?.ToDisplayString();
        var typeName = returnType.Name;

        // Check for System.Threading.Tasks.Task or System.Threading.Tasks.Task<T>
        if (typeNamespace == "System.Threading.Tasks")
        {
            if (typeName == "Task" || typeName == "ValueTask")
            {
                return true;
            }
        }

        // Fallback: Check if there's a GetAwaiter method with proper awaiter pattern
        // This handles custom awaitables
        foreach (var member in returnType.GetMembers("GetAwaiter"))
        {
            if (member is IMethodSymbol awaiterMethodSymbol &&
                awaiterMethodSymbol.Parameters.IsEmpty &&
                !awaiterMethodSymbol.ReturnsVoid)
            {
                // Verify the awaiter has required members (IsCompleted, OnCompleted, GetResult)
                var awaiterType = awaiterMethodSymbol.ReturnType;
                var hasIsCompleted = awaiterType.GetMembers("IsCompleted").Any(m => m is IPropertySymbol);
                var hasOnCompleted = awaiterType.GetMembers("OnCompleted").Any(m => m is IMethodSymbol);
                var hasGetResult = awaiterType.GetMembers("GetResult").Any(m => m is IMethodSymbol);

                if (hasIsCompleted && hasOnCompleted && hasGetResult)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static string MapAsync(this IMethodSymbol methodSymbol, Func<string> isAsync, Func<string> isNotAsync)
    {
        return (methodSymbol.IsAsync || methodSymbol.HasTaskLikeReturnType()) ? isAsync() : isNotAsync();
    }

}
