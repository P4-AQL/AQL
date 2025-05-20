
using Xunit;
using SimEngine.Core;
using System.Collections.Generic;

namespace SimEngine.Tests;

public class EntityTests
{
    [Fact]
    public void Constructor_SetsCreationAndArrivalTime()
    {
        var entity = new Entity(5.0);
        Assert.Equal(5.0, entity.CreationTime);
        Assert.Equal(5.0, entity.ArrivalTime);
    }

    [Fact]
    public void TotalWaitingTime_ComputesSum()
    {
        var entity = new Entity(0.0);
        entity.WaitingTimesInQueues = new List<double> { 1.0, 2.5, 3.5 };
        Assert.Equal(7.0, entity.TotalWaitingTime);
    }

    [Fact]
    public void TotalServiceTime_ComputesSum()
    {
        var entity = new Entity(0.0);
        entity.ServiceTimesInQueues = new List<double> { 2.0, 2.0 };
        Assert.Equal(4.0, entity.TotalServiceTime);
    }

    [Fact]
    public void TotalTime_ComputesCorrectly()
    {
        var entity = new Entity(1.0);
        entity.DepartureTime = 5.5;
        Assert.Equal(4.5, entity.TotalTime);
    }
}
