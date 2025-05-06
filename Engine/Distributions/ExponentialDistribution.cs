public class ExponentialDistribution
{
    private readonly Random rand = new();
    private readonly double lambda;

    public ExponentialDistribution(double lambda)
    {
        this.lambda = lambda;
    }

    public double Next()
    {
        return -Math.Log(1 - rand.NextDouble()) / lambda;
    }
}
