using Xunit;
using SimEngine.Metrics;

public class NetworkRuntimeStatsTests
{
    [Fact]
    public void AddRespondTime_ShouldAffectMeanAndVariance()
    {
        var stats = new NetworkRuntimeStats();
        stats.AddRespondTime(4.0);
        stats.AddRespondTime(6.0);

        Assert.Equal(5.0, stats.MeanRespondTime, 3);
        Assert.True(stats.VarianceRespondTime > 0);
    }

    [Fact]
    public void TailProbability_ShouldReturnCorrectFraction()
    {
        var stats = new NetworkRuntimeStats();
        stats.AddRespondTime(1.0);
        stats.AddRespondTime(5.0);
        stats.AddRespondTime(10.0);

        var p = stats.TailProbability(5.0);
        Assert.Equal(1.0 / 3.0, p, 3); // only 10 > 5
    }

    [Fact]
    public void EmptyStats_ShouldReturnZero()
    {
        var stats = new NetworkRuntimeStats();

        Assert.Equal(0, stats.MeanRespondTime);
        Assert.Equal(0, stats.VarianceRespondTime);
        Assert.Equal(0, stats.TailProbability(1.0));
    }
}