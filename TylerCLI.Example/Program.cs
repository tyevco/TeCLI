// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using TylerCLI;

Console.WriteLine("Hello world!");

IServiceCollection services = new ServiceCollection();
services.AddCommandDispatcher();

var sp = services.BuildServiceProvider();

var dispatcher = sp.GetRequiredService<CommandDispatcher>();
dispatcher.Dispatch(args);
