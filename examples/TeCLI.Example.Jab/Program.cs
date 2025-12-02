// Example using Jab for dependency injection

using TeCLI;

// Jab generates a CommandServiceProvider class with GetService<T>() method
var serviceProvider = new CommandServiceProvider();
var dispatcher = serviceProvider.GetService<CommandDispatcher>();

var exitCode = await dispatcher.DispatchAsync(args);

// Return the exit code to the calling process
return exitCode;
