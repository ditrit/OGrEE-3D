using System;
using System.Collections.Generic;

public abstract class AConnection
{
    public readonly Queue<string> incomingQueue = new Queue<string>();
    public readonly Queue<Action> mainThreadQueue = new Queue<Action>();
    public abstract void StartConnection(int _cliPort);
    public abstract string[] GetMessages();
    public abstract void Send(string _message);
    public abstract void Stop();
}
