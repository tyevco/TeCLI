// Simple TeCLI Example
// Demonstrates basic command-line parsing without dependency injection

using TeCLI;

var dispatcher = new CommandDispatcher();
var exitCode = await dispatcher.DispatchAsync(args);

// Return the exit code to the calling process
return exitCode;
