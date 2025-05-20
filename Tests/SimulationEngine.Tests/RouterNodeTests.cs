using Xunit;
using SimEngine.Nodes;
using SimEngine.Core;
using System;

public class RouterNodeTests
{
    [Fact]
    public void Route_ShouldChooseNextNode_FromChoices()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSimulationParameters(10, 1);

        engine.CreateQueueNode("Mall.Q1", 1, 5, () => 0.1);
        engine.CreateQueueNode("Mall.Q2", 1, 5, () => 0.1);
        var q1 = engine.GetQueueNode("Mall.Q1");
        var q2 = engine.GetQueueNode("Mall.Q2");

        var router = new RouterNode(engine, "Mall.Router");
        router.NextNodeChoices = new()
    {
        ((Node)q1, 0.3),
        ((Node)q2, 0.7)
    };

        var entity = new Entity(0);
        engine.RegisterEntity(entity);

        engine.RunSimulation();
        engine._simulation.Schedule(0, () => router.Route(entity));
        engine._simulation.Run(10);

        Console.WriteLine($"DEBUG: Routed ServiceTimes = {string.Join(", ", entity.ServiceTimesInQueues)}");
        Assert.True(entity.ServiceTimesInQueues.Count > 0);
    }
}
