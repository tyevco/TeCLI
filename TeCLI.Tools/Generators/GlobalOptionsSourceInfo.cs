using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace TeCLI.Generators;

/// <summary>
/// Holds information about a global options class for code generation
/// </summary>
public class GlobalOptionsSourceInfo
{
    /// <summary>
    /// The type symbol for the global options class
    /// </summary>
    public INamedTypeSymbol? TypeSymbol { get; set; }

    /// <summary>
    /// Fully qualified name of the global options type
    /// </summary>
    public string? FullTypeName { get; set; }

    /// <summary>
    /// Simple name of the global options type
    /// </summary>
    public string? TypeName { get; set; }

    /// <summary>
    /// List of option properties in the global options class
    /// </summary>
    public List<ParameterSourceInfo> Options { get; set; } = new();

    /// <summary>
    /// The namespace containing the global options class
    /// </summary>
    public string? Namespace { get; set; }
}
