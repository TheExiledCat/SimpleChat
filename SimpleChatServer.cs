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
    public SimpleChatServer(int port)
    {
        m_Listener = new HttpListener();
        m_Listener.Prefixes.Add($"http://localhost:{port}/chat/");
        m_Listener.Prefixes.Add($"http://127.0.0.1:{port}/chat/");

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
        //messages are max 1024 bytes and are null terminated by default
        int maximumMessageLength = 1024;
        byte[] incoming = new byte[maximumMessageLength];
        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await webSocket.ReceiveAsync(incoming, CancellationToken.None);
                byte[] bytes = incoming.Take(result.Count).ToArray();
                ChatMessage message = ChatMessage.FromBuffer(bytes);
                message.Dump();
            }
            catch (WebSocketException we)
            {
                AnsiConsole.WriteException(we, ExceptionFormats.NoStackTrace);
            }
        }

    }
}
