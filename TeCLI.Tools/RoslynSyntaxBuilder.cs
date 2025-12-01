using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TeCLI;

/// <summary>
/// Fluent API wrapper around Roslyn's SyntaxFactory for building C# code.
/// </summary>
public class RoslynSyntaxBuilder
{
    private readonly List<UsingDirectiveSyntax> _usings = new();
    private readonly List<MemberDeclarationSyntax> _members = new();
    private string? _namespace;

    public RoslynSyntaxBuilder()
    {
    }

    public RoslynSyntaxBuilder(params string[] usings)
    {
        foreach (var ns in usings)
        {
            AddUsing(ns);
        }
    }

    /// <summary>
    /// Adds a using directive.
    /// </summary>
    public RoslynSyntaxBuilder AddUsing(string ns)
    {
        if (string.IsNullOrWhiteSpace(ns))
            return this;

        var value = ns.Trim();

        // Remove "using " prefix if present
        if (value.StartsWith("using ", StringComparison.Ordinal))
        {
            value = value.Substring(6);
        }

        // Remove trailing semicolon if present
        if (value.EndsWith(";"))
        {
            value = value.Substring(0, value.Length - 1);
        }

        if (!string.IsNullOrWhiteSpace(value) && !_usings.Any(u => u.Name?.ToString() == value))
        {
            _usings.Add(UsingDirective(ParseName(value)));
        }

        return this;
    }

    /// <summary>
    /// Sets the namespace for the generated code.
    /// </summary>
    public RoslynSyntaxBuilder WithNamespace(string ns)
    {
        _namespace = ns;
        return this;
    }

    /// <summary>
    /// Adds a member declaration (class, struct, etc.).
    /// </summary>
    public RoslynSyntaxBuilder AddMember(MemberDeclarationSyntax member)
    {
        _members.Add(member);
        return this;
    }

    /// <summary>
    /// Builds the complete compilation unit.
    /// </summary>
    public CompilationUnitSyntax Build()
    {
        var members = _members.ToArray();

        if (!string.IsNullOrEmpty(_namespace))
        {
            var namespaceDecl = FileScopedNamespaceDeclaration(ParseName(_namespace))
                .WithMembers(List(members));
            members = new MemberDeclarationSyntax[] { namespaceDecl };
        }

        return CompilationUnit()
            .WithUsings(List(_usings))
            .WithMembers(List(members))
            .NormalizeWhitespace();
    }

    /// <summary>
    /// Returns the generated source code as a string.
    /// </summary>
    public override string ToString()
    {
        return Build().ToFullString();
    }

    public static implicit operator string(RoslynSyntaxBuilder builder) => builder.ToString();
}

/// <summary>
/// Builder for creating class declarations with fluent API.
/// </summary>
public class ClassBuilder
{
    private readonly string _name;
    private readonly List<SyntaxToken> _modifiers = new();
    private readonly List<MemberDeclarationSyntax> _members = new();
    private readonly List<BaseTypeSyntax> _baseTypes = new();

    public ClassBuilder(string name)
    {
        _name = name;
    }

    public static ClassBuilder Create(string name) => new(name);

    public ClassBuilder AddModifier(SyntaxKind modifier)
    {
        _modifiers.Add(Token(modifier));
        return this;
    }

    public ClassBuilder Public() => AddModifier(SyntaxKind.PublicKeyword);
    public ClassBuilder Private() => AddModifier(SyntaxKind.PrivateKeyword);
    public ClassBuilder Internal() => AddModifier(SyntaxKind.InternalKeyword);
    public ClassBuilder Static() => AddModifier(SyntaxKind.StaticKeyword);
    public ClassBuilder Partial() => AddModifier(SyntaxKind.PartialKeyword);

    public ClassBuilder AddBaseType(string typeName)
    {
        _baseTypes.Add(SimpleBaseType(ParseTypeName(typeName)));
        return this;
    }

    public ClassBuilder AddMember(MemberDeclarationSyntax member)
    {
        _members.Add(member);
        return this;
    }

