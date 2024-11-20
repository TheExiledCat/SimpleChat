using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Dumpify;
using Spectre.Console;

namespace SimpleChat;

public class SimpleChatServer
{
    HttpListener m_Listener;
    User m_Loggedin;
    public SimpleChatServer(User loggedin, int port)
    {
        m_Listener = new HttpListener();
        m_Listener.Prefixes.Add($"http://localhost:{port}/chat/");
        m_Listener.Prefixes.Add($"http://127.0.0.1:{port}/chat/");
        m_Loggedin = loggedin;
    }
    public async Task Run()
    {
        Console.Clear();
        m_Listener.Start();
        Spinner defaultSpinner = Spinner.Known.Circle;
        Console.WriteLine("Server started");
        while (true)
        {
            HttpListenerContext context =
            await AnsiConsole
            .Status()
            .Spinner(defaultSpinner)
            .StartAsync("Waiting for incoming connection", async cliContext =>
            {
                return await m_Listener.GetContextAsync();
            });
            Console.WriteLine("Connection established");

            if (context.Request.Headers["Client"] == "SimpleChat" && context.Request.IsWebSocketRequest)
            {
                try
                {
                    WebSocket socket = await AnsiConsole
                    .Status()
                    .Spinner(defaultSpinner)
                    .StartAsync("Establishing socket...", async cliContext =>
                    {
                        HttpListenerWebSocketContext webcontext = await context.AcceptWebSocketAsync(null);
                        Console.WriteLine("Socket established");
                        return webcontext.WebSocket;
                    });
                    await StartChat(socket);

                }
                catch (Exception e)
                {
                    Console.WriteLine($"Something went wrong:");
                    AnsiConsole.WriteException(e, ExceptionFormats.NoStackTrace);
                    context.Response.Close();
                }



            }
            else
            {
                Console.WriteLine("Request was not a websocket request or made by a non SimpleChat client");
                context.Response.Close();
            }
        }
    }
    public async Task StartChat(WebSocket webSocket)
    {
        Console.WriteLine("Starting chat...");
        await Task.Delay(2000);
        Console.Clear();
        //messages are max 4096 bytes

        byte[] incoming = new byte[4096];
        ChatScreen screen = new ChatScreen();

        await screen.Run();
        screen.OnSend += async (mes) =>
               {
                   ChatMessage messageToSend = new ChatMessage(mes, false);
                   messageToSend.Sender ??= m_Loggedin.Name;
                   await webSocket.SendAsync(messageToSend.Serialize(), WebSocketMessageType.Text, true, CancellationToken.None);
                   screen.ShowMessage(messageToSend);
               };
        Task.Run(async () =>
        {
            screen.GetInput();


        });
        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            try
            {


                result = await webSocket.ReceiveAsync(incoming, CancellationToken.None);
                byte[] bytes = incoming.Take(result.Count).ToArray();
                ChatMessage message = ChatMessage.FromBuffer(bytes);

                await screen.ShowMessage(message);
            }
            catch (ArgumentException ae)
            {
                AnsiConsole.WriteLine("Error: Incoming message was too large");

            }
            catch (WebSocketException we)
            {
                AnsiConsole.WriteLine("Error: Incoming message failed");


            }
        }
        Console.WriteLine("Connection closed...");
        return;

    }
}
