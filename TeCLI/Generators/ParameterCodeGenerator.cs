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

            // Generate validation code
            GenerateValidationCode(cb, sourceInfo, variableName);
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

            // Environment variable fallback for boolean switches
            if (!string.IsNullOrEmpty(sourceInfo.EnvVar))
            {
                using (cb.AddBlock($"if (!{variableName})"))
                {
                    cb.AppendLine($"var {variableName}EnvValue = System.Environment.GetEnvironmentVariable(\"{sourceInfo.EnvVar}\");");
                    using (cb.AddBlock($"if (!string.IsNullOrEmpty({variableName}EnvValue))"))
                    {
                        cb.AppendLine($"{variableName} = bool.TryParse({variableName}EnvValue, out var {variableName}Parsed) && {variableName}Parsed;");
                    }
                }
            }
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
                    else if (sourceInfo.HasCustomConverter && !string.IsNullOrEmpty(sourceInfo.CustomConverterType))
                    {
                        cb.AppendLine($"var {variableName}Converter = new {sourceInfo.CustomConverterType}();");
                        cb.AppendLine($"{variableName} = {variableName}Converter.Convert(args[{variableName}Index + 1]);");
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

                    if (sourceInfo.HasCustomConverter)
                    {
                        // For custom converters, re-throw ArgumentException to preserve the converter's message
                        tb.Catch("(System.ArgumentException)");
                        cb.AppendLine("throw;");
                    }
                    else
                    {
                        tb.Catch();

                        if (sourceInfo.IsEnum)
                        {
                            cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.DisplayType})));""");
                            cb.AppendLine($$"""throw new ArgumentException(string.Format("Invalid value '{{0}}' for option '--{{sourceInfo.Name}}'. Valid values are: {{1}}", args[{{variableName}}Index + 1], validValues));""");
                        }
                        else
                        {
                            cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.InvalidOptionValue}", "{sourceInfo.Name}"));""");
                        }
                    }
                }
            }
            using (cb.AddBlock("else"))
            {
                // Environment variable fallback
                if (!string.IsNullOrEmpty(sourceInfo.EnvVar))
                {
                    cb.AppendLine($"var {variableName}EnvValue = System.Environment.GetEnvironmentVariable(\"{sourceInfo.EnvVar}\");");
                    using (cb.AddBlock($"if (!string.IsNullOrEmpty({variableName}EnvValue))"))
                    {
                        using (var tb = cb.AddTry())
                        {
                            if (sourceInfo.IsEnum)
                            {
                                cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})System.Enum.Parse(typeof({sourceInfo.DisplayType}), {variableName}EnvValue, ignoreCase: true);");
                            }
                            else if (sourceInfo.HasCustomConverter && !string.IsNullOrEmpty(sourceInfo.CustomConverterType))
                            {
                                cb.AppendLine($"var {variableName}Converter = new {sourceInfo.CustomConverterType}();");
                                cb.AppendLine($"{variableName} = {variableName}Converter.Convert({variableName}EnvValue);");
                            }
                            else if (sourceInfo.IsCommonType && !string.IsNullOrEmpty(sourceInfo.CommonTypeParseMethod))
                            {
                                cb.AppendLine($"{variableName} = {string.Format(sourceInfo.CommonTypeParseMethod, $"{variableName}EnvValue")};");
                            }
                            else
                            {
                                cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType({variableName}EnvValue, typeof({sourceInfo.DisplayType}));");
                            }

                            if (sourceInfo.Optional)
                            {
                                cb.AppendLine($"{variableName}Set = true;");
                            }

                            if (sourceInfo.HasCustomConverter)
                            {
                                // For custom converters, re-throw ArgumentException to preserve the converter's message
                                tb.Catch("(System.ArgumentException)");
                                cb.AppendLine("throw;");
                            }
                            else
                            {
                                tb.Catch();

                                if (sourceInfo.IsEnum)
                                {
                                    cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.DisplayType})));""");
                                    cb.AppendLine($$"""throw new ArgumentException(string.Format("Invalid value '{{0}}' from environment variable '{{sourceInfo.EnvVar}}' for option '--{{sourceInfo.Name}}'. Valid values are: {{1}}", {{variableName}}EnvValue, validValues));""");
                                }
                                else
                                {
                                    cb.AppendLine($$"""throw new ArgumentException(string.Format("Invalid value '{{0}}' from environment variable '{{sourceInfo.EnvVar}}' for option '--{{sourceInfo.Name}}'", {{variableName}}EnvValue));""");
                                }
                            }
                        }
                    }
                    // Check if we should prompt for the value
                    using (cb.AddBlock("else"))
                    {
                        if (!string.IsNullOrEmpty(sourceInfo.Prompt))
                        {
                            GeneratePromptCode(cb, sourceInfo, variableName);
                        }
                        else if (sourceInfo.Required)
                        {
                            cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredOptionNotProvided}", "{sourceInfo.Name}"));""");
                        }
                    }
                }
                else
                {
                    // No environment variable
                    if (!string.IsNullOrEmpty(sourceInfo.Prompt))
                    {
                        // Prompt for value
                        GeneratePromptCode(cb, sourceInfo, variableName);
                    }
                    else if (sourceInfo.Required)
                    {
                        // No prompt, but required - error
                        cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredOptionNotProvided}", "{sourceInfo.Name}"));""");
                    }
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
                                else if (sourceInfo.HasElementCustomConverter && !string.IsNullOrEmpty(sourceInfo.ElementCustomConverterType))
                                {
                                    cb.AppendLine($"var {variableName}ElementConverter = new {sourceInfo.ElementCustomConverterType}();");
                                    cb.AppendLine($"{variableName}Values.Add({variableName}ElementConverter.Convert(trimmedVal));");
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

                        if (sourceInfo.HasElementCustomConverter)
                        {
                            // For custom converters, re-throw ArgumentException to preserve the converter's message
                            tb.Catch("(System.ArgumentException)");
                            cb.AppendLine("throw;");
                        }
                        else
                        {
                            tb.Catch();

                            if (sourceInfo.IsElementEnum)
                            {
                                cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.ElementType})));""");
                                cb.AppendLine($$"""throw new ArgumentException(string.Format("Invalid value for option '--{{sourceInfo.Name}}'. Valid values are: {{0}}", validValues));""");
                            }
                            else
                            {
                                cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.InvalidOptionValue}", "{sourceInfo.Name}"));""");
                            }
                        }
                    }
                }
            }
        }

        // Try environment variable if no values from command line
        if (!string.IsNullOrEmpty(sourceInfo.EnvVar))
        {
            using (cb.AddBlock($"if ({variableName}Values.Count == 0)"))
            {
                cb.AppendLine($"var {variableName}EnvValue = System.Environment.GetEnvironmentVariable(\"{sourceInfo.EnvVar}\");");
                using (cb.AddBlock($"if (!string.IsNullOrEmpty({variableName}EnvValue))"))
                {
                    using (var tb = cb.AddTry())
                    {
                        cb.AppendLine($"// Parse comma-separated values from environment variable");
                        cb.AppendLine($"var envValues = {variableName}EnvValue.Split(',');");
                        using (cb.AddBlock($"foreach (var val in envValues)"))
                        {
                            cb.AppendLine($"var trimmedVal = val.Trim();");
                            using (cb.AddBlock($"if (!string.IsNullOrWhiteSpace(trimmedVal))"))
                            {
                                if (sourceInfo.IsElementEnum)
                                {
                                    cb.AppendLine($"{variableName}Values.Add(({sourceInfo.ElementType})System.Enum.Parse(typeof({sourceInfo.ElementType}), trimmedVal, ignoreCase: true));");
                                }
                                else if (sourceInfo.HasElementCustomConverter && !string.IsNullOrEmpty(sourceInfo.ElementCustomConverterType))
                                {
                                    cb.AppendLine($"var {variableName}ElementConverter = new {sourceInfo.ElementCustomConverterType}();");
                                    cb.AppendLine($"{variableName}Values.Add({variableName}ElementConverter.Convert(trimmedVal));");
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

                        if (sourceInfo.HasElementCustomConverter)
                        {
                            // For custom converters, re-throw ArgumentException to preserve the converter's message
                            tb.Catch("(System.ArgumentException)");
                            cb.AppendLine("throw;");
                        }
                        else
                        {
                            tb.Catch();

                            if (sourceInfo.IsElementEnum)
                            {
                                cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.ElementType})));""");
                                cb.AppendLine($$"""throw new ArgumentException(string.Format("Invalid value in environment variable '{{sourceInfo.EnvVar}}' for option '--{{sourceInfo.Name}}'. Valid values are: {{0}}", validValues));""");
                            }
                            else
                            {
                                cb.AppendLine($"""throw new ArgumentException(string.Format("Invalid value in environment variable '{sourceInfo.EnvVar}' for option '--{sourceInfo.Name}'"));""");
                            }
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
            // Find the positional argument by counting non-option args, skipping options and their values
            cb.AppendLine($"string? {variableName}RawValue = null;");
            cb.AppendLine($"int {variableName}ArgCount = 0;");
            using (cb.AddBlock("for (int i = 0; i < args.Length; i++)"))
            {
                using (cb.AddBlock("if (args[i].StartsWith(\"-\"))"))
                {
                    cb.AppendLine("// Skip the option and its value (if it has one)");
                    using (cb.AddBlock("if (i + 1 < args.Length && !args[i + 1].StartsWith(\"-\"))"))
                    {
                        cb.AppendLine("i++; // Skip the option's value");
                    }
                }
                using (cb.AddBlock("else"))
                {
                    using (cb.AddBlock($"if ({variableName}ArgCount == {sourceInfo.ArgumentIndex})"))
                    {
                        cb.AppendLine($"{variableName}RawValue = args[i];");
                        cb.AppendLine("break;");
                    }
                    cb.AppendLine($"{variableName}ArgCount++;");
                }
            }

            using (cb.AddBlock($"if ({variableName}RawValue == null)"))
            {
                // Check if we should prompt for the value
                if (!string.IsNullOrEmpty(sourceInfo.Prompt))
                {
                    GeneratePromptCode(cb, sourceInfo, variableName);
                }
                else
                {
                    cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.RequiredArgumentNotProvided}", "{sourceInfo.Name}"));""");
                }
            }
            using (cb.AddBlock("else"))
            {
                using (var tb = cb.AddTry())
                {
                    if (sourceInfo.IsEnum)
                    {
                        cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})System.Enum.Parse(typeof({sourceInfo.DisplayType}), {variableName}RawValue, ignoreCase: true);");
                    }
                    else if (sourceInfo.HasCustomConverter && !string.IsNullOrEmpty(sourceInfo.CustomConverterType))
                    {
                        cb.AppendLine($"var {variableName}Converter = new {sourceInfo.CustomConverterType}();");
                        cb.AppendLine($"{variableName} = {variableName}Converter.Convert({variableName}RawValue);");
                    }
                    else if (sourceInfo.IsCommonType && !string.IsNullOrEmpty(sourceInfo.CommonTypeParseMethod))
                    {
                        cb.AppendLine($"{variableName} = {string.Format(sourceInfo.CommonTypeParseMethod, $"{variableName}RawValue")};");
                    }
                    else
                    {
                        cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType({variableName}RawValue, typeof({sourceInfo.DisplayType}));");
                    }

                    if (sourceInfo.HasCustomConverter)
                    {
                        // For custom converters, re-throw ArgumentException to preserve the converter's message
                        tb.Catch("(System.ArgumentException)");
                        cb.AppendLine("throw;");
                    }
                    else
                    {
                        tb.Catch();

                        if (sourceInfo.IsEnum)
                        {
                            cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.DisplayType})));""");
                            cb.AppendLine($$"""throw new ArgumentException(string.Format("Invalid value '{{0}}' for argument '{{sourceInfo.Name}}'. Valid values are: {{1}}", args[{{sourceInfo.ArgumentIndex}}], validValues));""");
                        }
                        else
                        {
                            cb.AppendLine($"""throw new ArgumentException(string.Format("{ErrorMessages.InvalidArgumentSyntax}", "{sourceInfo.Name}"));""");
                        }
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
                            cb.AppendLine($$"""throw new ArgumentException(string.Format("Invalid value for argument '{{sourceInfo.Name}}'. Valid values are: {{0}}", validValues));""");
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

    private static void GeneratePromptCode(CodeBuilder cb, ParameterSourceInfo sourceInfo, string variableName)
    {
        cb.AppendLine($"// Prompt for missing {(sourceInfo.ParameterType == ParameterType.Option ? "option" : "argument")}");
        cb.AppendLine($"System.Console.Write(\"{sourceInfo.Prompt}: \");");

        if (sourceInfo.SecurePrompt)
        {
            // For secure input, read character by character and mask with asterisks
            cb.AppendLine($"var {variableName}Input = new System.Text.StringBuilder();");
            using (cb.AddBlock("while (true)"))
            {
                cb.AppendLine($"var key = System.Console.ReadKey(intercept: true);");
                using (cb.AddBlock("if (key.Key == System.ConsoleKey.Enter)"))
                {
                    cb.AppendLine("System.Console.WriteLine();");
                    cb.AppendLine("break;");
                }
                using (cb.AddBlock($"else if (key.Key == System.ConsoleKey.Backspace && {variableName}Input.Length > 0)"))
                {
                    cb.AppendLine($"{variableName}Input.Length--;");
                    cb.AppendLine("System.Console.Write(\"\\b \\b\");");
                }
                using (cb.AddBlock("else if (!char.IsControl(key.KeyChar))"))
                {
                    cb.AppendLine($"{variableName}Input.Append(key.KeyChar);");
                    cb.AppendLine("System.Console.Write(\"*\");");
                }
            }
            cb.AppendLine($"var {variableName}PromptValue = {variableName}Input.ToString();");
        }
        else
        {
            cb.AppendLine($"var {variableName}PromptValue = System.Console.ReadLine();");
        }

        // Now parse the prompted value
        using (cb.AddBlock($"if (!string.IsNullOrEmpty({variableName}PromptValue))"))
        {
            using (var tb = cb.AddTry())
            {
                if (sourceInfo.IsEnum)
                {
                    cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})System.Enum.Parse(typeof({sourceInfo.DisplayType}), {variableName}PromptValue, ignoreCase: true);");
                }
                else if (sourceInfo.HasCustomConverter && !string.IsNullOrEmpty(sourceInfo.CustomConverterType))
                {
                    cb.AppendLine($"var {variableName}Converter = new {sourceInfo.CustomConverterType}();");
                    cb.AppendLine($"{variableName} = {variableName}Converter.Convert({variableName}PromptValue);");
                }
                else if (sourceInfo.IsCommonType && !string.IsNullOrEmpty(sourceInfo.CommonTypeParseMethod))
                {
                    cb.AppendLine($"{variableName} = {string.Format(sourceInfo.CommonTypeParseMethod, $"{variableName}PromptValue")};");
                }
                else
                {
                    cb.AppendLine($"{variableName} = ({sourceInfo.DisplayType})Convert.ChangeType({variableName}PromptValue, typeof({sourceInfo.DisplayType}));");
                }

                if (sourceInfo.Optional)
                {
                    cb.AppendLine($"{variableName}Set = true;");
                }

                if (sourceInfo.HasCustomConverter)
                {
                    // For custom converters, re-throw ArgumentException to preserve the converter's message
                    tb.Catch("(System.ArgumentException)");
                    cb.AppendLine("throw;");
                }
                else
                {
                    tb.Catch();

                    if (sourceInfo.IsEnum)
                    {
                        cb.AppendLine($"""var validValues = string.Join(", ", System.Enum.GetNames(typeof({sourceInfo.DisplayType})));""");
                        cb.AppendLine($$"""throw new ArgumentException(string.Format("Invalid value '{{0}}' for {{(sourceInfo.ParameterType == ParameterType.Option ? "option '--{{sourceInfo.Name}}'" : $"argument '{{sourceInfo.Name}}'")}}.  Valid values are: {{1}}", {{variableName}}PromptValue, validValues));""");
                    }
                    else
                    {
                        var errorMsg = sourceInfo.ParameterType == ParameterType.Option
                            ? $"\"Invalid value for option '--{sourceInfo.Name}'\""
                            : $"\"{ErrorMessages.InvalidArgumentSyntax}\", \"{sourceInfo.Name}\"";
                        cb.AppendLine($"throw new ArgumentException(string.Format({errorMsg}));");
                    }
                }
            }
        }
        using (cb.AddBlock("else"))
        {
            // Empty input
            if (sourceInfo.Required)
            {
                var errorMsg = sourceInfo.ParameterType == ParameterType.Option
                    ? $"\"{ErrorMessages.RequiredOptionNotProvided}\", \"{sourceInfo.Name}\""
                    : $"\"{ErrorMessages.RequiredArgumentNotProvided}\", \"{sourceInfo.Name}\"";
                cb.AppendLine($"throw new ArgumentException(string.Format({errorMsg}));");
            }
            // Otherwise, use default value (already set)
        }
    }

    private static void GenerateValidationCode(CodeBuilder cb, ParameterSourceInfo sourceInfo, string variableName)
    {
        if (sourceInfo.Validations.Count == 0)
        {
            return;
        }

        // For optional parameters, only validate if the value was set
        if (sourceInfo.Optional)
        {
            using (cb.AddBlock($"if ({variableName}Set)"))
            {
                foreach (var validation in sourceInfo.Validations)
                {
                    cb.AppendLine(string.Format(validation.ValidationCode, variableName) + ";");
                }
            }
        }
        else
        {
            // For required parameters or arguments, always validate
            foreach (var validation in sourceInfo.Validations)
            {
                cb.AppendLine(string.Format(validation.ValidationCode, variableName) + ";");
            }
        }
    }
}
