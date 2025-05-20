
using Xunit;
using SimEngine.Core;
using System;

namespace SimEngine.Tests;

public class EventTests
{
    [Fact]
    public void Constructor_SetsTimeAndAction()
    {
        bool wasCalled = false;
        Action action = () => wasCalled = true;
        var ev = new Event(12.5, action);

        Assert.Equal(12.5, ev.Time);
        Assert.Same(action, ev.Action);
    }

    [Fact]
    public void Action_InvokesCorrectly()
    {
        bool triggered = false;
        var ev = new Event(5.0, () => triggered = true);

        ev.Action();

        Assert.True(triggered);
    }
}
