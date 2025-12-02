// Example using Jab for dependency injection

using TeCLI;

// Jab generates a CommandServiceProvider class with GetService<T>() method
var serviceProvider = new CommandServiceProvider();
var dispatcher = serviceProvider.GetService<CommandDispatcher>();

await dispatcher.DispatchAsync(args);
