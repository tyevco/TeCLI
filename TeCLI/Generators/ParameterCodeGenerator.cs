using Microsoft.CodeAnalysis;
using TeCLI.Generators;
using static TeCLI.Constants;

namespace TeCLI.Generators;

/// <summary>
/// Generates code for parsing command-line parameters
/// </summary>
internal static class ParameterCodeGenerator
{
    public static void GenerateParameterParsingCode(CodeBuilder cb, IMethodSymbol methodSymbol, ParameterSourceInfo sourceInfo, string variableName)
    {
        cb.AppendLine($"{sourceInfo.DisplayType} {variableName} = default;");
        if (sourceInfo.Optional)
        {
            cb.AppendLine($"bool {variableName}Set = false;");
        }

        using (cb.AddBlankScope())
        {
            if (sourceInfo.ParameterType == ParameterType.Option)
            {
                GenerateOptionSource(cb, methodSymbol, sourceInfo, variableName);
            }
            else if (sourceInfo.ParameterType == ParameterType.Argument)
            {
                GenerateArgumentSource(cb, methodSymbol, sourceInfo, variableName);
            }
            else if (sourceInfo.ParameterType == ParameterType.Container)
            {
                // Container parameters are handled by their nested properties
            }
        }
    }

    private static void GenerateOptionSource(CodeBuilder cb, IMethodSymbol methodSymbol, ParameterSourceInfo sourceInfo, string variableName)
    {
        if (sourceInfo.IsSwitch)
        {
            string switchCheck = sourceInfo.ShortName != '\0'
                ? $"args.Contains(\"-{sourceInfo.ShortName}\") || args.Contains(\"--{sourceInfo.Name}\")"
                : $"args.Contains(\"--{sourceInfo.Name}\")";

            cb.AppendLine($"{variableName} = {switchCheck};");
        }
        else
        {
            string optionCheck = sourceInfo.ShortName != '\0'
                ? $"arg == \"-{sourceInfo.ShortName}\" || arg == \"--{sourceInfo.Name}\""
                : $"arg == \"--{sourceInfo.Name}\"";

            cb.AppendLine($"var {variableName}Index = Array.FindIndex(args, arg => {optionCheck});");
            using (cb.AddBlock($"if ({variableName}Index != -1 && args.Length > {variableName}Index + 1)"))
            {
                using (var tb = cb.AddTry())
                {
                    cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType(args[{variableName}Index + 1], typeof({sourceInfo.DisplayType}));");
                    if (sourceInfo.Optional)
                    {
                        cb.AppendLine($"{variableName}Set = true;");
                    }
                    tb.Catch();
                    cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.InvalidOptionValue}", "{sourceInfo.Name}"));""");
                }
            }

            // if the field is required, we want to throw an exception if it is not provided.
            if (sourceInfo.Required)
            {
                using (cb.AddBlock("else"))
                {
                    cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredOptionNotProvided}", "{sourceInfo.Name}"));""");
                }
            }
        }
    }

    private static void GenerateArgumentSource(CodeBuilder cb, IMethodSymbol methodSymbol, ParameterSourceInfo sourceInfo, string variableName)
    {
        using (cb.AddBlock($"if (args.Length < {sourceInfo.ArgumentIndex + 1})"))
        {
            cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredArgumentNotProvided}", "{sourceInfo.Name}"));""");
        }
        using (cb.AddBlock("else"))
        {
            using (var tb = cb.AddTry())
            {
                cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType(args[{sourceInfo.ArgumentIndex}], typeof({sourceInfo.DisplayType}));");
                tb.Catch();
                cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.InvalidArgumentSyntax}", "{sourceInfo.Name}"));""");
            }
        }
    }
}
