using Xunit;
using SimEngine.Core;
using System.Linq;

namespace SimulationEngine.Tests;

public class UtilizationLoadTests
{
    [Fact]
    public void Queue_Should_Be_Highly_Utilized_Under_High_Load()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(2024);

        engine.CreateDispatcherNode("U.D1", () => 0.2); // fast arrivals
        engine.CreateQueueNode("U.Q1", 1, 100, () => 1.0);
        engine.ConnectNode("U.D1", "U.Q1");

        engine.SetSimulationParameters(100, 1);
        engine.RunSimulation();

        var stats = engine.GetSimulationStats().QueueStats.First(q => q.Name == "U.Q1");

        Assert.True(stats.Utilization > 0.75);
    }
}
