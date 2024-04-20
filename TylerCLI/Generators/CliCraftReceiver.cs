using TylerCLI.Attributes;
using TylerCLI.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;


namespace TylerCLI.Generators;

class TylerCLIReceiver : ISyntaxReceiver
{
    public HashSet<ClassDeclarationSyntax> CommandClasses { get; } = [];

    public HashSet<ClassDeclarationSyntax> ParameterClasses { get; } = [];

    public Dictionary<string, ClassDeclarationSyntax> ClassMap { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDecl)
        {
            if (classDecl.AttributeLists.Any(al => al.Attributes.HasAnyAttribute<CommandAttribute>()))
            {
                CommandClasses.Add(classDecl);
                ClassMap[classDecl.Identifier.ToFullString()] = classDecl;
            }
            else if (classDecl.Members.OfType<PropertyDeclarationSyntax>().Any(ContainsParameters))
            {
                ParameterClasses.Add(classDecl);
                ClassMap[classDecl.Identifier.ToFullString()] = classDecl;
            }
        }
    }

    private bool ContainsParameters(PropertyDeclarationSyntax propertyDecl)
    {
        return propertyDecl.AttributeLists.Any(al => al.Attributes.HasAnyAttribute<ArgumentAttribute, OptionAttribute>());
    }
}