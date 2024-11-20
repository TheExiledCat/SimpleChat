using System;
using System.Text;
using Dumpify;
using Spectre.Console;

namespace SimpleChat;

public class ChatMessage
{
    public const int MAXIMUM_MESSAGE_LENGTH = 1024;
    public const int MAXIMUM_SENDER_CHAR = 32;
    public const int MAXIMUM_SENDER_LENGTH = 2 * MAXIMUM_SENDER_CHAR;

    //maximum length: 4 bytes (value counts its own 4 bytes too)
    public int Length { get; set; }
    //maximum length: 64 bytes
    //char is 2 utf16 bytes so half
    string m_Sender = "Anonymous";
    public bool IsExternal { get; private set; }
    public string Sender
    {
        get => m_Sender; set
        {
            m_Sender = value;
        }
    }
    byte[] m_Message = [];
    public byte[] Message
    {
        get => m_Message; set
        {


            if (value.Length > MAXIMUM_MESSAGE_LENGTH)
            {
                m_Message = new ArraySegment<byte>(value, 0, MAXIMUM_MESSAGE_LENGTH).ToArray();
            }
            m_Message = value;
            Length = Message.Length + MAXIMUM_SENDER_LENGTH + 4;
        }
    }
    public string MessageString
    {
        get => Encoding.Unicode.GetString(m_Message);

    }
    public ChatMessage()
    {

    }
    public ChatMessage(string message, bool external = true)
    {
        Message = Encoding.Unicode.GetBytes(message);
        IsExternal = external;


    }
    public void Send()
    {

    }
    public Panel GetPanel(Color color, Justify justify = Justify.Center)
    {
        Markup message = new Markup(MessageString, new Style(foreground: color));
        message.Justify(justify);
        Panel panel = new Panel(message);

        panel.HeaderAlignment(justify);
        panel.Header(Sender + (IsExternal ? "" : "(You)"));
        panel.Padding(new Padding(5, 0));
        return panel;
    }
    public byte[] Serialize()
    {
        var buffer = new byte[Length];

        var writer = new BinaryWriter(new MemoryStream(buffer), Encoding.Unicode);
        writer.Write(Length);
        byte[] paddedSenderName = new byte[MAXIMUM_SENDER_LENGTH];
        byte[] sender = Encoding.Unicode.GetBytes(Sender);
        Array.Copy(sender, paddedSenderName, sender.Length);
        writer.Write(paddedSenderName);
        writer.Write(Message);
        return buffer;
    }
    public static ChatMessage FromBuffer(byte[] buffer)
    {
        ChatMessage message = new ChatMessage();

        message.Sender = Encoding.Unicode.GetString(new ArraySegment<byte>(buffer, 4, MAXIMUM_SENDER_LENGTH));
        message.Message = new ArraySegment<byte>(buffer, 4 + MAXIMUM_SENDER_LENGTH, buffer.Length - 4 - MAXIMUM_SENDER_LENGTH).ToArray();
        message.IsExternal = true;
        return message;
    }


}