    public ClassBuilder AddField(string type, string name, SyntaxKind accessibility = SyntaxKind.PrivateKeyword, ExpressionSyntax? initializer = null)
    {
        var field = FieldDeclaration(
            VariableDeclaration(ParseTypeName(type))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier(name))
                        .WithInitializer(initializer != null ? EqualsValueClause(initializer) : null))))
            .WithModifiers(TokenList(Token(accessibility)));

        _members.Add(field);
        return this;
    }

    public ClassBuilder AddMethod(MethodDeclarationSyntax method)
    {
        _members.Add(method);
        return this;
    }

    public ClassDeclarationSyntax Build()
    {
        var classDecl = ClassDeclaration(_name)
            .WithModifiers(TokenList(_modifiers))
            .WithMembers(List(_members));

        if (_baseTypes.Count > 0)
        {
            classDecl = classDecl.WithBaseList(BaseList(SeparatedList(_baseTypes)));
        }

        return classDecl;
    }
}

/// <summary>
/// Builder for creating method declarations with fluent API.
/// </summary>
public class MethodBuilder
{
    private readonly string _name;
    private TypeSyntax _returnType = PredefinedType(Token(SyntaxKind.VoidKeyword));
    private readonly List<SyntaxToken> _modifiers = new();
    private readonly List<ParameterSyntax> _parameters = new();
    private BlockSyntax? _body;
    private bool _isAsync;

    public MethodBuilder(string name)
    {
        _name = name;
    }

    public static MethodBuilder Create(string name) => new(name);

    public MethodBuilder WithReturnType(string type)
    {
        _returnType = ParseTypeName(type);
        return this;
    }

    public MethodBuilder WithReturnType(TypeSyntax type)
    {
        _returnType = type;
        return this;
    }

    public MethodBuilder AddModifier(SyntaxKind modifier)
    {
        _modifiers.Add(Token(modifier));
        return this;
    }

    public MethodBuilder Public() => AddModifier(SyntaxKind.PublicKeyword);
    public MethodBuilder Private() => AddModifier(SyntaxKind.PrivateKeyword);
    public MethodBuilder Protected() => AddModifier(SyntaxKind.ProtectedKeyword);
    public MethodBuilder Internal() => AddModifier(SyntaxKind.InternalKeyword);
    public MethodBuilder Static() => AddModifier(SyntaxKind.StaticKeyword);

    public MethodBuilder Async()
    {
        _isAsync = true;
        _modifiers.Add(Token(SyntaxKind.AsyncKeyword));
        return this;
    }

    public MethodBuilder AddParameter(string type, string name, ExpressionSyntax? defaultValue = null)
    {
        var param = Parameter(Identifier(name))
            .WithType(ParseTypeName(type));

        if (defaultValue != null)
        {
            param = param.WithDefault(EqualsValueClause(defaultValue));
        }

        _parameters.Add(param);
        return this;
    }

    public MethodBuilder WithBody(BlockSyntax body)
    {
        _body = body;
        return this;
    }

    public MethodBuilder WithBody(params StatementSyntax[] statements)
    {
        _body = Block(statements);
        return this;
    }

    public MethodBuilder WithBody(IEnumerable<StatementSyntax> statements)
    {
        _body = Block(statements);
        return this;
    }

    public MethodDeclarationSyntax Build()
    {
        var method = MethodDeclaration(_returnType, Identifier(_name))
            .WithModifiers(TokenList(_modifiers))
            .WithParameterList(ParameterList(SeparatedList(_parameters)));

        if (_body != null)
        {
            method = method.WithBody(_body);
        }
        else
        {
            method = method.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        return method;
    }
}

/// <summary>
/// Builder for creating blocks of statements.
/// </summary>
public class BlockBuilder
{
    private readonly List<StatementSyntax> _statements = new();

    public static BlockBuilder Create() => new();

    public BlockBuilder AddStatement(StatementSyntax statement)
    {
        _statements.Add(statement);
        return this;
    }

    public BlockBuilder AddStatement(string statement)
    {
        _statements.Add(ParseStatement(statement));
        return this;
    }

    public BlockBuilder AddStatements(IEnumerable<StatementSyntax> statements)
    {
        _statements.AddRange(statements);
        return this;
    }

