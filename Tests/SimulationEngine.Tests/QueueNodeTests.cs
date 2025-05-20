using Xunit;
using SimEngine.Nodes;
using SimEngine.Core;
using SimEngine.Metrics;
using System;

public class QueueNodeTests
{
    [Fact]
    public void ProcessDeparture_ShouldExitEntity_WhenNoNextNode()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSimulationParameters(10, 1);
        engine.CreateQueueNode("ExitQueue", 1, 5, () => 0.1);
        var queue = engine.GetQueueNode("ExitQueue");

        var entity = new Entity(0);
        engine.RegisterEntity(entity);

        engine.RunSimulation(); // Initializes engine + resets _simulation
        engine._simulation.Schedule(0, () => queue.ProcessArrival(entity));
        engine._simulation.Run(11); // Run new instance manually

        Console.WriteLine($"DEBUG: DepartureTime = {entity.DepartureTime}");
        Assert.True(entity.DepartureTime > 0);
    }

    [Fact]
    public void ProcessDeparture_ShouldSendToNextQueue()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSimulationParameters(10, 1);
        engine.CreateQueueNode("Q1", 1, 5, () => 0.1);
        engine.CreateQueueNode("Q2", 1, 5, () => 0.1);
        var q1 = engine.GetQueueNode("Q1");
        var q2 = engine.GetQueueNode("Q2");

        q1.NextNode = q2;

        var entity = new Entity(0);
        engine.RegisterEntity(entity);

        engine.RunSimulation();
        engine._simulation.Schedule(0, () => q1.ProcessArrival(entity));
        engine._simulation.Run(11);

        Console.WriteLine($"DEBUG: Service Count = {entity.ServiceTimesInQueues.Count}");
        Assert.True(entity.ServiceTimesInQueues.Count >= 2);
    }
}