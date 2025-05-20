
using Xunit;
using SimEngine.Core;
using System;

namespace SimEngine.Tests;

public class SimulationTests
{
    [Fact]
    public void Schedule_AddsEventToQueue()
    {
        var sim = new Simulation();
        bool actionCalled = false;
        sim.Schedule(5.0, () => actionCalled = true);
        sim.Run(10.0);
        Assert.True(actionCalled);
    }

    [Fact]
    public void Run_StopsAtUntilTime()
    {
        var sim = new Simulation();
        sim.Schedule(100.0, () => throw new Exception("Should not run"));
        sim.Run(50.0);
        Assert.Equal(50.0, sim.Now);
    }
}
