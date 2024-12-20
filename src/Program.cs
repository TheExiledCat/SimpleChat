﻿namespace SimpleChat;

using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Dumpify;
class Program
{
    static int Main(string[] args)
    {
        CommandApp<RunCommand> app = new CommandApp<RunCommand>();

        FigletText welcomeMessage = new FigletText("SimpleChat").Centered().Color(Color.Purple);
        AnsiConsole.Write(welcomeMessage);
        ProgramConfig.Load();
        return app.Run(args);




    }
}

class RunSettings : CommandSettings
{
    [CommandOption("-p|--port <PORT>")]
    [Description("The port to run the server on when being the server")]
    [DefaultValue(3000)]
    public int Port { get; set; }
    [Description("flag to immediately start as the server")]
    [CommandOption("--server")]
    [DefaultValue(false)]
    public bool Server { get; set; }
    [Description("flag to immediately start as the client")]
    [CommandOption("--client")]
    [DefaultValue(false)]
    public bool Client { get; set; }
    [Description("name of the user in messages")]
    [CommandOption("-n|--name <NAME>")]
    public string? Username { get; set; }
}
class RunCommand : AsyncCommand<RunSettings>
{


    public override async Task<int> ExecuteAsync(CommandContext context, RunSettings settings)
    {
        ProgramConfig.Config.Dump();
        User u = new User();
        u.Name = settings.Username;
        if (u.Name == null)
        {
            u.Name = AnsiConsole.Ask("What is your name?\n> ", "Anonymous");
        }
        SimpleChatServer server = new SimpleChatServer(u, settings.Port);
        SimpleChatClient client = new SimpleChatClient(u);
        Dictionary<string, Func<Task>> choices = new Dictionary<string, Func<Task>>();
        choices["Server"] = server.Run;
        choices["Client"] = client.Run;
        Func<Task>? run = null;
        if (settings.Server || settings.Client)
        {
            run = choices[settings.Server ? "Server" : "Client"];
        }
        else
        {
            SelectionPrompt<string> prompt =
            new SelectionPrompt<string>()
                .Title("Run as:")
                .AddChoices(choices.Keys);
            run = choices[AnsiConsole.Prompt(prompt)];
        }

        if (run != null)
        {
            await run();
        }
        else
        {
            Console.WriteLine("Something went wrong, closing...");
            return 1;
        }
        return 0;
    }
}