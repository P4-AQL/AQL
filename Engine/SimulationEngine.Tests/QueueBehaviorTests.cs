using Xunit;
using SimEngine.Core;
using System.Linq;

namespace SimulationEngine.Tests;

public class QueueBehaviorTests
{
    [Fact]
    public void Queue_Should_Drop_Entities_When_OverCapacity()
    {
        var engine = new SimulationEngineAPI();
        engine.CreateDispatcherNode("Net.D1", () => 0.1);  // Fast arrival
        engine.CreateQueueNode("Net.Q1", 1, 2, () => 5.0); // Small capacity
        engine.ConnectNode("Net.D1", "Net.Q1");

        engine.SetSimulationParameters(10, 1);
        engine.RunSimulation();

        var stats = engine.GetSimulationStats();
        var qStats = stats.QueueStats.First(q => q.Name == "Net.Q1");

        Assert.True(qStats.DroppedEntities > 0);
        Assert.InRange(qStats.DropRate, 0.01, 1.0);
    }
}
