// Advanced TeCLI Example
// Demonstrates advanced features: validation, enums, collections,
// nested commands, aliases, environment variables, exit codes, and more.

using TeCLI;

var dispatcher = new CommandDispatcher();
var exitCode = await dispatcher.DispatchAsync(args);

// Return the exit code to the calling process
return exitCode;