    public BlockBuilder AddLocalDeclaration(string type, string name, ExpressionSyntax? initializer = null)
    {
        var declaration = LocalDeclarationStatement(
            VariableDeclaration(ParseTypeName(type))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier(name))
                        .WithInitializer(initializer != null ? EqualsValueClause(initializer) : null))));

        _statements.Add(declaration);
        return this;
    }

    public BlockBuilder AddIf(ExpressionSyntax condition, BlockSyntax thenBlock, BlockSyntax? elseBlock = null)
    {
        var ifStatement = IfStatement(condition, thenBlock);
        if (elseBlock != null)
        {
            ifStatement = ifStatement.WithElse(ElseClause(elseBlock));
        }
        _statements.Add(ifStatement);
        return this;
    }

    public BlockBuilder AddIf(string condition, BlockSyntax thenBlock, BlockSyntax? elseBlock = null)
    {
        return AddIf(ParseExpression(condition), thenBlock, elseBlock);
    }

    public BlockBuilder AddSwitch(ExpressionSyntax expression, params SwitchSectionSyntax[] sections)
    {
        _statements.Add(SwitchStatement(expression, List(sections)));
        return this;
    }

    public BlockBuilder AddTryCatch(BlockSyntax tryBlock, BlockSyntax catchBlock, string? exceptionType = null, string? exceptionVarName = null)
    {
        var catchClause = CatchClause();

        if (exceptionType != null)
        {
            var catchDecl = CatchDeclaration(ParseTypeName(exceptionType));
            if (exceptionVarName != null)
            {
                catchDecl = catchDecl.WithIdentifier(Identifier(exceptionVarName));
            }
            catchClause = catchClause.WithDeclaration(catchDecl);
        }

        catchClause = catchClause.WithBlock(catchBlock);

        _statements.Add(TryStatement(tryBlock, SingletonList(catchClause), null));
        return this;
    }

    public BlockBuilder AddForEach(string type, string varName, ExpressionSyntax collection, BlockSyntax body)
    {
        _statements.Add(ForEachStatement(
            ParseTypeName(type),
            Identifier(varName),
            collection,
            body));
        return this;
    }

    public BlockBuilder AddFor(string initialization, string condition, string incrementor, BlockSyntax body)
    {
        // Parse the for loop components
        var forStatement = ForStatement(body)
            .WithDeclaration(ParseVariableDeclaration(initialization))
            .WithCondition(ParseExpression(condition))
            .WithIncrementors(SingletonSeparatedList<ExpressionSyntax>(ParseExpression(incrementor)));

        _statements.Add(forStatement);
        return this;
    }

    public BlockBuilder AddWhile(ExpressionSyntax condition, BlockSyntax body)
    {
        _statements.Add(WhileStatement(condition, body));
        return this;
    }

    public BlockBuilder AddReturn(ExpressionSyntax? expression = null)
    {
        _statements.Add(expression != null
            ? ReturnStatement(expression)
            : ReturnStatement());
        return this;
    }

    public BlockBuilder AddThrow(ExpressionSyntax expression)
    {
        _statements.Add(ThrowStatement(expression));
        return this;
    }

    public BlockBuilder AddBreak()
    {
        _statements.Add(BreakStatement());
        return this;
    }

    public BlockBuilder AddContinue()
    {
        _statements.Add(ContinueStatement());
        return this;
    }

    public BlockBuilder AddAwait(ExpressionSyntax expression)
    {
        _statements.Add(ExpressionStatement(AwaitExpression(expression)));
        return this;
    }

    public BlockBuilder AddInvocation(string expression)
    {
        _statements.Add(ExpressionStatement(ParseExpression(expression)));
        return this;
    }

    public BlockBuilder AddAssignment(string target, ExpressionSyntax value)
    {
        _statements.Add(ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                ParseExpression(target),
                value)));
        return this;
    }

    public BlockBuilder AddBlankLine()
    {
        // Add an empty statement with a leading newline trivia
        // This creates visual separation in the output
        if (_statements.Count > 0)
        {
            var lastStatement = _statements[_statements.Count - 1];
            _statements[_statements.Count - 1] = lastStatement.WithTrailingTrivia(
                lastStatement.GetTrailingTrivia().Add(CarriageReturnLineFeed));
        }
        return this;
    }

    public BlockSyntax Build()
    {
        return Block(_statements);
    }

    public IReadOnlyList<StatementSyntax> Statements => _statements;

    private static VariableDeclarationSyntax ParseVariableDeclaration(string declaration)
    {
        // Parse "int i = 0" style declarations
        var parts = declaration.Split(new[] { '=' }, 2);
        var typeAndName = parts[0].Trim().Split(' ');

        var type = string.Join(" ", typeAndName.Take(typeAndName.Length - 1));
        var name = typeAndName.Last();

        var variableDeclarator = VariableDeclarator(Identifier(name));

        if (parts.Length > 1)
        {
            variableDeclarator = variableDeclarator.WithInitializer(
                EqualsValueClause(ParseExpression(parts[1].Trim())));
        }

        return VariableDeclaration(ParseTypeName(type))
            .WithVariables(SingletonSeparatedList(variableDeclarator));
    }
}

