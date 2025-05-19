using Xunit;
using SimEngine.Nodes;
using SimEngine.Core;
using System;

public class DispatcherNodeTests
{
    [Fact]
    public void ScheduleInitialArrival_ShouldCreateEntity_AndDispatchToQueue()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSimulationParameters(10, 1);

        engine.CreateDispatcherNode("Shop.Entry", () => 1.0);
        engine.CreateQueueNode("Shop.Queue", 1, 5, () => 1.0);
        engine.ConnectNode("Shop.Entry", "Shop.Queue");

        engine.RunSimulation();

        var entities = engine.GetEntities();
        Assert.NotEmpty(entities);
        Assert.True(entities[0].ServiceTimesInQueues.Count > 0);
    }

    [Fact]
    public void ScheduleInitialArrival_ShouldRespectNextNodeChoices()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSimulationParameters(10, 1);

        engine.CreateDispatcherNode("Shop.Entry", () => 1.0);
        engine.CreateQueueNode("Shop.Q1", 1, 5, () => 1.0);
        engine.CreateQueueNode("Shop.Q2", 1, 5, () => 1.0);

        engine.ConnectNode("Shop.Entry", "Shop.Q1", 0.3);
        engine.ConnectNode("Shop.Entry", "Shop.Q2", 0.7);

        engine.RunSimulation();

        var entities = engine.GetEntities();
        Assert.NotEmpty(entities);
        Assert.True(entities[0].ServiceTimesInQueues.Count > 0);
    }

}