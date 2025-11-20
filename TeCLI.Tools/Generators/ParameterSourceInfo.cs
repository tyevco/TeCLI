using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace TeCLI.Generators
{
    public class ParameterSourceInfo
    {
        public SpecialType SpecialType { get; set; }
        public bool IsSwitch { get; set; }
        public bool Required { get; set; }
        public ParameterType ParameterType { get; set; }
        public char ShortName { get; set; }
        public TypedConstant Description { get; set; }
        public string? ParameterName { get; set; }
        public string? Name { get; set; }
        public SyntaxNode? Parent { get; set; }
        public int ArgumentIndex { get; set; }
        public IEnumerable<ParameterSourceInfo> Children { get; set; } = [];
        public int ParameterIndex { get; set; }
        public string? DisplayType { get; set; }
        public string? DefaultValue { get; set; }
        public bool IsCollection { get; set; }
        public string? ElementType { get; set; }
        public SpecialType ElementSpecialType { get; set; }
        public string? EnvVar { get; set; }

        // Enum support
        public bool IsEnum { get; set; }
        public bool IsFlags { get; set; }
        public bool IsElementEnum { get; set; }
        public bool IsElementFlags { get; set; }

        // Common convertible types support (Uri, DateTime, TimeSpan, etc.)
        public bool IsCommonType { get; set; }
        public bool IsElementCommonType { get; set; }
        public string? CommonTypeParseMethod { get; set; }
        public string? ElementCommonTypeParseMethod { get; set; }

        // Validation support
        public List<ValidationInfo> Validations { get; set; } = new();

        public bool Optional => !Required;
    }

    /// <summary>
    /// Information about a validation attribute applied to a parameter
    /// </summary>
    public class ValidationInfo
    {
        public string AttributeTypeName { get; set; } = string.Empty;
        public string ValidationCode { get; set; } = string.Empty;
    }

    public enum ParameterType
    {
        Unknown,
        Option,
        Argument,
        Container,
    }
}