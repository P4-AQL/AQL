using Xunit;
using SimEngine.Core;
using System.Linq;

namespace SimulationEngine.Tests;

public class EntityCompletionTests
{
    [Fact]
    public void Entities_Should_Exit_When_No_NextNode()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(42);

        engine.CreateDispatcherNode("C.D1", () => 1.0);
        engine.CreateQueueNode("C.Q1", 1, 100, () => 2.0);
        engine.ConnectNode("C.D1", "C.Q1");

        engine.SetSimulationParameters(100, 1);
        engine.RunSimulation();

        var exitedEntities = engine.GetEntities().Count(e => e.DepartureTime > 0);
        Assert.True(exitedEntities > 0);
    }
}
