using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeCLI.Attributes;
using TeCLI.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TeCLI.Generators;

public partial class CommandLineArgsGenerator
{
    /// <summary>
    /// Generates DisplayApplicationHelp method (Roslyn syntax version)
    /// </summary>
    private MethodDeclarationSyntax GenerateDisplayApplicationHelpMethod()
    {
        var statements = new List<StatementSyntax>
        {
            ParseStatement(@"Console.WriteLine(""Usage: <command> [action] [options] [arguments]"");"),
            ParseStatement(@"Console.WriteLine();"),
            ParseStatement(@"Console.WriteLine(""Global Options:"");"),
            ParseStatement(@"Console.WriteLine(""  --help, -h                     Display help information"");"),
            ParseStatement(@"Console.WriteLine(""  --version                      Display version information"");"),
            ParseStatement(@"Console.WriteLine(""  --generate-completion <shell>  Generate shell completion script"");"),
            ParseStatement(@"Console.WriteLine(""                                 Supported shells: bash, zsh, powershell, fish"");"),
            ParseStatement(@"Console.WriteLine();"),
            ParseStatement(@"Console.WriteLine(""Available commands:"");"),
            ParseStatement(@"Console.WriteLine();"),
            ParseStatement(@"// Commands will be listed by individual command help generators")
        };

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("DisplayApplicationHelp"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithBody(Block(statements));
    }

    /// <summary>
    /// Generates DisplayVersion method (Roslyn syntax version)
    /// </summary>
    private MethodDeclarationSyntax GenerateDisplayVersionMethod()
    {
        var code = @"var assembly = System.Reflection.Assembly.GetEntryAssembly();
if (assembly != null)
{
    var assemblyName = assembly.GetName();
    var appName = assemblyName.Name ?? ""app"";

    // Try to get AssemblyInformationalVersion first (e.g., ""1.2.3-beta"")
    var infoVersionAttr = assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>();
    if (infoVersionAttr != null && !string.IsNullOrEmpty(infoVersionAttr.InformationalVersion))
    {
        Console.WriteLine($""{appName} {infoVersionAttr.InformationalVersion}"");
    }
    else if (assemblyName.Version != null)
    {
        // Fallback to AssemblyVersion
        Console.WriteLine($""{appName} {assemblyName.Version}"");
    }
    else
    {
        Console.WriteLine($""{appName} (version unknown)"");
    }
}
else
{
    Console.WriteLine(""Version information not available"");
}";

        var parsed = ParseStatement($"{{ {code} }}");
        var statements = parsed is BlockSyntax block ? block.Statements : SingletonList(parsed);

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("DisplayVersion"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithBody(Block(statements));
    }

    /// <summary>
    /// Generates command-specific help method (Roslyn syntax version)
    /// </summary>
    private MethodDeclarationSyntax GenerateCommandHelpMethod(CommandSourceInfo commandInfo, Compilation compilation)
    {
        var classSymbol = commandInfo.TypeSymbol!;
        string? commandDesc = commandInfo.Description;

        var statements = new List<StatementSyntax>();

        // Display command header with full path and aliases
        string fullPath = commandInfo.GetFullCommandPath();
        if (commandInfo.Aliases.Count > 0)
        {
            var aliasesStr = string.Join(", ", commandInfo.Aliases.Select(a => $"\\\"{a}\\\""));
            statements.Add(ParseStatement($@"Console.WriteLine(""Command: {fullPath} (aliases: {aliasesStr})"");"));
        }
        else
        {
            statements.Add(ParseStatement($@"Console.WriteLine(""Command: {fullPath}"");"));
        }

        if (!string.IsNullOrEmpty(commandDesc))
        {
            statements.Add(ParseStatement($@"Console.WriteLine(""Description: {EscapeForStringLiteral(commandDesc!)}"");"));
        }
        statements.Add(ParseStatement(@"Console.WriteLine();"));

        // Get all actions
        var actionMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, ActionAttribute>().ToList();
        var primaryMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, PrimaryAttribute>().ToList();

        // Build usage patterns
        if (primaryMethods.Count > 0 || actionMethods.Count > 0 || commandInfo.Subcommands.Count > 0)
        {
            statements.Add(ParseStatement(@"Console.WriteLine(""Usage:"");"));

            if (primaryMethods.Count > 0)
            {
                var primaryMethod = primaryMethods.First();
                string usagePattern = BuildUsagePatternFromInfo(commandInfo, null, primaryMethod);
                statements.Add(ParseStatement($@"Console.WriteLine(""  {EscapeForStringLiteral(usagePattern)}"");"));
            }

            if (commandInfo.Subcommands.Count > 0)
            {
                statements.Add(ParseStatement($@"Console.WriteLine(""  {fullPath} <subcommand> [options]"");"));
            }

            foreach (var action in actionMethods)
            {
                var actionAttr = action.GetAttribute<ActionAttribute>();
                if (actionAttr != null && actionAttr.ConstructorArguments.Length > 0)
                {
                    string actionName = actionAttr.ConstructorArguments[0].Value?.ToString() ?? action.Name;
                    string usagePattern = BuildUsagePatternFromInfo(commandInfo, actionName, action);
                    statements.Add(ParseStatement($@"Console.WriteLine(""  {EscapeForStringLiteral(usagePattern)}"");"));
                }
            }

            statements.Add(ParseStatement(@"Console.WriteLine();"));
        }

        // Display subcommands if any
        if (commandInfo.Subcommands.Count > 0)
        {
            statements.Add(ParseStatement(@"Console.WriteLine(""Subcommands:"");"));

            foreach (var subcommand in commandInfo.Subcommands)
            {
                string subDisplay = subcommand.CommandName!;
                if (subcommand.Aliases.Count > 0)
                {
                    var aliasesStr = string.Join(", ", subcommand.Aliases);
                    subDisplay = $"{subcommand.CommandName} ({aliasesStr})";
                }

                if (!string.IsNullOrEmpty(subcommand.Description))
                {
                    statements.Add(ParseStatement($@"Console.WriteLine(""  {subDisplay.PadRight(30)} {EscapeForStringLiteral(subcommand.Description!)}"");"));
                }
                else
                {
                    statements.Add(ParseStatement($@"Console.WriteLine(""  {subDisplay}"");"));
                }
            }

            statements.Add(ParseStatement(@"Console.WriteLine();"));
        }

        // Display actions
        if (actionMethods.Count > 0)
        {
            statements.Add(ParseStatement(@"Console.WriteLine(""Actions:"");"));

            foreach (var action in actionMethods)
            {
                var actionAttr = action.GetAttribute<ActionAttribute>();
                if (actionAttr != null && actionAttr.ConstructorArguments.Length > 0)
                {
                    string actionName = actionAttr.ConstructorArguments[0].Value?.ToString() ?? action.Name;
                    string? actionDesc = actionAttr.NamedArguments.FirstOrDefault(na => na.Key == "Description").Value.Value?.ToString();

                    var actionAliases = new List<string>();
                    var actionAliasesArg = actionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
                    if (!actionAliasesArg.Value.IsNull && actionAliasesArg.Value.Kind == TypedConstantKind.Array)
                    {
                        foreach (var value in actionAliasesArg.Value.Values)
                        {
                            if (value.Value is string alias)
                            {
                                actionAliases.Add(alias);
                            }
                        }
                    }

                    string actionDisplay = actionName;
                    if (actionAliases.Count > 0)
                    {
                        var aliasesStr = string.Join(", ", actionAliases);
                        actionDisplay = $"{actionName} ({aliasesStr})";
                    }

                    if (!string.IsNullOrEmpty(actionDesc))
                    {
                        statements.Add(ParseStatement($@"Console.WriteLine(""  {actionDisplay.PadRight(30)} {EscapeForStringLiteral(actionDesc!)}"");"));
                    }
                    else
                    {
                        statements.Add(ParseStatement($@"Console.WriteLine(""  {actionDisplay}"");"));
                    }
                }
            }

            statements.Add(ParseStatement(@"Console.WriteLine();"));
        }

        // Display options if there are any
        bool hasOptions = false;
        foreach (var method in primaryMethods.Concat(actionMethods))
        {
            if (method.Parameters.Any(p => p.HasAttribute<OptionAttribute>()))
            {
                hasOptions = true;
                break;
            }
        }

        if (hasOptions)
        {
            statements.Add(ParseStatement(@"Console.WriteLine(""Options:"");"));
            statements.Add(ParseStatement(@"Console.WriteLine(""  --help, -h           Display this help message"");"));
            statements.Add(ParseStatement(@"Console.WriteLine();"));
        }

        // Always show global options
        statements.Add(ParseStatement(@"Console.WriteLine(""Global Options:"");"));
        statements.Add(ParseStatement(@"Console.WriteLine(""  --version            Display version information"");"));
        statements.Add(ParseStatement(@"Console.WriteLine();"));

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier($"DisplayCommand{classSymbol.Name}Help"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithParameterList(ParameterList(SingletonSeparatedList(
                Parameter(Identifier("actionName"))
                    .WithType(NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))))))
            .WithBody(Block(statements));
    }

    private static string EscapeForStringLiteral(string input)
    {
        return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private string BuildUsagePatternFromInfo(CommandSourceInfo commandInfo, string? actionName, IMethodSymbol method)
    {
        StringBuilder usage = new StringBuilder();
        usage.Append(commandInfo.GetFullCommandPath());

        if (!string.IsNullOrEmpty(actionName))
        {
            usage.Append($" {actionName}");
        }

        // Add options
        var optionParams = method.Parameters.Where(p => p.HasAttribute<OptionAttribute>()).ToList();
        if (optionParams.Count > 0)
        {
            usage.Append(" [options]");
        }

        // Add arguments
        var argParams = method.Parameters.Where(p => p.HasAttribute<ArgumentAttribute>()).ToList();
        foreach (var arg in argParams)
        {
            string argName = arg.Name.ToUpper();
            if (arg.HasExplicitDefaultValue)
            {
                usage.Append($" [{argName}]");
            }
            else
            {
                usage.Append($" <{argName}>");
            }
        }

        return usage.ToString();
    }

    /// <summary>
    /// Generates documentation/help source file for a specific command.
    /// Called by Commands.cs to add help methods to the dispatcher.
    /// </summary>
    private void GenerateCommandDocumentation(SourceProductionContext context, Compilation compilation, CommandSourceInfo commandInfo)
    {
        // Build usings
        var usings = new List<UsingDirectiveSyntax>
        {
            UsingDirective(ParseName("System"))
        };

        // Build the class members - generate the help method for this command
        var classMembers = new List<MemberDeclarationSyntax>
        {
            GenerateCommandHelpMethod(commandInfo, compilation)
        };

        // Build the class declaration
        var classDecl = ClassDeclaration("CommandDispatcher")
            .WithModifiers(TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.PartialKeyword)))
            .WithMembers(List(classMembers));

        // Build the namespace
        var namespaceDecl = FileScopedNamespaceDeclaration(ParseName("TeCLI"))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(classDecl));

