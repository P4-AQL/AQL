
namespace SimEngine.Core;

using System;

public class Event
{
    public double Time { get; }
    public Action Action { get; }

    public Event(double time, Action action)
    {
        Time = time;
        Action = action;
    }
}