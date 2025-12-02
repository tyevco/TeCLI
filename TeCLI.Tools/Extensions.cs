using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeCLI.Extensions;

public static class Extensions
{
    public static string[] GetAttributeNamesForComparison(params string[] attributes)
    {
        var names = new string[attributes.Length * 2];
        for (int i = 0, j = 0; i < attributes.Length; i++)
        {
            string name = attributes[i];
            names[j++] = name;
            names[j++] = name.EndsWith("Attribute") ? name.Substring(0, name.Length - 9) : $"{name}Attribute";
        }

        return names;
    }

    public static bool HasAnyAttribute(this IReadOnlyList<AttributeSyntax> attributeList, params string[] attributes)
    {
        return attributeList.Any(attribute => attribute.Name.EndsWithOneOf(GetAttributeNamesForComparison(attributes)));
    }

    public static bool EndsWithOneOf(this NameSyntax name, params string[] values)
    {
        return name.ToString().EndsWithOneOf(values);
    }

    public static bool EndsWithOneOf(this string nameString, params string[] values)
    {
        return values.Any(x => nameString.EndsWith(x, StringComparison.Ordinal));
    }

    public static IEnumerable<TMemberType> GetMembersWithAttribute<TMemberType>(this INamedTypeSymbol classSymbol, string attributeName)
        where TMemberType : ISymbol
    {
        return classSymbol.GetMembers().OfType<TMemberType>()?.Where(x => x.HasAttribute(attributeName)) ?? [];
    }

    public static IEnumerable<IPropertySymbol> GetPropertiesWithAttribute(this INamedTypeSymbol classSymbol, string attributeName)
    {
        return classSymbol.GetMembers().OfType<IPropertySymbol>()?.Where(x => x.HasAttribute(attributeName)) ?? [];
    }

    public static bool HasAttribute(this ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes().Any(x => x.AttributeClass?.Name.EndsWithOneOf([attributeName, attributeName.Replace("Attribute", "")]) ?? false);
    }

    public static bool TryGetAttribute(this ISymbol symbol, out AttributeData? attributeData, string attributeName)
    {
        attributeData = symbol.GetAttributes().FirstOrDefault(ad => GetAttributeNamesForComparison(attributeName).Any(x => string.Equals(x, ad.AttributeClass?.Name)));

        return attributeData != null;
    }

    public static AttributeData? GetAttribute(this ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes().FirstOrDefault(ad => GetAttributeNamesForComparison(attributeName).Any(x => string.Equals(x, ad.AttributeClass?.Name)));
    }

    public static string FormatCode(this string source)
    {
        return CSharpSyntaxTree.ParseText(source).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
    }


    public static bool HasAttribute(this MemberDeclarationSyntax symbol, string attributeName)
    {
        return symbol
                .AttributeLists
                .Any(al =>
                    al
                        .Attributes
                        .Any(x => x.Name.EndsWithOneOf(GetAttributeNamesForComparison(attributeName))));
    }

    public static AttributeSyntax? GetAttribute(this MemberDeclarationSyntax symbol, string attributeName)
    {
        return symbol
                .AttributeLists
                .Select(al =>
                    al
                        .Attributes
                        .FirstOrDefault(x => x.Name.EndsWithOneOf(GetAttributeNamesForComparison(attributeName))))
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
        // Check if it's a primitive type
        if (IsPrimitiveType(typeSymbol))
        {
            return true;
        }

        // Check if it's an enum type
        if (IsEnumType(typeSymbol))
        {
            return true;
        }

        // Check if it's a common convertible type
        if (IsCommonConvertibleType(typeSymbol))
        {
            return true;
        }

        // Check if it's a stream type (special handling for stdin/stdout)
        if (IsStreamType(typeSymbol))
        {
            return true;
        }

        // Check if it's a collection of primitive types, enums, or common types
        if (IsCollectionType(typeSymbol, out var elementType) && elementType != null)
        {
            return IsPrimitiveType(elementType) || IsEnumType(elementType) || IsCommonConvertibleType(elementType);
        }

        return false;
    }

    public static bool IsCommonConvertibleType(this ITypeSymbol typeSymbol)
    {
        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return fullTypeName switch
        {
            "global::System.Uri" => true,
            "global::System.DateTime" => true,
            "global::System.DateTimeOffset" => true,
            "global::System.TimeSpan" => true,
            "global::System.Guid" => true,
            "global::System.IO.FileInfo" => true,
            "global::System.IO.DirectoryInfo" => true,
            _ => false
        };
    }

