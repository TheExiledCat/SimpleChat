using System;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using Dumpify;
using Spectre.Console;

namespace SimpleChat;

public class SimpleChatClient
{
    ClientWebSocket m_Client;
    User m_Loggedin;
    string m_Ip = "";
    public SimpleChatClient(User loggedin)
    {
        m_Client = new ClientWebSocket();
        m_Loggedin = loggedin;
    }
    public async Task Run()
    {
        Console.Clear();
        m_Ip = AnsiConsole.Prompt(
            new TextPrompt<string>("Please fill in the ip and port of the server (e.g. 127.0.0.1:12345)\n>")
            .Validate(url =>
            {
                string pattern = @"^((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])(:[0-9]{1,5})?$";
                bool correct = Regex.IsMatch(url, pattern);
                if (correct)
                {
                    return ValidationResult.Success();
                }
                return ValidationResult.Error("Incorrect format: please ensure valid ipv4 and port number");

            }).DefaultValue("127.0.0.1:3000")
        );

        m_Client.Options.SetRequestHeader("Client", "SimpleChat");
        await m_Client.ConnectAsync(new Uri($"ws://{m_Ip}/chat/"), CancellationToken.None);
        ChatScreen screen = new ChatScreen();

        await screen.Run();
        screen.OnSend += async (mes) =>
               {
                   ChatMessage messageToSend = new ChatMessage(mes, false);
                   messageToSend.Sender ??= m_Loggedin.Name ?? "Anonynmous";

                   await m_Client.SendAsync(messageToSend.Serialize(), WebSocketMessageType.Text, true, CancellationToken.None);
                   screen.ShowMessage(messageToSend);
               };
        Task.Run(() => screen.GetInput());

        WebSocketReceiveResult result;
        while (m_Client.State == WebSocketState.Open)
        {

            byte[] incoming = new byte[4096];
            result = await m_Client.ReceiveAsync(incoming, CancellationToken.None);
            byte[] bytes = incoming.Take(result.Count).ToArray();
            ChatMessage message = ChatMessage.FromBuffer(bytes);

            await screen.ShowMessage(message);

        }
        screen.Stop();
        Console.WriteLine("Connection closed...");
        return;

    }
}
