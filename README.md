### Developer Documentation for TylerCLI

#### Introduction

TylerCLI is a source-generated CLI parsing library designed to simplify the development of command-line interfaces in .NET applications. It uses custom attributes to mark classes and methods as commands and subcommands, automatically generating the necessary parsing and dispatching logic.

#### Features

- **Command and Subcommand Handling:** Define commands and optional subcommands using attributes.
- **Argument Parsing:** Automatically parse command line arguments into primitive types or custom classes with parameterized constructors.
- **Help Generation:** Automatically generate help menus based on command and subcommand descriptions.

#### Getting Started

##### Installation

To use TylerCLI in your project, add a reference to the TylerCLI library. You can include it as a project in your solution or as a NuGet package if it is available in that form.

```bash
dotnet add package TylerCLI
```

##### Basic Setup

1. **Define Commands**

   Use the `CommandAttribute` to mark classes that represent CLI commands:

   ```csharp
   using TylerCLI;

   [Command("greet", Description = "Greets the user")]
   public class GreetCommand
   {
   }
   ```

2. **Define Actions**

   Use the `ActionAttribute` for methods that should be executed as part of a command:

   ```csharp
   [PrimaryAction(Description = "Say hello")]
   public void Hello(string name)
   {
       Console.WriteLine($"Hello, {name}!");
   }
   ```

##### Example Usage

To use the defined commands, ensure your application's entry point calls the generated `CommandDispatcher`:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CommandDispatcher.Dispatch(args);
    }
}
```

#### How to Define Commands

Commands are classes annotated with the `CommandAttribute`, which includes properties for the command name and an optional description. Each command can have multiple actions, which are methods annotated with `ActionAttribute`.

- **Command Attribute:** Marks a class as a command with a specific command name.
- **Action Attribute:** Marks methods within command classes to be callable actions. The `PrimaryAttribute` is used for the default action if no specific action is called.

#### Extending Functionality

TylerCLI is designed to be extensible:

- **Adding New Commands:** Simply create a new class with the `CommandAttribute` and define methods with `ActionAttribute`.
- **Custom Argument Parsers:** Implement custom parsing logic for complex types by defining a constructor or conversion method that takes a string argument.

#### FAQs

- **How do I handle optional arguments?**
  
  Use properties in your command classes with default values to handle optional arguments.

- **Can I override the help generation?**
  
  Currently, the help menu is automatically generated, but you can extend the dispatcher to customize help messages.

#### Contributing

Contributions are welcome! To contribute, please fork the repository, make your changes, and submit a pull request.

### Conclusion

This documentation provides the basic information necessary for developers to get started with TylerCLI, understand its structure, and begin integrating and extending it within their own projects. Make sure to expand on each section with more specific examples and detailed descriptions as needed to address all potential user concerns and use cases.