    /// <summary>
    /// Checks if the type is a stream type that requires special handling for stdin/stdout/file streams
    /// </summary>
    public static bool IsStreamType(this ITypeSymbol typeSymbol)
    {
        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return fullTypeName switch
        {
            "global::System.IO.Stream" => true,
            "global::System.IO.TextReader" => true,
            "global::System.IO.TextWriter" => true,
            "global::System.IO.StreamReader" => true,
            "global::System.IO.StreamWriter" => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the stream direction (input or output) based on the stream type
    /// </summary>
    public static StreamDirection GetStreamDirection(this ITypeSymbol typeSymbol)
    {
        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return fullTypeName switch
        {
            "global::System.IO.Stream" => StreamDirection.Bidirectional,
            "global::System.IO.TextReader" => StreamDirection.Input,
            "global::System.IO.TextWriter" => StreamDirection.Output,
            "global::System.IO.StreamReader" => StreamDirection.Input,
            "global::System.IO.StreamWriter" => StreamDirection.Output,
            _ => StreamDirection.Unknown
        };
    }

    /// <summary>
    /// Gets the concrete stream type name for the stream type
    /// </summary>
    public static string? GetStreamTypeName(this ITypeSymbol typeSymbol)
    {
        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return fullTypeName switch
        {
            "global::System.IO.Stream" => "Stream",
            "global::System.IO.TextReader" => "TextReader",
            "global::System.IO.TextWriter" => "TextWriter",
            "global::System.IO.StreamReader" => "StreamReader",
            "global::System.IO.StreamWriter" => "StreamWriter",
            _ => null
        };
    }

    /// <summary>
    /// Checks if the type is a progress context type that should be auto-injected.
    /// Supports IProgressContext interface for rich terminal UI progress indicators.
    /// </summary>
    public static bool IsProgressContextType(this ITypeSymbol typeSymbol)
    {
        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return fullTypeName switch
        {
            "global::TeCLI.Console.IProgressContext" => true,
            "TeCLI.Console.IProgressContext" => true,
            _ => typeSymbol.Name == "IProgressContext" &&
                 typeSymbol.ContainingNamespace?.ToDisplayString() == "TeCLI.Console"
        };
    }

    public enum StreamDirection
    {
        Unknown,
        Input,
        Output,
        Bidirectional
    }

    public static string? GetCommonTypeParseMethod(this ITypeSymbol typeSymbol)
    {
        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return fullTypeName switch
        {
            "global::System.Uri" => "new System.Uri({0})",
            "global::System.DateTime" => "System.DateTime.Parse({0})",
            "global::System.DateTimeOffset" => "System.DateTimeOffset.Parse({0})",
            "global::System.TimeSpan" => "System.TimeSpan.Parse({0})",
            "global::System.Guid" => "System.Guid.Parse({0})",
            "global::System.IO.FileInfo" => "new System.IO.FileInfo({0})",
            "global::System.IO.DirectoryInfo" => "new System.IO.DirectoryInfo({0})",
            _ => null
        };
    }

    public static bool IsPrimitiveType(this ITypeSymbol typeSymbol)
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

    public static bool IsEnumType(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.TypeKind == TypeKind.Enum;
    }

    public static bool IsFlagsEnum(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind != TypeKind.Enum)
        {
            return false;
        }

        // Check if the enum has the [Flags] attribute
        return typeSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.Name == "FlagsAttribute" &&
            attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "System");
    }

    public static bool IsCollectionType(this ITypeSymbol typeSymbol, out ITypeSymbol? elementType)
    {
        elementType = null;

        // Check for array type (T[])
        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            elementType = arrayType.ElementType;
            return true;
        }

        // Check for generic collection types (List<T>, IEnumerable<T>, ICollection<T>, etc.)
        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var typeArguments = namedType.TypeArguments;
            if (typeArguments.Length == 1)
            {
                var typeName = namedType.OriginalDefinition.ToDisplayString();

                // Support common collection interfaces and types
                if (typeName == "System.Collections.Generic.List<T>" ||
                    typeName == "System.Collections.Generic.IEnumerable<T>" ||
                    typeName == "System.Collections.Generic.ICollection<T>" ||
                    typeName == "System.Collections.Generic.IList<T>" ||
                    typeName == "System.Collections.Generic.IReadOnlyCollection<T>" ||
                    typeName == "System.Collections.Generic.IReadOnlyList<T>")
                {
                    elementType = typeArguments[0];
                    return true;
                }
            }
        }

        return false;
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

    /// <summary>
    /// Checks if a type symbol represents a CancellationToken
    /// </summary>
    public static bool IsCancellationToken(this ITypeSymbol typeSymbol)
    {
        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return fullTypeName == "global::System.Threading.CancellationToken";
    }

    /// <summary>
    /// Checks if a parameter symbol is a CancellationToken parameter
    /// </summary>
    public static bool IsCancellationTokenParameter(this IParameterSymbol parameterSymbol)
    {
        return parameterSymbol.Type.IsCancellationToken();
    }

    /// <summary>
    /// Checks if a method has a CancellationToken parameter
    /// </summary>
    public static bool HasCancellationTokenParameter(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.Parameters.Any(p => p.IsCancellationTokenParameter());
    }

    /// <summary>
    /// Gets the CancellationToken parameter from a method, if present
    /// </summary>
    public static IParameterSymbol? GetCancellationTokenParameter(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.Parameters.FirstOrDefault(p => p.IsCancellationTokenParameter());
    }

}
