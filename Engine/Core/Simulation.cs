public class Simulation
{
    public double CurrentTime { get; private set; }
    private readonly EventQueue eventQueue = new();

    public void Schedule(double delay, Action action)
    {
        eventQueue.Schedule(new Event(CurrentTime + delay, action));
    }

    public void Run(double untilTime)
    {
        while (eventQueue.HasEvents())
        {
            var nextEvent = eventQueue.Next();

            if (nextEvent.Time > untilTime)
                break;

            CurrentTime = nextEvent.Time;
            nextEvent.Action();
        }
    }
}
