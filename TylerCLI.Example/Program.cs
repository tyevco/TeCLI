// See https://aka.ms/new-console-template for more information

using TylerCLI;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello world!");

IServiceCollection services = new ServiceCollection();
services.AddCommandDispatcher();

var sp = services.BuildServiceProvider();

var dispatcher = sp.GetRequiredService<CommandDispatcher>();
dispatcher.Dispatch(args);
