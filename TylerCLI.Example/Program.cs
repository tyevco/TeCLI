// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using TylerCLI;

IServiceCollection services = new ServiceCollection();
services.AddCommandDispatcher();

var sp = services.BuildServiceProvider();

var dispatcher = sp.GetRequiredService<CommandDispatcher>();
await dispatcher.DispatchAsync(args);
