
using Xunit;
using SimEngine.Core;
using SimEngine.Nodes;
using System;

namespace SimEngine.Tests;

public class NodeTests
{
    [Fact]
    public void RouterNode_CanRouteEntity()
    {
        var engine = new SimulationEngineAPI();
        var router = new RouterNode(engine, "Router");

        // Can't test without full network, but call should not throw
        var entity = new Entity(0.0);
        router.Route(entity);
    }

    [Fact]
    public void QueueNode_ResetDoesNotThrow()
    {
        var engine = new SimulationEngineAPI();
        var queue = new QueueNode(engine, "Q", 1, 10, () => 1.0);
        var sim = new Simulation();
        queue.Reset(sim);
    }
}
