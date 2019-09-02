# Telegram Bot Framework

This is a simple project that can work like a framework for easily making Telegram Bots without the hassle of setting up an "skeleton" for your application.
This includes Dependency Injection, parameter matching and some route solving for your bot commands.

## Getting Started

To use this project, you need to clone the repo and reference it from another project since this repository is not published on NuGet

Then, in your project, you would need to create an Startup class for your bot on which you would register all the services required for your bot.
The Startup class must extend the `TelegramBotFramework.IBotStartup` interface for it to be valid.

Then, to run your bot from your Program class you need to create a new instance of `TelegramBotFramework.TelegramBotBuilder` and pass your bot API Token as a parameter (Probably this should be replaced with the use of a service or something so you can reference the API token from a configuration file).

After creating an instance of the builder, you can set it to use your Startup class and then build into a full bot (The build process is not really that of a build, but just a symbolical build)

``` csharp

_bot = new TelegramBotBuilder("<MY API TOKEN>")
    .UseStartup<MyBotStartup>()
    .Build();

```

Once your bot is built, you can just call the `Run()` method

### Controllers

Controller are automatically mapped on bot startup from the entry point assembly, and a controller should look like this:

``` csharp

using System.Threading.Tasks;
using TelegramBotFramework.Attributes;
using TelegramBotFramework.Controllers;

namespace MyBot.Controllers {

    [BotCommandController(name: "echo", description: "Make the bot say things")]
    public class EchoController : Controller {
        [BotCommand(usage: "Tell the bot a text to echo")]
        public async Task Echo([ParameterHelp("Text to echo")]string message) {
            await Bot.SendTextMessageAsync(Request.Message.Chat.Id, "You said: " + message);
        }

        public enum EchoActions {
            SayHello,
            SayBye
        }

        [BotCommand(usage: "Tell the bot to do something")]
        public async Task DoAction([ParameterHelp("Action to do")]EchoActions action) {
            switch (action) {
                case EchoActions.SayHello:
                    await Bot.SendTextMessageAsync(Request.Message.Chat.Id, "Hello!");
                    break;
                case EchoActions.SayBye:
                    await Bot.SendTextMessageAsync(Request.Message.Chat.Id, "Bye!");
                    break;
            }
        }
    }

}

```

The controller is a class that inherits from the `TelegramBotFramework.Controllers.Controller` class and is marked with the `TelegramBotFramework.Attributes.BotCommandController` attribute which actually activates this controller (You could comment out the attribute to disable the command), this attribute takes as parameters the name of the command (On which the received commands are compared to) and a description that is shown on the default help text of the framework (More on that later).

The methods that you want to expose as a "command variant" should be marked with the `TelegramBotFramework.Attributes.BotCommand` attribute, which tells the framework that this method can be called by the runtime (You can add extra methods that should not be called by users, but maybe are helper methods or extra functionality), the attribute takes the `usage` parameter which tells the framework what to show in the help command for this variant

Finally, the method that handles the command call defines the parameters it takes and each parameter can be optionally prefixed with the `TelegramBotFramework.Attributes.ParameterHelp` attribute to indicate a description for each parameter

The runtime will try to match the parameters sent in the command call from the user, to the variants set in each command controller and, if none is perfectly matched, the first variant that takes a string parameter will be used passing all the text after the command name:

For example, if we call the above controller with `/echo SayHello` or `/echo "SayHello"`, the DoAction variant will be matched, but, if we call the controller with `/echo SayHello World`, this will mean we want to use two parameters and since we haven't defined any variant that takes two parameters, the Echo command will be called with the string `SayHello World`, also, if we defined a variant that didn't take any parameters and removed the one that takes a string, the parameterless one would be called instead, the hierarchy goes as follows:

1. Try to call the variant on which all the parameters can be converted to the required parameters and types
2. Call the single parameter variant that takes a string (If it exists)
3. Call the parameterless variant (If it exists)
4. Fail the call, ignore it (Don't send a response) and log it

> Command variants are just a name for the methods part of a single command controller that allow this command to be called with different parameters, for example, you could define a variant that takes a single string parameter and another variant that takes a string and an enum (Yes, you can use enums as parameters)

> As a side note, optional parameters do not work properly with the framework at this moment, since, it would be complicated compared to ASP.NET Core on how to handle this sort of parameters because, here we do not have a name for each parameter, just a position

### Default help command

A help command is defined in the command handler of this framework and this handler sends information of all the controllers registered in the bots and the metadata set in the parameters of the attributes when a user asks for the /help command.

> Right now the design of the bot is not extensible enough to allow for proper a proper pipeline of messages, so, some changes would be need to be done on the framework for a custom help command. (Remember, pull requests are welcome)

### EF Core Support

If you have used database migration before in ASP.NET Core, you can still create an extension method to migrate the database before running your bot, just use a TelegramBot instead of IWebHost, also, you can define a BuildWebHost method in your program to allow for EF Core design tools to work with your bot

Something like:

``` csharp

// This "WebHost" will be used by EF Core design time tools :)
static object BuildWebHost(string[] args) =>
    BuildBot();

static TelegramBot BuildBot() =>
    new TelegramBotBuilder("<MY API TOKEN>")
        .UseStartup<MyBotStartup>()
        .Build();

```

and then, your extension method:

``` csharp

public static class TelegramBotExtensions {

    public static TelegramBot MigrateDatabase<TDatabase>(this TelegramBot host)
        where TDatabase : DbContext {
        using (var scope = host.Services.CreateScope()) {
            var db = scope.ServiceProvider.GetRequiredService<TDatabase>();
            db.Database.Migrate();
        }
        return host;
    }

}

```