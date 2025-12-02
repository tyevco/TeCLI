using TeCLI.Attributes;
using TeCLI.Tests.TestTypes;
using TeCLI.TypeConversion;

namespace TeCLI.Tests.TestCommands;

[Command("custom", Description = "Test command with custom type converters")]
public class CustomConverterCommand
{
    public static bool WasCalled { get; private set; }
    public static EmailAddress? CapturedEmail { get; private set; }
    public static PhoneNumber? CapturedPhone { get; private set; }
    public static EmailAddress? CapturedRecipient { get; private set; }
    public static PhoneNumber[]? CapturedContacts { get; private set; }
    public static EmailAddress? CapturedFromEnv { get; private set; }

    [Action("send")]
    public void Send(
        [Option("email")] [TypeConverter(typeof(EmailAddressConverter))] EmailAddress email,
        [Option("phone")] [TypeConverter(typeof(PhoneNumberConverter))] PhoneNumber? phone = null)
    {
        WasCalled = true;
        CapturedEmail = email;
        CapturedPhone = phone;
    }

    [Action("notify")]
    public void Notify(
        [Argument(Description = "Recipient email")] [TypeConverter(typeof(EmailAddressConverter))] EmailAddress recipient,
        [Option("contacts", ShortName = 'c')] [TypeConverter(typeof(PhoneNumberConverter))] PhoneNumber[]? contacts = null)
    {
        WasCalled = true;
        CapturedRecipient = recipient;
        CapturedContacts = contacts;
    }

    [Action("connect")]
    public void Connect(
        [Option("email", EnvVar = "USER_EMAIL")] [TypeConverter(typeof(EmailAddressConverter))] EmailAddress? email = null)
    {
        WasCalled = true;
        CapturedFromEnv = email;
    }

    public static void Reset()
    {
        WasCalled = false;
        CapturedEmail = null;
        CapturedPhone = null;
        CapturedRecipient = null;
        CapturedContacts = null;
        CapturedFromEnv = null;
    }
}
