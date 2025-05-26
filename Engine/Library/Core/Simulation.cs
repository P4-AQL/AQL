namespace SimEngine.Core;

using System;
using System.Collections.Generic;

public class Simulation
{
    public SimulationEngineAPI? simulationEngineAPI;
    public int runNumber = 0;
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
        DateTime then = DateTime.Now;
        while (_eventQueue.Count > 0)
        {
            var ev = _eventQueue.Peek();
            if (ev.Time > untilTime) break;
            _eventQueue.Dequeue();
            _currentTime = ev.Time;
            ev.Action();

            DateTime now = DateTime.Now;
            if (then.AddSeconds(1) < now)
            {
                then = now;
                Console.WriteLine($"Running simulation {runNumber}... Time for this run: {Now}\n\tEntities in system: {simulationEngineAPI?._entities.Count ?? 0}");
            }
        }
        _currentTime = untilTime;
    }
}
