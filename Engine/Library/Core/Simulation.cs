namespace SimEngine.Core;

using System;
using System.Collections.Generic;

public class Simulation
{
    private double _currentTime = 0;
    private readonly PriorityQueue<Event, double> _eventQueue = new();
    public double Now => _currentTime;

    public void Schedule(double delay, Action action)
    {
        var ev = new Event(_currentTime + delay, action);
        _eventQueue.Enqueue(ev, ev.Time);
    }

    public void Run(double untilTime)
    {
        while (_eventQueue.Count > 0)
        {
            var ev = _eventQueue.Peek();
            if (ev.Time > untilTime) break;
            _eventQueue.Dequeue();
            _currentTime = ev.Time;
            ev.Action();
        }
        _currentTime = untilTime;
    }
}