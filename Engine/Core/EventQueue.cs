using System.Collections.Generic;

public class EventQueue
{
    private readonly SortedSet<Event> events = new();

    public void Schedule(Event simEvent)
    {
        events.Add(simEvent);
    }

    public Event Next()
    {
        var nextEvent = events.Min;
        events.Remove(nextEvent);
        return nextEvent;
    }

    public bool HasEvents() => events.Count > 0;
}
