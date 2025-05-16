using Xunit;
using SimEngine.Core;
using System.Linq;

namespace SimulationEngine.Tests;

public class MetricsCalculationTests
{
    [Fact]
    public void Queue_Should_Accurately_Report_Metrics()
    {
        var engine = new SimulationEngineAPI();
        engine.CreateDispatcherNode("Net.D1", () => 1.0);
        engine.CreateQueueNode("Net.Q1", 1, 10, () => 1.0);
        engine.ConnectNode("Net.D1", "Net.Q1");

        engine.SetSimulationParameters(20, 3);
        engine.RunSimulation();

        var qStats = engine.GetSimulationStats().QueueStats.First(q => q.Name == "Net.Q1");

        Assert.True(qStats.Utilization > 0);
        Assert.True(qStats.Throughput > 0);
        Assert.True(qStats.AvgWaitTime >= 0);
    }
}
