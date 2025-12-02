// Example using Pure.DI for dependency injection

using TeCLI;

// Pure.DI generates a Composition class with the CommandDispatcher property
var composition = new Composition();
var dispatcher = composition.CommandDispatcher;

await dispatcher.DispatchAsync(args);
