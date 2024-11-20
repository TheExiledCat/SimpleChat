using System;

namespace SimpleChat;

public interface IChatCommand
{

    public string GetOutput(string command);
}
