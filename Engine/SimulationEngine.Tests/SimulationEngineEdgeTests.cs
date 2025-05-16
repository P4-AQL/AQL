namespace SimulationEngine.Tests;

using Xunit;
using SimEngine.Core;
using System.Linq;

public class SimulationEngineEdgeTests
{
    [Fact]
    public void Entity_Should_Have_Zero_Wait_If_Server_Is_Free()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(123);

        engine.CreateDispatcherNode("W.D1", () => 5.0);
        engine.CreateQueueNode("W.Q1", 1, 10, () => 2.0);
        engine.ConnectNode("W.D1", "W.Q1");

        engine.SetSimulationParameters(10, 1);
        engine.RunSimulation();

        var entity = engine.GetEntities().First();
        Assert.Equal(0, entity.WaitingTimesInQueues.First(), 1);  // Tolerance of 1 tick
    }

    [Fact]
    public void Dropped_Entities_Should_Not_Affect_Response_Time()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(999);

        engine.CreateDispatcherNode("D.D1", () => 0.1);
        engine.CreateQueueNode("D.Q1", 1, 1, () => 100.0);
        engine.ConnectNode("D.D1", "D.Q1");

        engine.SetSimulationParameters(150, 1);
        engine.RunSimulation();

        var network = engine.GetSimulationStats().NetworkStats.FirstOrDefault();

        Assert.NotNull(network);
        Assert.NotEmpty(network.RespondTimes); // We expect at least one response time recorded
        Assert.All(network.RespondTimes, rt => Assert.True(rt > 0));
    }


    [Fact]
    public void Simulation_Should_Aggregate_Stats_Across_Runs()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(321);

        engine.CreateDispatcherNode("M.D1", () => 1.0);
        engine.CreateQueueNode("M.Q1", 1, 10, () => 2.0);
        engine.ConnectNode("M.D1", "M.Q1");

        engine.SetSimulationParameters(50, 3);
        engine.RunSimulation();

        var stats = engine.GetSimulationStats().QueueStats.First();
        Assert.Equal(3, stats.RunCount);
    }

    [Fact]
    public void Throughput_Should_Equal_Served_Per_Run()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(777);
    
        engine.CreateDispatcherNode("T.D1", () => 1.0);
        engine.CreateQueueNode("T.Q1", 1, 10, () => 1.0);
        engine.ConnectNode("T.D1", "T.Q1");
    
        engine.SetSimulationParameters(30, 2);
        engine.RunSimulation();
    
        var stats = engine.GetSimulationStats().QueueStats.First();
        var expected = (double)stats.TotalServed / stats.RunCount;
    
        Assert.Equal(expected, stats.Throughput, 3);  // Allow small float error
    }

    [Fact]
    public void Tail_Probability_Should_Increase_When_Slow()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(2025);

        engine.CreateDispatcherNode("S.D1", () => 1.0);
        engine.CreateQueueNode("S.Q1", 1, 10, () => 10.0); // Slow server
        engine.ConnectNode("S.D1", "S.Q1");

        engine.SetSimulationParameters(60, 1);
        engine.RunSimulation();

        var net = engine.GetSimulationStats().NetworkStats.First();
        Assert.True(net.TailProbability(5.0) > 0.5);
    }
}   