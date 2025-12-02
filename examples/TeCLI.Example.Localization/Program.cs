using TeCLI;
using TeCLI.Localization;
using TeCLI.Example.Localization.Resources;

// Initialize localization with automatic culture detection
// This checks command line args (--lang, --locale) and environment variables (LANG, LC_ALL)
Localizer.Initialize(
    new ResourceLocalizationProvider(typeof(Strings)),
    args
);

// Check if help is requested - show localized help
if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h"))
{
    // Use the localized help renderer instead of the generated one
    Localizer.ShowHelp<GreetCommand>();
    return 0;
}

// Run the CLI normally
return CommandDispatcher.Run(args);
