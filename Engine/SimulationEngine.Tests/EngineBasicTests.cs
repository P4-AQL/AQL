using Xunit;
using SimEngine.Core;

namespace SimulationEngine.Tests;

public class EngineBasicTests
{
    [Fact]
    public void Dispatcher_Should_Inject_Entities_Into_The_System()
    {
        var engine = new SimulationEngineAPI();
        engine.CreateDispatcherNode("Net.D1", () => 1.0);
        engine.CreateQueueNode("Net.Q1", 1, 10, () => 2.0);
        engine.ConnectNode("Net.D1", "Net.Q1");

        engine.SetSimulationParameters(50, 1);
        engine.RunSimulation();

        var stats = engine.GetSimulationStats();
        Assert.True(stats.TotalEntitiesCreated > 0);
    }
}
