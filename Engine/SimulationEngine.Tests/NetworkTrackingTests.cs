using Xunit;
using SimEngine.Core;
using System.Linq;

namespace SimulationEngine.Tests;

public class NetworkTrackingTests
{
    [Fact]
    public void Network_Should_Record_ResponseTimes()
    {
        var engine = new SimulationEngineAPI();
        engine.CreateDispatcherNode("A.D1", () => 1.0);
        engine.CreateQueueNode("A.Q1", 1, 10, () => 2.0);
        engine.ConnectNode("A.D1", "A.Q1");

        engine.SetSimulationParameters(50, 2);
        engine.RunSimulation();

        var nStats = engine.GetSimulationStats().NetworkStats.First(n => n.Name == "A");

        Assert.True(nStats.MeanRespondTime > 0);
        Assert.True(nStats.TailProbability(5) >= 0);
    }
}