/// <summary>
/// Extension methods for syntax building.
/// </summary>
public static class SyntaxBuilderExtensions
{
    /// <summary>
    /// Creates a Console.WriteLine statement.
    /// </summary>
    public static StatementSyntax ConsoleWriteLine(string text)
    {
        return ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("Console"),
                    IdentifierName("WriteLine")))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(
                Argument(LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(text)))))));
    }

    /// <summary>
    /// Creates a Console.WriteLine statement with an expression.
    /// </summary>
    public static StatementSyntax ConsoleWriteLine(ExpressionSyntax expression)
    {
        return ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("Console"),
                    IdentifierName("WriteLine")))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(expression)))));
    }

    /// <summary>
    /// Creates a switch section (case statement).
    /// </summary>
    public static SwitchSectionSyntax SwitchCase(string label, params StatementSyntax[] statements)
    {
        var stmts = statements.ToList();
        if (!stmts.Any(s => s is BreakStatementSyntax or ReturnStatementSyntax or ThrowStatementSyntax))
        {
            stmts.Add(BreakStatement());
        }

        return SwitchSection()
            .WithLabels(SingletonList<SwitchLabelSyntax>(
                CaseSwitchLabel(LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(label)))))
            .WithStatements(List(stmts));
    }

    /// <summary>
    /// Creates a default switch section.
    /// </summary>
    public static SwitchSectionSyntax SwitchDefault(params StatementSyntax[] statements)
    {
        var stmts = statements.ToList();
        if (!stmts.Any(s => s is BreakStatementSyntax or ReturnStatementSyntax or ThrowStatementSyntax))
        {
            stmts.Add(BreakStatement());
        }

        return SwitchSection()
            .WithLabels(SingletonList<SwitchLabelSyntax>(DefaultSwitchLabel()))
            .WithStatements(List(stmts));
    }

    /// <summary>
    /// Creates a string literal expression.
    /// </summary>
    public static ExpressionSyntax StringLiteral(string value)
    {
        return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
    }

    /// <summary>
    /// Creates a numeric literal expression.
    /// </summary>
    public static ExpressionSyntax NumericLiteral(int value)
    {
        return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
    }

    /// <summary>
    /// Creates a boolean literal expression.
    /// </summary>
    public static ExpressionSyntax BoolLiteral(bool value)
    {
        return value
            ? LiteralExpression(SyntaxKind.TrueLiteralExpression)
            : LiteralExpression(SyntaxKind.FalseLiteralExpression);
    }

    /// <summary>
    /// Creates a null literal expression.
    /// </summary>
    public static ExpressionSyntax NullLiteral()
    {
        return LiteralExpression(SyntaxKind.NullLiteralExpression);
    }

    /// <summary>
    /// Creates a throw statement with a new exception.
    /// </summary>
    public static StatementSyntax ThrowNew(string exceptionType, params ExpressionSyntax[] arguments)
    {
        return ThrowStatement(
            ObjectCreationExpression(ParseTypeName(exceptionType))
                .WithArgumentList(ArgumentList(SeparatedList(arguments.Select(Argument)))));
    }

    /// <summary>
    /// Creates an await expression statement.
    /// </summary>
    public static StatementSyntax AwaitStatement(string expression)
    {
        return ExpressionStatement(AwaitExpression(ParseExpression(expression)));
    }

    /// <summary>
    /// Parses a statement from a string.
    /// </summary>
    public static StatementSyntax ParseStatement(string code)
    {
        return SyntaxFactory.ParseStatement(code);
    }

    /// <summary>
    /// Parses an expression from a string.
    /// </summary>
    public static ExpressionSyntax ParseExpression(string code)
    {
        return SyntaxFactory.ParseExpression(code);
    }
}
