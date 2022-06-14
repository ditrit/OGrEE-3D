using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AConnection 
{
    public readonly Queue<string> incomingQueue = new Queue<string>();
    public abstract void StartConnection(int _receivePort, int _sendPort);
    public abstract string[] GetMessages();
    public abstract void Send(string _message);
    public abstract void Stop();
}
