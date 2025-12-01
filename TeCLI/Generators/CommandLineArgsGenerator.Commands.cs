using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TeCLI.Attributes;
using TeCLI.Extensions;
using static TeCLI.Constants;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TeCLI.Generators;

/// <summary>
/// Generates the CommandDispatcher class and command routing logic.
/// This partial class handles the top-level command dispatch infrastructure.
/// </summary>
public partial class CommandLineArgsGenerator
{
    private void GenerateCommandDispatcher(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> commandClasses, ImmutableArray<ClassDeclarationSyntax> globalOptionsClasses)
    {
        // Extract global options information
        GlobalOptionsSourceInfo? globalOptions = null;
        if (globalOptionsClasses.Length > 0)
        {
            // Only support one global options class for now
            var globalOptionsClass = globalOptionsClasses[0];
            globalOptions = ExtractGlobalOptionsInfo(compilation, globalOptionsClass);
        }

        // Build command hierarchies
        var commandHierarchies = BuildCommandHierarchies(compilation, commandClasses);

        // Build the compilation unit using Roslyn syntax
        var usings = new List<UsingDirectiveSyntax>
        {
            UsingDirective(ParseName("System")),
            UsingDirective(ParseName("System.Linq")),
            UsingDirective(ParseName("System.Threading.Tasks"))
        };

        // Add namespace for global options if present
        if (globalOptions != null && !string.IsNullOrEmpty(globalOptions.Namespace))
        {
            usings.Add(UsingDirective(ParseName(globalOptions.Namespace!)));
        }

        // Build the class members
        var classMembers = new List<MemberDeclarationSyntax>();

        // Add global options field if present
        if (globalOptions != null)
        {
            var fieldDecl = FieldDeclaration(
                VariableDeclaration(ParseTypeName(globalOptions.FullTypeName!))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("_globalOptions"))
                            .WithInitializer(EqualsValueClause(
                                ObjectCreationExpression(ParseTypeName(globalOptions.FullTypeName!))
                                    .WithArgumentList(ArgumentList()))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

            classMembers.Add(fieldDecl);
        }

        // Generate the DispatchAsync method
        classMembers.Add(GenerateDispatchAsyncMethod(commandHierarchies, globalOptions));

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

        context.AddSource("CommandDispatcher.cs", SourceText.From(compilationUnit.ToFullString(), Encoding.UTF8));

        // Generate completion support methods in a separate partial class file using CodeBuilder
        GenerateCompletionSupportFile(context, commandHierarchies, globalOptions);

        // Generate dispatch methods for all commands in the hierarchies
        foreach (var commandInfo in commandHierarchies)
        {
            GenerateCommandSourceFileHierarchical(context, compilation, commandInfo, globalOptions);
            GenerateCommandDocumentation(context, compilation, commandInfo);
        }

        GenerateApplicationDocumentation(context, compilation);
    }

    private MethodDeclarationSyntax GenerateDispatchAsyncMethod(List<CommandSourceInfo> commandHierarchies, GlobalOptionsSourceInfo? globalOptions)
    {
        var statements = new List<StatementSyntax>();

        // if (args.Length == 0) { DisplayApplicationHelp(); return; }
        statements.Add(IfStatement(
            BinaryExpression(
                SyntaxKind.EqualsExpression,
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("args"),
                    IdentifierName("Length")),
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))),
            Block(
                ExpressionStatement(InvocationExpression(IdentifierName("DisplayApplicationHelp"))),
                ReturnStatement())));

        // Parse global options if present
        if (globalOptions != null && globalOptions.Options.Count > 0)
        {
            statements.Add(ParseStatement("// Parse global options"));
            statements.Add(ParseStatement("var globalOptionsParsed = new System.Collections.Generic.HashSet<int>();"));
            statements.AddRange(GenerateGlobalOptionsParsingStatements(globalOptions));

            statements.Add(ParseStatement("// Remove parsed global options from args"));
            statements.Add(ParseStatement("var commandArgs = new System.Collections.Generic.List<string>();"));

            // for loop to filter out global options
            statements.Add(ForStatement(
                Block(
                    IfStatement(
                        PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("globalOptionsParsed"),
                                    IdentifierName("Contains")))
                            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("i")))))),
                        Block(ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("commandArgs"),
                                    IdentifierName("Add")))
                            .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                Argument(ElementAccessExpression(IdentifierName("args"))
                                    .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName("i"))))))))))))))
                .WithDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("i"))
                            .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))))))
                .WithCondition(BinaryExpression(SyntaxKind.LessThanExpression,
                    IdentifierName("i"),
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("args"), IdentifierName("Length"))))
                .WithIncrementors(SingletonSeparatedList<ExpressionSyntax>(
                    PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName("i")))));

            statements.Add(ParseStatement("args = commandArgs.ToArray();"));

            // Check again after filtering global options - args might now be empty
            statements.Add(IfStatement(
                BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("args"),
                        IdentifierName("Length")),
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))),
                Block(
                    ExpressionStatement(InvocationExpression(IdentifierName("DisplayApplicationHelp"))),
                    ReturnStatement())));
        }

        // Check for version flag
        statements.Add(IfStatement(
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("args"),
                    IdentifierName("Contains")))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("--version")))))),
            Block(
                ExpressionStatement(InvocationExpression(IdentifierName("DisplayVersion"))),
                ReturnStatement())));

        // Check for help flag
        statements.Add(IfStatement(
            BinaryExpression(SyntaxKind.LogicalOrExpression,
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("args"),
                        IdentifierName("Contains")))
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("--help")))))),
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("args"),
                        IdentifierName("Contains")))
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("-h"))))))),
            Block(
                ExpressionStatement(InvocationExpression(IdentifierName("DisplayApplicationHelp"))),
                ReturnStatement())));

        // Check for generate-completion flag
        statements.Add(IfStatement(
            BinaryExpression(SyntaxKind.LogicalAndExpression,
                BinaryExpression(SyntaxKind.EqualsExpression,
                    ElementAccessExpression(IdentifierName("args"))
                        .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(
                            Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))))),
                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("--generate-completion"))),
                BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("args"), IdentifierName("Length")),
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(2)))),
            Block(
                ExpressionStatement(
                    InvocationExpression(IdentifierName("GenerateCompletion"))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(
                            Argument(ElementAccessExpression(IdentifierName("args"))
                                .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(
                                    Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))))))))))),
                ReturnStatement())));

        // string command = args[0].ToLower();
        statements.Add(LocalDeclarationStatement(
            VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("command"))
                        .WithInitializer(EqualsValueClause(
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    ElementAccessExpression(IdentifierName("args"))
                                        .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(
                                            Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))))),
                                    IdentifierName("ToLower")))))))));

        // string[] remainingArgs = args.Skip(1).ToArray();
        statements.Add(LocalDeclarationStatement(
            VariableDeclaration(ArrayType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("remainingArgs"))
                        .WithInitializer(EqualsValueClause(
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    InvocationExpression(
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("args"),
                                            IdentifierName("Skip")))
                                        .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                            Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)))))),
                                    IdentifierName("ToArray")))))))));

        // Build switch statement for commands
        var switchSections = new List<SwitchSectionSyntax>();

        foreach (var commandInfo in commandHierarchies)
        {
            var methodName = $"Dispatch{GetUniqueTypeName(commandInfo.TypeSymbol)}Async";

            // Primary command name case
            switchSections.Add(SwitchSection()
                .WithLabels(SingletonList<SwitchLabelSyntax>(
                    CaseSwitchLabel(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(commandInfo.CommandName!.ToLower())))))
                .WithStatements(List(new StatementSyntax[]
                {
                    ExpressionStatement(AwaitExpression(
                        InvocationExpression(IdentifierName(methodName))
                            .WithArgumentList(ArgumentList(SeparatedList(new[]
                            {
                                Argument(IdentifierName("remainingArgs")),
                                Argument(IdentifierName("cancellationToken"))
                            }))))),
                    BreakStatement()
                })));

            // Alias cases
            foreach (var alias in commandInfo.Aliases)
            {
                switchSections.Add(SwitchSection()
                    .WithLabels(SingletonList<SwitchLabelSyntax>(
                        CaseSwitchLabel(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(alias.ToLower())))))
                    .WithStatements(List(new StatementSyntax[]
                    {
                        ExpressionStatement(AwaitExpression(
                            InvocationExpression(IdentifierName(methodName))
                                .WithArgumentList(ArgumentList(SeparatedList(new[]
                                {
                                    Argument(IdentifierName("remainingArgs")),
                                    Argument(IdentifierName("cancellationToken"))
                                }))))),
                        BreakStatement()
                    })));
            }
        }

        // Default case with suggestion
        var defaultStatements = new List<StatementSyntax>();

        // Build available commands array
        var availableCommandsElements = new List<ExpressionSyntax>();
        foreach (var commandInfo in commandHierarchies)
        {
            availableCommandsElements.Add(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(commandInfo.CommandName!.ToLower())));
            foreach (var alias in commandInfo.Aliases)
            {
                availableCommandsElements.Add(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(alias.ToLower())));
            }
        }

        defaultStatements.Add(LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("availableCommands"))
                        .WithInitializer(EqualsValueClause(
                            ImplicitArrayCreationExpression(
                                InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                                    SeparatedList(availableCommandsElements)))))))));

        defaultStatements.Add(LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("suggestion"))
                        .WithInitializer(EqualsValueClause(
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("TeCLI"),
                                        IdentifierName("StringSimilarity")),
                                    IdentifierName("FindMostSimilar")))
                            .WithArgumentList(ArgumentList(SeparatedList(new[]
                            {
                                Argument(IdentifierName("command")),
                                Argument(IdentifierName("availableCommands"))
                            })))))))));

        // if (suggestion != null) with error message
        defaultStatements.Add(IfStatement(
            BinaryExpression(SyntaxKind.NotEqualsExpression,
                IdentifierName("suggestion"),
                LiteralExpression(SyntaxKind.NullLiteralExpression)),
            Block(ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Console"),
                        IdentifierName("WriteLine")))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                    Argument(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                            IdentifierName("Format")))
                        .WithArgumentList(ArgumentList(SeparatedList(new[]
                        {
                            Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(ErrorMessages.UnknownCommandWithSuggestion))),
                            Argument(ElementAccessExpression(IdentifierName("args"))
                                .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(
                                    Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))))))),
                            Argument(IdentifierName("suggestion"))
                        }))))))))),
            ElseClause(Block(ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Console"),
                        IdentifierName("WriteLine")))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                    Argument(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                            IdentifierName("Format")))
                        .WithArgumentList(ArgumentList(SeparatedList(new[]
                        {
                            Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(ErrorMessages.UnknownCommand))),
                            Argument(ElementAccessExpression(IdentifierName("args"))
                                .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(
                                    Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))))))
                        }))))))))))));

        defaultStatements.Add(ExpressionStatement(InvocationExpression(IdentifierName("DisplayApplicationHelp"))));
        defaultStatements.Add(BreakStatement());

        switchSections.Add(SwitchSection()
            .WithLabels(SingletonList<SwitchLabelSyntax>(DefaultSwitchLabel()))
            .WithStatements(List(defaultStatements)));

        statements.Add(SwitchStatement(IdentifierName("command"))
            .WithSections(List(switchSections)));

        return MethodDeclaration(
            IdentifierName("Task"),  // Non-generic Task
            Identifier("DispatchAsync"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword)))
            .WithParameterList(ParameterList(SeparatedList(new[]
            {
                Parameter(Identifier("args"))
                    .WithType(ArrayType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                        .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))))),
                Parameter(Identifier("cancellationToken"))
                    .WithType(ParseTypeName("System.Threading.CancellationToken"))
                    .WithDefault(EqualsValueClause(DefaultExpression(ParseTypeName("System.Threading.CancellationToken"))))
            })))
            .WithBody(Block(statements));
    }

    private List<StatementSyntax> GenerateGlobalOptionsParsingStatements(GlobalOptionsSourceInfo globalOptions)
    {
        var statements = new List<StatementSyntax>();

        foreach (var option in globalOptions.Options)
        {
            statements.Add(ParseStatement($"// Parse global option: {option.Name}"));

            // Build condition for option matching
            var conditions = new List<ExpressionSyntax>
            {
                BinaryExpression(SyntaxKind.EqualsExpression,
                    IdentifierName("arg"),
                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal($"--{option.Name}")))
            };

            if (option.ShortName != '\0')
            {
                conditions.Add(BinaryExpression(SyntaxKind.EqualsExpression,
                    IdentifierName("arg"),
                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal($"-{option.ShortName}"))));
            }

            var combinedCondition = conditions.Count == 1
                ? conditions[0]
                : BinaryExpression(SyntaxKind.LogicalOrExpression, conditions[0], conditions[1]);

            var ifBodyStatements = new List<StatementSyntax>
            {
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("globalOptionsParsed"),
                            IdentifierName("Add")))
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("i"))))))
            };

            if (option.IsSwitch)
            {
                ifBodyStatements.Add(ExpressionStatement(
                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("_globalOptions"),
                            IdentifierName(option.Name!)),
                        LiteralExpression(SyntaxKind.TrueLiteralExpression))));
            }
            else
            {
                var parseStatements = new List<StatementSyntax>
                {
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("globalOptionsParsed"),
                                IdentifierName("Add")))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(
                            Argument(BinaryExpression(SyntaxKind.AddExpression,
                                IdentifierName("i"),
                                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))))))))
                };

                // Add parsing based on type
                if (option.IsEnum)
                {
                    parseStatements.Add(ParseStatement(
                        $"_globalOptions.{option.Name} = ({option.DisplayType})System.Enum.Parse(typeof({option.DisplayType}), args[i + 1], ignoreCase: true);"));
                }
                else if (option.HasCustomConverter && !string.IsNullOrEmpty(option.CustomConverterType))
                {
                    parseStatements.Add(ParseStatement($"var converter_{option.Name} = new {option.CustomConverterType}();"));
                    parseStatements.Add(ParseStatement($"_globalOptions.{option.Name} = converter_{option.Name}.Convert(args[i + 1]);"));
                }
                else if (option.IsCommonType && !string.IsNullOrEmpty(option.CommonTypeParseMethod))
                {
                    parseStatements.Add(ParseStatement($"_globalOptions.{option.Name} = {string.Format(option.CommonTypeParseMethod, "args[i + 1]")};"));
                }
                else if (option.DisplayType == "string" || option.DisplayType == "global::System.String")
                {
                    parseStatements.Add(ParseStatement($"_globalOptions.{option.Name} = args[i + 1];"));
                }
                else
                {
                    parseStatements.Add(ParseStatement($"_globalOptions.{option.Name} = {option.DisplayType}.Parse(args[i + 1]);"));
                }

                // Add validation if present
                foreach (var validation in option.Validations)
                {
                    var validationCode = string.Format(validation.ValidationCode, $"_globalOptions.{option.Name}");
                    parseStatements.Add(ParseStatement($"{validationCode};"));
                }

                ifBodyStatements.Add(IfStatement(
                    BinaryExpression(SyntaxKind.LessThanExpression,
                        BinaryExpression(SyntaxKind.AddExpression,
                            IdentifierName("i"),
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))),
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("args"),
                            IdentifierName("Length"))),
                    Block(parseStatements)));
            }

            ifBodyStatements.Add(BreakStatement());

            // Build the for loop
            statements.Add(ForStatement(
                Block(
                    LocalDeclarationStatement(
                        VariableDeclaration(IdentifierName("var"))
                            .WithVariables(SingletonSeparatedList(
                                VariableDeclarator(Identifier("arg"))
                                    .WithInitializer(EqualsValueClause(
                                        ElementAccessExpression(IdentifierName("args"))
                                            .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(
                                                Argument(IdentifierName("i")))))))))),
                    IfStatement(combinedCondition, Block(ifBodyStatements))))
                .WithDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("i"))
                            .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))))))
                .WithCondition(BinaryExpression(SyntaxKind.LessThanExpression,
                    IdentifierName("i"),
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("args"), IdentifierName("Length"))))
                .WithIncrementors(SingletonSeparatedList<ExpressionSyntax>(
                    PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName("i")))));
        }

        return statements;
    }

    private void GenerateCommandSourceFile(SourceProductionContext context, Compilation compilation, List<string> methodNames, ClassDeclarationSyntax classDecl)
    {
        var cb = new CodeBuilder("System", "System.Linq", "TeCLI", "TeCLI.Attributes");

        var actionMap = GetActionInfo(compilation, classDecl);

        cb.AddUsing(compilation.GetNamespace(classDecl)!);

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                foreach (var methodName in methodNames)
                {
                    using (cb.AddBlock($"private async Task {methodName}(string[] args)"))
                    {
                        // Check for help flag first
                        using (cb.AddBlock("if (args.Contains(\"--help\") || args.Contains(\"-h\"))"))
                        {
                            cb.AppendLine($"DisplayCommand{GetUniqueTypeName(classDecl)}Help();");
                            cb.AppendLine("return;");
                        }

                        cb.AddBlankLine();

                        using (cb.AddBlock("if (args.Length == 0)"))
                        {
                            GeneratePrimaryMethodInvocation(cb, compilation, classDecl, throwOnNoPrimary: true);
                        }
                        using (cb.AddBlock("else"))
                        {
                            cb.AppendLine("string action = args[0].ToLower();");
                            cb.AppendLine("string[] remainingArgs = args.Skip(1).ToArray();");

                            using (cb.AddBlock("switch (action)"))
                            {
                                foreach (var action in actionMap)
                                {
                                    GenerateCommandActions(compilation, cb, classDecl, action);
                                }

                                using (cb.AddBlock("default:"))
                                {
                                    GeneratePrimaryMethodInvocation(cb, compilation, classDecl, throwOnNoPrimary: false);

                                    // Build list of available actions (including aliases) for suggestions
                                    cb.AppendLine("var availableActions = new[] {");
                                    bool first = true;
                                    foreach (var action in actionMap)
                                    {
                                        if (!first) cb.Append(", ");
                                        cb.Append($"\"{action.ActionName!.ToLower()}\"");
                                        first = false;

                                        // Add aliases to the suggestion list
                                        foreach (var alias in action.Aliases)
                                        {
                                            cb.Append($", \"{alias.ToLower()}\"");
                                        }
                                    }
                                    cb.AppendLine(" };");

                                    cb.AppendLine("var suggestion = TeCLI.StringSimilarity.FindMostSimilar(action, availableActions);");
                                    using (cb.AddBlock("if (suggestion != null)"))
                                    {
                                        cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownActionWithSuggestion.Replace("\n", "\\n")}", action, suggestion));""");
                                    }
                                    using (cb.AddBlock("else"))
                                    {
                                        cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownAction}", action));""");
                                    }
                                    cb.AppendLine("break;");
                                }
                            }
                        }
                    }

                    cb.AddBlankLine();
                }

                // generator process action methods
                foreach (var entry in actionMap)
                {
                    cb.AddBlankLine();
                    GenerateActionCode(cb, entry);
                }

                cb.AddBlankLine();
            }
        }

        context.AddSource($"CommandDispatcher.Command.{GetUniqueTypeName(classDecl)}.cs", SourceText.From(cb, Encoding.UTF8));
    }

    /// <summary>
    /// Generates dispatch methods for a command hierarchy (including nested subcommands)
    /// </summary>
    private void GenerateCommandSourceFileHierarchical(SourceProductionContext context, Compilation compilation, CommandSourceInfo commandInfo, GlobalOptionsSourceInfo? globalOptions = null)
    {
        var cb = new CodeBuilder("System", "System.Linq", "TeCLI", "TeCLI.Attributes");

        // Add namespace for the command type
        var namespaceSymbol = commandInfo.TypeSymbol!.ContainingNamespace;
        if (namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace)
        {
            cb.AddUsing(namespaceSymbol.ToDisplayString());
        }

        using (cb.AddBlock("namespace TeCLI"))
        {
            using (cb.AddBlock("public partial class CommandDispatcher"))
            {
                // Generate dispatch method for this command
                GenerateCommandDispatchMethod(cb, compilation, commandInfo);

                // Generate action processing methods for this command's actions
                foreach (var action in commandInfo.Actions)
                {
                    cb.AddBlankLine();
                    GenerateActionCode(cb, action, commandInfo, globalOptions);
                }

                cb.AddBlankLine();
            }
        }

        // Build unique type name including containing types for nested classes
        var typeNameParts = new List<string>();
        var currentType = commandInfo.TypeSymbol;
        while (currentType != null)
        {
            typeNameParts.Insert(0, currentType.Name);
            currentType = currentType.ContainingType;
        }
        var uniqueTypeName = string.Join("_", typeNameParts);
        context.AddSource($"CommandDispatcher.Command.{uniqueTypeName}.cs", SourceText.From(cb, Encoding.UTF8));

        // Recursively generate dispatch methods for subcommands
        foreach (var subcommand in commandInfo.Subcommands)
        {
            GenerateCommandSourceFileHierarchical(context, compilation, subcommand, globalOptions);
        }
    }

    /// <summary>
    /// Generates the dispatch method for a single command (which may have subcommands and/or actions)
    /// </summary>
    private void GenerateCommandDispatchMethod(CodeBuilder cb, Compilation compilation, CommandSourceInfo commandInfo)
    {
        var uniqueTypeName = GetUniqueTypeName(commandInfo.TypeSymbol);
        var methodName = $"Dispatch{uniqueTypeName}Async";

        using (cb.AddBlock($"private async Task {methodName}(string[] args, System.Threading.CancellationToken cancellationToken = default)"))
        {
            // Check for help flag first
            using (cb.AddBlock("if (args.Contains(\"--help\") || args.Contains(\"-h\"))"))
            {
                cb.AppendLine($"DisplayCommand{uniqueTypeName}Help();");
                cb.AppendLine("return;");
            }

            cb.AddBlankLine();

            using (cb.AddBlock("if (args.Length == 0)"))
            {
                // If no args, try to invoke primary action
                GeneratePrimaryMethodInvocationFromInfo(cb, compilation, commandInfo, throwOnNoPrimary: true);
            }
            using (cb.AddBlock("else"))
            {
                cb.AppendLine("string subcommandOrAction = args[0].ToLower();");
                cb.AppendLine("string[] remainingArgs = args.Skip(1).ToArray();");

                cb.AddBlankLine();

                using (cb.AddBlock("switch (subcommandOrAction)"))
                {
                    // Generate cases for subcommands first (they take precedence)
                    foreach (var subcommand in commandInfo.Subcommands)
                    {
                        var subMethodName = $"Dispatch{GetUniqueTypeName(subcommand.TypeSymbol)}Async";

                        // Primary subcommand name
                        using (cb.AddBlock($"case \"{subcommand.CommandName!.ToLower()}\":"))
                        {
                            cb.AppendLine($"await {subMethodName}(remainingArgs, cancellationToken);");
                            cb.AppendLine("return;");
                        }

                        cb.AddBlankLine();

                        // Subcommand aliases
                        foreach (var alias in subcommand.Aliases)
                        {
                            using (cb.AddBlock($"case \"{alias.ToLower()}\":"))
                            {
                                cb.AppendLine($"await {subMethodName}(remainingArgs, cancellationToken);");
                                cb.AppendLine("return;");
                            }

                            cb.AddBlankLine();
                        }
                    }

                    // Generate cases for actions
                    foreach (var action in commandInfo.Actions)
                    {
                        var actionInvokeMethodName = $"{GetUniqueTypeName(commandInfo.TypeSymbol)}{action.Method!.Name}";

                        // Check if action has hooks - if so, always use async version
                        bool hasHooks = ActionHasHooks(action, commandInfo);
                        string actionCall = hasHooks
                            ? $"await Process{actionInvokeMethodName}Async(remainingArgs, cancellationToken);"
                            : action.Method.MapAsync(
                                () => $"await Process{actionInvokeMethodName}Async(remainingArgs, cancellationToken);",
                                () => $"Process{actionInvokeMethodName}(remainingArgs, cancellationToken);");

                        // Primary action name
                        using (cb.AddBlock($"case \"{action.ActionName!.ToLower()}\":"))
                        {
                            cb.AppendLine(actionCall);
                            cb.AppendLine("break;");
                        }

                        cb.AddBlankLine();

                        // Action aliases
                        foreach (var alias in action.Aliases)
                        {
                            using (cb.AddBlock($"case \"{alias.ToLower()}\":"))
                            {
                                cb.AppendLine(actionCall);
                                cb.AppendLine("break;");
                            }

                            cb.AddBlankLine();
                        }
                    }

                    // Default case: try primary action or show error
                    using (cb.AddBlock("default:"))
                    {
                        GeneratePrimaryMethodInvocationFromInfo(cb, compilation, commandInfo, throwOnNoPrimary: false);

                        // Build list of available subcommands and actions for suggestions
                        cb.AppendLine("var availableOptions = new List<string>();");

                        // Add subcommands
                        foreach (var subcommand in commandInfo.Subcommands)
                        {
                            cb.AppendLine($"availableOptions.Add(\"{subcommand.CommandName!.ToLower()}\");");
                            foreach (var alias in subcommand.Aliases)
                            {
                                cb.AppendLine($"availableOptions.Add(\"{alias.ToLower()}\");");
                            }
                        }

                        // Add actions
                        foreach (var action in commandInfo.Actions)
                        {
                            cb.AppendLine($"availableOptions.Add(\"{action.ActionName!.ToLower()}\");");
                            foreach (var alias in action.Aliases)
                            {
                                cb.AppendLine($"availableOptions.Add(\"{alias.ToLower()}\");");
                            }
                        }

                        cb.AppendLine("var suggestion = TeCLI.StringSimilarity.FindMostSimilar(subcommandOrAction, availableOptions.ToArray());");
                        using (cb.AddBlock("if (suggestion != null)"))
                        {
                            cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownActionWithSuggestion.Replace("\n", "\\n")}", subcommandOrAction, suggestion));""");
                        }
                        using (cb.AddBlock("else"))
                        {
                            cb.AppendLine($"""Console.WriteLine(string.Format("{ErrorMessages.UnknownAction}", subcommandOrAction));""");
                        }
                        cb.AppendLine("break;");
                    }
                }
            }
        }

        cb.AddBlankLine();
    }

    /// <summary>
    /// Generates primary method invocation from CommandSourceInfo
    /// </summary>
    private void GeneratePrimaryMethodInvocationFromInfo(CodeBuilder cb, Compilation compilation, CommandSourceInfo commandInfo, bool throwOnNoPrimary)
    {
        var primaryMethods = commandInfo.TypeSymbol!.GetMembersWithAttribute<IMethodSymbol, PrimaryAttribute>();

        int count = 0;
        if (primaryMethods != null)
        {
            foreach (var primaryMethod in primaryMethods)
            {
                if (count++ > 0)
                {
                    // Multiple primary attributes defined - use the first one
                    break;
                }
                else
                {
                    // Use this method as the primary action
                    var actionInvokeMethodName = $"{GetUniqueTypeName(commandInfo.TypeSymbol)}{primaryMethod.Name}";
                    cb.AppendLine(primaryMethod.MapAsync(
                            () => $"await Process{actionInvokeMethodName}Async(args, cancellationToken);",
                            () => $"Process{actionInvokeMethodName}(args, cancellationToken);"));
                }
            }
        }

        if (count == 0 && throwOnNoPrimary)
        {
            cb.AppendLine($"""throw new InvalidOperationException(string.Format("{ErrorMessages.NoPrimaryActionDefined}", "{commandInfo.CommandName}"));""");
        }
    }

    private string? GetCommandName(ClassDeclarationSyntax classDecl)
    {
        // Logic to extract command name from attributes
        var commandAttribute = classDecl.GetAttribute<CommandAttribute>();

        if (commandAttribute?.ArgumentList?.Arguments.Count > 0)
        {
            return commandAttribute.ArgumentList.Arguments.First().ToString().Trim('"');
        }
        return null;
    }

    private List<string> GetCommandAliases(Compilation compilation, ClassDeclarationSyntax classDecl)
    {
        var aliases = new List<string>();
        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);

        if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol classSymbol)
        {
            var commandAttr = classSymbol.GetAttribute<CommandAttribute>();
            if (commandAttr != null)
            {
                var aliasesArg = commandAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
                if (!aliasesArg.Value.IsNull && aliasesArg.Value.Kind == TypedConstantKind.Array)
                {
                    foreach (var value in aliasesArg.Value.Values)
                    {
                        if (value.Value is string alias)
                        {
                            aliases.Add(alias);
                        }
                    }
                }
            }
        }

        return aliases;
    }

    private void GeneratePrimaryMethodInvocation(CodeBuilder cb, Compilation compilation, ClassDeclarationSyntax classDecl, bool throwOnNoPrimary)
    {
        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);

        if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol classSymbol)
        {
            var primaryMethods = classSymbol.GetMembersWithAttribute<IMethodSymbol, PrimaryAttribute>();

            int count = 0;
            if (primaryMethods != null)
            {
                foreach (var primaryMethod in primaryMethods)
                {
                    if (count++ > 0)
                    {
                        // Multiple primary attributes defined - this should be reported as a diagnostic
                        // For now, we silently ignore additional primary attributes and use the first one
                        break;
                    }
                    else
                    {
                        // Use this method as the primary action
                        var actionInvokeMethodName = $"{GetUniqueTypeName(classDecl)}{primaryMethod.Name}";
                        cb.AppendLine(primaryMethod.MapAsync(
                                () => $"await Process{actionInvokeMethodName}Async(args);",
                                () => $"Process{actionInvokeMethodName}(args);"));
                    }
                }
            }

            if (count == 0 && throwOnNoPrimary)
            {
                cb.AppendLine($"""throw new InvalidOperationException(string.Format("{ErrorMessages.NoPrimaryActionDefined}", "{GetCommandName(classDecl)}"));""");
            }
        }
    }

    /// <summary>
    /// Recursively extracts command information including nested subcommands
    /// </summary>
    private CommandSourceInfo ExtractCommandInfo(Compilation compilation, INamedTypeSymbol typeSymbol, CommandSourceInfo? parent = null, int level = 0)
    {
        var commandInfo = new CommandSourceInfo
        {
            TypeSymbol = typeSymbol,
            Parent = parent,
            Level = level
        };

        // Extract command name and metadata from CommandAttribute
        var commandAttr = typeSymbol.GetAttribute<CommandAttribute>();
        if (commandAttr != null)
        {
            // Get command name from first constructor argument
            if (commandAttr.ConstructorArguments.Length > 0)
            {
                commandInfo.CommandName = commandAttr.ConstructorArguments[0].Value?.ToString();
            }

            // Get description from named argument
            var descArg = commandAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Description");
            if (!descArg.Value.IsNull)
            {
                commandInfo.Description = descArg.Value.Value?.ToString();
            }

            // Get aliases from named argument
            var aliasesArg = commandAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
            if (!aliasesArg.Value.IsNull && aliasesArg.Value.Kind == TypedConstantKind.Array)
            {
                foreach (var value in aliasesArg.Value.Values)
                {
                    if (value.Value is string alias)
                    {
                        commandInfo.Aliases.Add(alias);
                    }
                }
            }
        }

        // Extract command-level hooks
        ExtractHooksFromSymbol(typeSymbol, commandInfo.BeforeExecuteHooks, commandInfo.AfterExecuteHooks, commandInfo.OnErrorHooks);

        // Extract actions from methods with ActionAttribute
        var actionMethods = typeSymbol.GetMembersWithAttribute<IMethodSymbol, ActionAttribute>();
        if (actionMethods != null)
        {
            foreach (var method in actionMethods)
            {
                var actionInfo = new ActionSourceInfo
                {
                    Method = method
                };

                var actionAttr = method.GetAttribute<ActionAttribute>();
                if (actionAttr != null)
                {
                    // Get action name from first constructor argument
                    if (actionAttr.ConstructorArguments.Length > 0)
                    {
                        actionInfo.ActionName = actionAttr.ConstructorArguments[0].Value?.ToString();
                    }

                    // Get display name
                    var displayNameArg = actionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "DisplayName");
                    if (!displayNameArg.Value.IsNull)
                    {
                        actionInfo.DisplayName = displayNameArg.Value.Value?.ToString();
                    }

                    // Get aliases
                    var aliasesArg = actionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Aliases");
                    if (!aliasesArg.Value.IsNull && aliasesArg.Value.Kind == TypedConstantKind.Array)
                    {
                        foreach (var value in aliasesArg.Value.Values)
                        {
                            if (value.Value is string alias)
                            {
                                actionInfo.Aliases.Add(alias);
                            }
                        }
                    }

                    // Set invoker method name
                    actionInfo.InvokerMethodName = $"{GetUniqueTypeName(typeSymbol)}{method.Name}";
                }

                // Extract action-level hooks
                ExtractHooksFromSymbol(method, actionInfo.BeforeExecuteHooks, actionInfo.AfterExecuteHooks, actionInfo.OnErrorHooks);

                commandInfo.Actions.Add(actionInfo);
            }
        }

        // Recursively extract nested commands (nested classes with CommandAttribute)
        var nestedTypes = typeSymbol.GetTypeMembers();
        foreach (var nestedType in nestedTypes)
        {
            if (nestedType.GetAttribute<CommandAttribute>() != null)
            {
                var nestedCommandInfo = ExtractCommandInfo(compilation, nestedType, commandInfo, level + 1);
                commandInfo.Subcommands.Add(nestedCommandInfo);
            }
        }

        return commandInfo;
    }

    /// <summary>
    /// Builds a flat list of all commands in the hierarchy for easy iteration
    /// </summary>
    private List<CommandSourceInfo> FlattenCommandHierarchy(CommandSourceInfo rootCommand)
    {
        var result = new List<CommandSourceInfo> { rootCommand };

        foreach (var subcommand in rootCommand.Subcommands)
        {
            result.AddRange(FlattenCommandHierarchy(subcommand));
        }

        return result;
    }

    /// <summary>
    /// Builds command hierarchies from all top-level command classes
    /// </summary>
    private List<CommandSourceInfo> BuildCommandHierarchies(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> commandClasses)
    {
        var hierarchies = new List<CommandSourceInfo>();

        foreach (var commandClass in commandClasses)
        {
            var model = compilation.GetSemanticModel(commandClass.SyntaxTree);
            if (model.GetDeclaredSymbol(commandClass) is INamedTypeSymbol typeSymbol)
            {
                var commandInfo = ExtractCommandInfo(compilation, typeSymbol);
                hierarchies.Add(commandInfo);
            }
        }

        return hierarchies;
    }

    /// <summary>
    /// Extracts global options information from a class marked with [GlobalOptions]
    /// </summary>
    private GlobalOptionsSourceInfo ExtractGlobalOptionsInfo(Compilation compilation, ClassDeclarationSyntax classDecl)
    {
        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
        var typeSymbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

        var globalOptions = new GlobalOptionsSourceInfo
        {
            TypeSymbol = typeSymbol,
            TypeName = typeSymbol?.Name,
            FullTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Namespace = typeSymbol?.ContainingNamespace?.ToDisplayString()
        };

        if (typeSymbol == null)
            return globalOptions;

        // Extract all properties with [Option] attribute
        var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>();
        foreach (var property in properties)
        {
            var optionAttr = property.GetAttribute<OptionAttribute>();
            if (optionAttr != null)
            {
                var paramInfo = new ParameterSourceInfo
                {
                    Name = property.Name,
                    DisplayType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Required = !(property.NullableAnnotation == NullableAnnotation.Annotated || property.Type.IsValueType == false),
                    ParameterType = ParameterType.Option,
                    SpecialType = property.Type.SpecialType
                };

                // Detect collection types
                if (property.Type is INamedTypeSymbol namedType && namedType.IsGenericType)
                {
                    var typeArgs = namedType.TypeArguments;
                    if (typeArgs.Length == 1)
                    {
                        paramInfo.IsCollection = true;
                        paramInfo.ElementType = typeArgs[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        paramInfo.ElementSpecialType = typeArgs[0].SpecialType;
                    }
                }

                // Detect enum types
                if (property.Type.TypeKind == TypeKind.Enum)
                {
                    paramInfo.IsEnum = true;
                    paramInfo.DisplayType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }

                // Extract option name, short name, required, envvar from attribute
                var optionName = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Name").Value;
                if (optionName.IsNull && optionAttr.ConstructorArguments.Length > 0)
                {
                    optionName = optionAttr.ConstructorArguments[0];
                }
                paramInfo.Name = optionName.IsNull ? property.Name : optionName.Value?.ToString() ?? property.Name;

                var shortName = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "ShortName").Value;
                paramInfo.ShortName = !shortName.IsNull && shortName.Value is char ch ? ch : '\0';

                var required = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "Required").Value;
                if (!required.IsNull && required.Value is bool isRequired)
                {
                    paramInfo.Required = isRequired;
                }

                var envVar = optionAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "EnvVar").Value;
                if (!envVar.IsNull && envVar.Value is string envVarName)
                {
                    paramInfo.EnvVar = envVarName;
                }

                bool isBoolean = property.Type.SpecialType == SpecialType.System_Boolean;
                if (isBoolean)
                {
                    paramInfo.IsSwitch = true;
                }

                // Extract validation and custom converter info
                ParameterInfoExtractor.ExtractValidationInfo(paramInfo, property);
                ParameterInfoExtractor.ExtractCustomConverterInfo(paramInfo, property);

                globalOptions.Options.Add(paramInfo);
            }
        }

        return globalOptions;
    }

    /// <summary>
    /// Extracts hook attributes from a symbol (command class or action method)
    /// </summary>
    private void ExtractHooksFromSymbol(
        ISymbol symbol,
        List<HookInfo> beforeExecuteHooks,
        List<HookInfo> afterExecuteHooks,
        List<HookInfo> onErrorHooks)
    {
        var attributes = symbol.GetAttributes();

        // Extract BeforeExecute hooks
        foreach (var attr in attributes.Where(a => a.AttributeClass?.Name == "BeforeExecuteAttribute"))
        {
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is INamedTypeSymbol hookType)
            {
                var hookInfo = new HookInfo
                {
                    HookTypeName = hookType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                };

                // Get Order property if specified
                var orderArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "Order");
                if (!orderArg.Value.IsNull && orderArg.Value.Value is int order)
                {
                    hookInfo.Order = order;
                }

                beforeExecuteHooks.Add(hookInfo);
            }
        }

        // Extract AfterExecute hooks
        foreach (var attr in attributes.Where(a => a.AttributeClass?.Name == "AfterExecuteAttribute"))
        {
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is INamedTypeSymbol hookType)
            {
                var hookInfo = new HookInfo
                {
                    HookTypeName = hookType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                };

                // Get Order property if specified
                var orderArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "Order");
                if (!orderArg.Value.IsNull && orderArg.Value.Value is int order)
                {
                    hookInfo.Order = order;
                }

                afterExecuteHooks.Add(hookInfo);
            }
        }

        // Extract OnError hooks
        foreach (var attr in attributes.Where(a => a.AttributeClass?.Name == "OnErrorAttribute"))
        {
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is INamedTypeSymbol hookType)
            {
                var hookInfo = new HookInfo
                {
                    HookTypeName = hookType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                };

                // Get Order property if specified
                var orderArg = attr.NamedArguments.FirstOrDefault(arg => arg.Key == "Order");
                if (!orderArg.Value.IsNull && orderArg.Value.Value is int order)
                {
                    hookInfo.Order = order;
                }

                onErrorHooks.Add(hookInfo);
            }
        }

        // Sort hooks by order
        beforeExecuteHooks.Sort((a, b) => a.Order.CompareTo(b.Order));
        afterExecuteHooks.Sort((a, b) => a.Order.CompareTo(b.Order));
        onErrorHooks.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    /// <summary>
    /// Gets a unique type name for a command that includes containing types for nested classes.
    /// This prevents method name collisions when different commands have nested classes with the same name.
    /// </summary>
    private static string GetUniqueTypeName(INamedTypeSymbol? typeSymbol)
    {
        if (typeSymbol == null)
        {
            return string.Empty;
        }

        var typeNameParts = new List<string>();
        var currentType = typeSymbol;
        while (currentType != null)
        {
            typeNameParts.Insert(0, currentType.Name);
            currentType = currentType.ContainingType;
        }
        return string.Join("_", typeNameParts);
    }

    /// <summary>
    /// Gets a unique type name from a ClassDeclarationSyntax by walking up the parent syntax nodes.
    /// This is used for legacy code paths that work with syntax rather than symbols.
    /// </summary>
    private static string GetUniqueTypeName(ClassDeclarationSyntax classDecl)
    {
        var typeNameParts = new List<string>();
        SyntaxNode? currentNode = classDecl;

        while (currentNode != null)
        {
            if (currentNode is ClassDeclarationSyntax classSyntax)
            {
                typeNameParts.Insert(0, classSyntax.Identifier.Text);
            }
            currentNode = currentNode.Parent;
        }

        return string.Join("_", typeNameParts);
    }

}
