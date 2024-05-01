using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace TylerCLI.Generators
{
    internal class ParameterSourceInfo
    {
        public SpecialType SpecialType { get; set; }
        public bool IsSwitch { get; set; }
        public bool Required { get; set; }
        public ParameterType ParameterType { get; set; }
        public char ShortName { get; set; }
        public TypedConstant Description { get; set; }
        public string? Name { get; set; }
        public SyntaxNode? Parent { get; set; }
        public int ArgumentIndex { get; set; }
        public IEnumerable<ParameterSourceInfo> Children { get; set; } = [];
        public int ParameterIndex { get; set; }
        public string? DisplayType { get; set; }

        public bool Optional => !Required;
    }

    enum ParameterType
    {
        Unknown,
        Option,
        Argument,
        Container,
    }
}