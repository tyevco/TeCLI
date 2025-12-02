// Example using Autofac for dependency injection

using Autofac;
using TeCLI;

// Build the Autofac container with TeCLI commands
var builder = new ContainerBuilder();
builder.AddCommandDispatcher();

var container = builder.Build();

// Resolve the dispatcher and run
var dispatcher = container.Resolve<CommandDispatcher>();
var exitCode = await dispatcher.DispatchAsync(args);

// Return the exit code to the calling process
return exitCode;
