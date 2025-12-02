// Example using Microsoft.Extensions.DependencyInjection

using Microsoft.Extensions.DependencyInjection;
using TeCLI;

IServiceCollection services = new ServiceCollection();
services.AddCommandDispatcher();

var sp = services.BuildServiceProvider();

var dispatcher = sp.GetRequiredService<CommandDispatcher>();
var exitCode = await dispatcher.DispatchAsync(args);

// Return the exit code to the calling process
return exitCode;
