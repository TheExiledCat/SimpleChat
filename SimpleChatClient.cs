using System;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using Dumpify;
using Spectre.Console;

namespace SimpleChat;

public class SimpleChatClient
{
    ClientWebSocket m_Client;
    string m_Ip = "";
    public SimpleChatClient()
    {
        m_Client = new ClientWebSocket();

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

            })
        );
        m_Client.Options.SetRequestHeader("Client", "SimpleChat");
        await m_Client.ConnectAsync(new Uri($"ws://{m_Ip}/chat/"), CancellationToken.None);
        while (true)
        {
            ChatMessage message = new ChatMessage(Console.ReadLine());
            await m_Client.SendAsync(message.Serialize(), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        await m_Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

    }
}
