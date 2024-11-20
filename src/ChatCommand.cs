using System;

namespace SimpleChat;

public class ChatCommand : IChatCommand
{
    public virtual string GetOutput(string command)
    {
        return command;
    }
}
