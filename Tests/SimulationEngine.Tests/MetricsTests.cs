
using Xunit;
using SimEngine.Metrics;

namespace SimEngine.Tests;

public class MetricsTests
{
    [Fact]
    public void QueueRuntimeStats_CalculatesStatsCorrectly()
    {
        var stats = new QueueRuntimeStats { ServerCount = 2, SimulationTimePerRun = 10 };
        stats.AddArrival();
        stats.AddServed(3.0, 5.0);
        stats.AddBusyTime(12.0);
        stats.IncrementRun();
        stats.AddDropped();

        Assert.Equal(3.0, stats.AvgWaitTime);
        Assert.Equal(5.0, stats.AvgServiceTime);
        Assert.Equal(0.6, stats.Utilization, 1);
        Assert.Equal(1.0, stats.Throughput);
        Assert.Equal(1.0, stats.DropRate);
    }

    [Fact]
    public void NetworkRuntimeStats_CalculatesMeanAndVariance()
    {
        var stats = new NetworkRuntimeStats();
        stats.AddRespondTime(4.0);
        stats.AddRespondTime(6.0);

        Assert.Equal(5.0, stats.MeanRespondTime);
        Assert.True(stats.VarianceRespondTime > 0);
        Assert.Equal(0.5, stats.TailProbability(5.0));
    }

    [Fact]
    public void SimulationStats_CalculatesRates()
    {
        var stats = new SimulationStats();
        stats.AddEntityCreated(); // 1
        stats.AddEntityCreated(); // 2
        stats.AddSimulationRunTime(10.0); // 2 entities / 10 time → rate = 0.2, interarrival = 5

        Assert.Equal(0.2, stats.ArrivalRate, 1);
        Assert.Equal(5.0, stats.MeanInterarrivalTime, 1);
    }
}
