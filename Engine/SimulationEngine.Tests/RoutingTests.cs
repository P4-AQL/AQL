using Xunit;
using SimEngine.Core;
using System.Linq;

namespace SimulationEngine.Tests;

public class RoutingTests
{
    [Fact]
    public void Queue_Should_Route_According_To_Probabilities()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(1234);

        engine.CreateDispatcherNode("R.D1", () => 1.0);
        engine.CreateQueueNode("R.Q1", 1, 100, () => 1.0);
        engine.CreateQueueNode("R.Q2", 1, 100, () => 1.0);
        engine.CreateQueueNode("R.Q3", 1, 100, () => 1.0);

        engine.ConnectNode("R.D1", "R.Q1");
        engine.ConnectNode("R.Q1", "R.Q2", 0.3);
        engine.ConnectNode("R.Q1", "R.Q3", 0.7);

        engine.SetSimulationParameters(200, 1);
        engine.RunSimulation();

        var q2 = engine.GetSimulationStats().QueueStats.First(q => q.Name == "R.Q2");
        var q3 = engine.GetSimulationStats().QueueStats.First(q => q.Name == "R.Q3");

        double ratio = q2.TotalArrived / (double)(q2.TotalArrived + q3.TotalArrived);
        Assert.InRange(ratio, 0.25, 0.35); // Should center around 0.3
    }
}