        // Build the compilation unit
        var compilationUnit = CompilationUnit()
            .WithUsings(List(usings))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDecl))
            .NormalizeWhitespace();

        // Build unique type name including containing types for nested classes
        var typeNameParts = new List<string>();
        var currentType = commandInfo.TypeSymbol!;
        while (currentType != null)
        {
            typeNameParts.Insert(0, currentType.Name);
            currentType = currentType.ContainingType;
        }
        var uniqueTypeName = string.Join("_", typeNameParts);
        context.AddSource($"CommandDispatcher.Help.{uniqueTypeName}.cs", SourceText.From(compilationUnit.ToFullString(), Encoding.UTF8));
    }

    /// <summary>
    /// Generates application-level documentation/help source file.
    /// Called by Commands.cs to add DisplayApplicationHelp and DisplayVersion methods to the dispatcher.
    /// </summary>
    private void GenerateApplicationDocumentation(SourceProductionContext context, Compilation compilation)
    {
        // Build usings
        var usings = new List<UsingDirectiveSyntax>
        {
            UsingDirective(ParseName("System"))
        };

        // Build the class members - generate the application-level help methods
        var classMembers = new List<MemberDeclarationSyntax>
        {
            GenerateDisplayApplicationHelpMethod(),
            GenerateDisplayVersionMethod()
        };

        // Build the class declaration
        var classDecl = ClassDeclaration("CommandDispatcher")
            .WithModifiers(TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.PartialKeyword)))
            .WithMembers(List(classMembers));

        // Build the namespace
        var namespaceDecl = FileScopedNamespaceDeclaration(ParseName("TeCLI"))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(classDecl));

        // Build the compilation unit
        var compilationUnit = CompilationUnit()
            .WithUsings(List(usings))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDecl))
            .NormalizeWhitespace();

        context.AddSource("CommandDispatcher.Help.cs", SourceText.From(compilationUnit.ToFullString(), Encoding.UTF8));
    }
}
