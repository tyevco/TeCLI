using TylerCLI.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;


namespace TylerCLI.Extensions.DependencyInjection.Generators;

class TylerCLIDependencyReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> InvokerClasses { get; } = [];

    public Dictionary<string, ClassDeclarationSyntax> ClassMap { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDecl)
        {
            if (classDecl.AttributeLists.Any(al => al.Attributes.HasAnyAttribute<CommandAttribute>()))
            {
                InvokerClasses.Add(classDecl);
                ClassMap[classDecl.Identifier.ToFullString()] = classDecl;
            }
        }
    }
}