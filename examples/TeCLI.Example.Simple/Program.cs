// Simple TeCLI Example
// Demonstrates basic command-line parsing without dependency injection

using TeCLI;

var dispatcher = new CommandDispatcher();
await dispatcher.DispatchAsync(args);
