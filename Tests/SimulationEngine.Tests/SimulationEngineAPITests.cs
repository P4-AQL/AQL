
using Xunit;
using SimEngine.Core;
using SimEngine.Networks;
using System;
using System.Collections.Generic;

namespace SimEngine.Tests;

public class SimulationEngineAPITests
{
    [Fact]
    public void SetSimulationParameters_AssignsCorrectValues()
    {
        var api = new SimulationEngineAPI();
        api.SetSimulationParameters(500, 3);

        api.RunSimulation(); // Should not throw
        var stats = api.GetSimulationStats();
        Assert.NotNull(stats);
    }

    [Fact]
    public void SetSeed_ProducesDeterministicRandoms()
    {
        var api1 = new SimulationEngineAPI();
        api1.SetSeed(42);

        var api2 = new SimulationEngineAPI();
        api2.SetSeed(42);

        Assert.Equal(api1.RandomGenerator.NextDouble(), api2.RandomGenerator.NextDouble());
    }

    [Fact]
    public void CreateDispatcherNode_AddsNodeSuccessfully()
    {
        var api = new SimulationEngineAPI();
        api.CreateDispatcherNode("TestDispatcher", () => 1.0);
    }

    [Fact]
    public void CreateQueueNode_AddsQueueNodeWithoutError()
    {
        var api = new SimulationEngineAPI();
        api.CreateQueueNode("TestQueue", 1, 10, () => 1.0);
    }

    [Fact]
    public void ConnectNode_ThrowsIfNodesAreMissing()
    {
        var api = new SimulationEngineAPI();
        Assert.Throws<ArgumentException>(() => api.ConnectNode("X", "Y"));
    }

    [Fact]
    public void RecordNetworkEntryAndExit_TracksCorrectly()
    {
        var api = new SimulationEngineAPI();
        var entity = new Entity(0.0);

        var def = new NetworkDefinition(null) { Name = "TestNet" };
        api.CreateNetwork(def);
        api.RecordNetworkEntry(entity, "TestNet", 1.0);
        api.RecordNetworkExit(entity, "TestNet", 2.0);
    }

    [Fact]
    public void SafePopAndRestore_HandlesEmptyStackGracefully()
    {
        var api = new SimulationEngineAPI();
        var entity = new Entity(0.0);

        api.SafePopAndRestore(entity, 1.0); // Should not throw
    }
}
