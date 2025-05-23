


namespace Interpreter.SemanticAnalysis;

public class Simulation(Network queueable)
{
    public Network Root { get; } = queueable;
    public int Time { get; private set; } = 0;

    PriorityQueue<Event, int> eventQueue = new();


    public void StartSimulations(int until, int runs)
    {
        for (int i = 0; i < runs; i++)
        {
            StartSimulation(until);
        }
    }

    public void StartSimulation(int until)
    {
        Time = 0;
        while (Time < until)
        {
            // Simulate the next event
            SimulateNextEvent();
        }
    }


    private void SimulateNextEvent()
    {
        if (eventQueue.Count == 0)
        {
            // No more events to process
            return;
        }

        // Dequeue the next event
        Event @event = eventQueue.Dequeue();

        // Execute the action
        Time = @event.Time;
        @event.Action.Invoke();
    }

    public void ScheduleEvent(Event @event)
    {
        // Schedule an event to be executed at a specific time
        eventQueue.Enqueue(@event, @event.Time);
    }

    public void ScheduleEvent(int time, Action action)
    {
        // Schedule an event to be executed at a specific time
        eventQueue.Enqueue(new Event(time, action), time);
    }

    public void DispatchEntity(IReadOnlyList<FuncRoute> routes)
    {
        // Select a route based on the weights
        double totalWeight = 0;
        foreach (var route in routes)
        {
            totalWeight += route.ToProbabilityPair.Weight;
        }
        double randomValue = new Random().NextDouble() * totalWeight;

        double cumulativeWeight = 0;
        FuncRoute selectedRoute = routes[0];
        foreach (var route in routes)
        {
            cumulativeWeight += route.ToProbabilityPair.Weight;
            if (randomValue <= cumulativeWeight)
            {
                selectedRoute = route;
                break;
            }
        }

        // Dispatch an entity to the next event
        int arrivalTime = (int)(selectedRoute.FromRate.Invoke() + Time);
        NetworkEntity entity = selectedRoute.ToProbabilityPair.To;
        ScheduleEvent(arrivalTime, () => ArrivalEvent(entity));
    }

    public void ArrivalEvent(NetworkEntity entity)
    {

    }
}

public class Event(int time, Action action)
{
    public int Time { get; } = time;
    public Action Action { get; } = action;
}
