
using Xunit;
using SimEngine.Networks;
using System;

namespace SimEngine.Tests;

public class NetworkDefinitionTests
{
    [Fact]
    public void NetworkDefinition_CanAddQueuesRoutesAndSubnets()
    {
        var def = new NetworkDefinition(null) { Name = "TestNet" };

        def.AddQueue("Prep", 1, 10, () => 1.0);
        def.AddEntryPoint("Start");
        def.AddExitPoint("End");
        def.Connect("Start", "Prep");
        def.Connect("Prep", "End", 0.5);

        var sub = new NetworkDefinition(def) { Name = "SubNet" };
        def.AddSubNetwork(sub);

        Assert.Single(def.Queues);
        Assert.Contains("Start", def.RouterEntries);
        Assert.Contains("End", def.RouterExits);
        Assert.Equal(2, def.Routes.Count);
        Assert.Single(def.SubNetworks);
    }
}

public class NetworkStatsTests
{
    [Fact]
    public void NetworkStats_RecordsEntriesAndExits()
    {
        var stats = new NetworkStats("Mall");
        var entity = new Core.Entity(0.0);

        stats.RecordEntry(entity, 1.0);
        stats.RecordExit(entity, 3.0); // Should not throw
    }
}
