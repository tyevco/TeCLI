// Example using SimpleInjector for dependency injection

using SimpleInjector;
using TeCLI;

// Create and configure the SimpleInjector container
var container = new Container();
container.AddCommandDispatcher();

// Verify the container configuration
container.Verify();

// Resolve the dispatcher and run
var dispatcher = container.GetInstance<CommandDispatcher>();
var exitCode = await dispatcher.DispatchAsync(args);

// Return the exit code to the calling process
return exitCode;
