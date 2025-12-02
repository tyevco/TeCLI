// Advanced TeCLI Example
// Demonstrates advanced features: validation, enums, collections,
// nested commands, aliases, environment variables, and more.

using TeCLI;

var dispatcher = new CommandDispatcher();
await dispatcher.DispatchAsync(args);
