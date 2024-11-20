using System;
using System.Text;
using Spectre.Console;

namespace SimpleChat;

public class ChatScreen
{
    public event Action<string>? OnSend;
    bool m_IsRunning = false;
    List<string> m_Chatlog;
    Table m_Root;
    Grid m_Chat;
    bool m_Updated = false;
    string m_CurrentInput = "";
    int m_LogIndex = -1;

    public async Task Run()
    {
        m_IsRunning = true;
        AnsiConsole.Clear();
        m_Root = new Table().Centered().Width(Console.LargestWindowWidth);
        m_Root.AddColumn(new TableColumn("Chat").Centered());
        m_Chat = new Grid().Centered().Width(m_Root.Width).AddColumns([
            new GridColumn().LeftAligned().Width(m_Root.Width/2).Alignment(Justify.Left),
            new GridColumn().RightAligned().Width(m_Root.Width/2).Alignment(Justify.Right)
        ]);
        m_Root.AddRow(m_Chat);
        m_Chatlog = [];
        Render();


    }
    public void GetInput()
    {
        while (m_IsRunning)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Backspace)
            {
                m_CurrentInput = m_CurrentInput.Substring(0, m_CurrentInput.Length - 1);
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                //send logic
                OnSend?.Invoke(m_CurrentInput);

                m_Chatlog.Add(m_CurrentInput);

                m_CurrentInput = "";
                m_LogIndex = -1;
            }
            else if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow)
            {
                m_LogIndex += (key.Key == ConsoleKey.UpArrow) ? 1 : -1;
                if (m_Chatlog.Count > 0)
                {

                    m_LogIndex = Math.Clamp(m_LogIndex, -1, m_Chatlog.Count);
                    m_CurrentInput = m_LogIndex >= 0 ? m_Chatlog[m_LogIndex] : "";
                }

            }
            else
            {
                m_CurrentInput += key.KeyChar;

            }
            Render();
        }

    }
    public async Task ShowMessage(ChatMessage message)
    {

        Panel p = message.GetPanel(Color.White, message.IsExternal ? Justify.Left : Justify.Right);
        if (!message.IsExternal)
        {
            m_Chat.AddRow(new Text(""), Align.Right(p));

        }
        else
        {
            m_Chat.AddRow(Align.Left(p), new Text(""));

        }

        Render();
    }
    void Render()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(m_Root);


        AnsiConsole.Write(new Markup("> " + m_CurrentInput));
    }
    public void Stop()
    {
        m_IsRunning = false;
        AnsiConsole.Clear();
    }
}
