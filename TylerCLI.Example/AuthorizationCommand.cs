using TylerCLI.Attributes;

namespace TylerCLI.Example;

[Command("auth")]
public class AuthorizationCommand
{
    [Action("login", Description = "Login and validate user session.")]
    public Task LoginAsync([Argument] string username, [Option("password")] string password)
    {
        return Task.CompletedTask;
    }

    [Action("logout", Description = "Logout of user session.")]
    public Task LogoutAsync([Option("purgeData")] bool deleteUserData)
    {
        return Task.CompletedTask;
    }
}

