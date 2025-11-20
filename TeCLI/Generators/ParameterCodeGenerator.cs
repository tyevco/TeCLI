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
        else if (sourceInfo.IsCollection)
        {
            // Generate collection parsing code
            GenerateCollectionOptionSource(cb, sourceInfo, variableName);
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
                    if (sourceInfo.IsEnum)
                    {
                        cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})System.Enum.Parse(typeof({sourceInfo.DisplayType}), args[{variableName}Index + 1], ignoreCase: true);");
                    }
                    else if (sourceInfo.IsCommonType && !string.IsNullOrEmpty(sourceInfo.CommonTypeParseMethod))
                    {
                        cb.AppendLine($"{variableName} = {string.Format(sourceInfo.CommonTypeParseMethod, $"args[{variableName}Index + 1]")};");
                    }
                    else
                    {
                        cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType(args[{variableName}Index + 1], typeof({sourceInfo.DisplayType}));");
                    }

                    if (sourceInfo.Optional)
                    {
                        cb.AppendLine($"{variableName}Set = true;");
                    }
                    tb.Catch();

                    if (sourceInfo.IsEnum)
                    {
                        cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.DisplayType})));""");
                        cb.AppendLine($"""throw new ArgumentException(string.Format("Invalid value '{{0}}' for option '--{sourceInfo.Name}'. Valid values are: {{1}}", args[{variableName}Index + 1], validValues));""");
                    }
                    else
                    {
                        cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.InvalidOptionValue}", "{sourceInfo.Name}"));""");
                    }
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

    private static void GenerateCollectionOptionSource(CodeBuilder cb, ParameterSourceInfo sourceInfo, string variableName)
    {
        string optionCheck = sourceInfo.ShortName != '\0'
            ? $"args[i] == \"-{sourceInfo.ShortName}\" || args[i] == \"--{sourceInfo.Name}\""
            : $"args[i] == \"--{sourceInfo.Name}\"";

        cb.AppendLine($"var {variableName}Values = new System.Collections.Generic.List<{sourceInfo.ElementType}>();");

        using (cb.AddBlock($"for (int i = 0; i < args.Length - 1; i++)"))
        {
            using (cb.AddBlock($"if ({optionCheck})"))
            {
                // Check if next arg is not another option
                using (cb.AddBlock($"if (i + 1 < args.Length && !args[i + 1].StartsWith(\"-\"))"))
                {
                    using (var tb = cb.AddTry())
                    {
                        cb.AppendLine($"// Support comma-separated values");
                        cb.AppendLine($"var values = args[i + 1].Split(',');");
                        using (cb.AddBlock($"foreach (var val in values)"))
                        {
                            cb.AppendLine($"var trimmedVal = val.Trim();");
                            using (cb.AddBlock($"if (!string.IsNullOrWhiteSpace(trimmedVal))"))
                            {
                                if (sourceInfo.IsElementEnum)
                                {
                                    cb.AppendLine($"{variableName}Values.Add(({sourceInfo.ElementType})System.Enum.Parse(typeof({sourceInfo.ElementType}), trimmedVal, ignoreCase: true));");
                                }
                                else if (sourceInfo.IsElementCommonType && !string.IsNullOrEmpty(sourceInfo.ElementCommonTypeParseMethod))
                                {
                                    cb.AppendLine($"{variableName}Values.Add({string.Format(sourceInfo.ElementCommonTypeParseMethod, "trimmedVal")});");
                                }
                                else
                                {
                                    cb.AppendLine($"{variableName}Values.Add(({sourceInfo.ElementType})Convert.ChangeType(trimmedVal, typeof({sourceInfo.ElementType})));");
                                }
                            }
                        }
                        tb.Catch();

                        if (sourceInfo.IsElementEnum)
                        {
                            cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.ElementType})));""");
                            cb.AppendLine($"""throw new ArgumentException(string.Format("Invalid value for option '--{sourceInfo.Name}'. Valid values are: {{0}}", validValues));""");
                        }
                        else
                        {
                            cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.InvalidOptionValue}", "{sourceInfo.Name}"));""");
                        }
                    }
                }
            }
        }

        // Convert list to final collection type
        using (cb.AddBlock($"if ({variableName}Values.Count > 0)"))
        {
            // Determine how to create the final collection
            if (sourceInfo.DisplayType!.Contains("[]"))
            {
                // Array type
                cb.AppendLine($"{variableName} = {variableName}Values.ToArray();");
            }
            else if (sourceInfo.DisplayType.Contains("List<"))
            {
                // List<T>
                cb.AppendLine($"{variableName} = {variableName}Values;");
            }
            else
            {
                // IEnumerable, ICollection, IList, etc. - use ToArray() and let the runtime handle the conversion
                cb.AppendLine($"{variableName} = {variableName}Values.ToArray();");
            }

            if (sourceInfo.Optional)
            {
                cb.AppendLine($"{variableName}Set = true;");
            }
        }

        // If required and no values were provided
        if (sourceInfo.Required)
        {
            using (cb.AddBlock("else"))
            {
                cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredOptionNotProvided}", "{sourceInfo.Name}"));""");
            }
        }
    }

    private static void GenerateArgumentSource(CodeBuilder cb, IMethodSymbol methodSymbol, ParameterSourceInfo sourceInfo, string variableName)
    {
        if (sourceInfo.IsCollection)
        {
            // For collection arguments, collect all remaining positional arguments starting from this index
            GenerateCollectionArgumentSource(cb, sourceInfo, variableName);
        }
        else
        {
            using (cb.AddBlock($"if (args.Length < {sourceInfo.ArgumentIndex + 1})"))
            {
                cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredArgumentNotProvided}", "{sourceInfo.Name}"));""");
            }
            using (cb.AddBlock("else"))
            {
                using (var tb = cb.AddTry())
                {
                    if (sourceInfo.IsEnum)
                    {
                        cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})System.Enum.Parse(typeof({sourceInfo.DisplayType}), args[{sourceInfo.ArgumentIndex}], ignoreCase: true);");
                    }
                    else if (sourceInfo.IsCommonType && !string.IsNullOrEmpty(sourceInfo.CommonTypeParseMethod))
                    {
                        cb.AppendLine($"{variableName} = {string.Format(sourceInfo.CommonTypeParseMethod, $"args[{sourceInfo.ArgumentIndex}]")};");
                    }
                    else
                    {
                        cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType(args[{sourceInfo.ArgumentIndex}], typeof({sourceInfo.DisplayType}));");
                    }
                    tb.Catch();

                    if (sourceInfo.IsEnum)
                    {
                        cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.DisplayType})));""");
                        cb.AppendLine($"""throw new ArgumentException(string.Format("Invalid value '{{0}}' for argument '{sourceInfo.Name}'. Valid values are: {{1}}", args[{sourceInfo.ArgumentIndex}], validValues));""");
                    }
                    else
                    {
                        cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.InvalidArgumentSyntax}", "{sourceInfo.Name}"));""");
                    }
                }
            }
        }
    }

    private static void GenerateCollectionArgumentSource(CodeBuilder cb, ParameterSourceInfo sourceInfo, string variableName)
    {
        cb.AppendLine($"var {variableName}Values = new System.Collections.Generic.List<{sourceInfo.ElementType}>();");

        // Collect all positional (non-option) arguments starting from the argument index
        cb.AppendLine($"int {variableName}ArgCount = 0;");
        using (cb.AddBlock("for (int i = 0; i < args.Length; i++)"))
        {
            using (cb.AddBlock("if (!args[i].StartsWith(\"-\"))"))
            {
                using (cb.AddBlock($"if ({variableName}ArgCount >= {sourceInfo.ArgumentIndex})"))
                {
                    using (var tb = cb.AddTry())
                    {
                        cb.AppendLine($"// Support comma-separated values in arguments too");
                        cb.AppendLine($"var values = args[i].Split(',');");
                        using (cb.AddBlock("foreach (var val in values)"))
                        {
                            cb.AppendLine($"var trimmedVal = val.Trim();");
                            using (cb.AddBlock("if (!string.IsNullOrWhiteSpace(trimmedVal))"))
                            {
                                if (sourceInfo.IsElementEnum)
                                {
                                    cb.AppendLine($"{variableName}Values.Add(({sourceInfo.ElementType})System.Enum.Parse(typeof({sourceInfo.ElementType}), trimmedVal, ignoreCase: true));");
                                }
                                else if (sourceInfo.IsElementCommonType && !string.IsNullOrEmpty(sourceInfo.ElementCommonTypeParseMethod))
                                {
                                    cb.AppendLine($"{variableName}Values.Add({string.Format(sourceInfo.ElementCommonTypeParseMethod, "trimmedVal")});");
                                }
                                else
                                {
                                    cb.AppendLine($"{variableName}Values.Add(({sourceInfo.ElementType})Convert.ChangeType(trimmedVal, typeof({sourceInfo.ElementType})));");
                                }
                            }
                        }
                        tb.Catch();

                        if (sourceInfo.IsElementEnum)
                        {
                            cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.ElementType})));""");
                            cb.AppendLine($"""throw new ArgumentException(string.Format("Invalid value for argument '{sourceInfo.Name}'. Valid values are: {{0}}", validValues));""");
                        }
                        else
                        {
                            cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.InvalidArgumentSyntax}", "{sourceInfo.Name}"));""");
                        }
                    }
                }
                cb.AppendLine($"{variableName}ArgCount++;");
            }
            using (cb.AddBlock("else"))
            {
                cb.AppendLine($"// Skip the option and its value (if it has one)");
                using (cb.AddBlock("if (i + 1 < args.Length && !args[i + 1].StartsWith(\"-\"))"))
                {
                    cb.AppendLine("i++; // Skip the option's value");
                }
            }
        }

        // Convert list to final collection type
        using (cb.AddBlock($"if ({variableName}Values.Count > 0)"))
        {
            // Determine how to create the final collection
            if (sourceInfo.DisplayType!.Contains("[]"))
            {
                // Array type
                cb.AppendLine($"{variableName} = {variableName}Values.ToArray();");
            }
            else if (sourceInfo.DisplayType.Contains("List<"))
            {
                // List<T>
                cb.AppendLine($"{variableName} = {variableName}Values;");
            }
            else
            {
                // IEnumerable, ICollection, IList, etc. - use ToArray() and let the runtime handle the conversion
                cb.AppendLine($"{variableName} = {variableName}Values.ToArray();");
            }
        }

        // If required and no values were provided
        if (sourceInfo.Required)
        {
            using (cb.AddBlock("else"))
            {
                cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredArgumentNotProvided}", "{sourceInfo.Name}"));""");
            }
        }
    }
}
