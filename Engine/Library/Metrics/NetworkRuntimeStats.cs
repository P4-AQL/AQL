namespace SimEngine.Metrics;

using System;
using System.Collections.Generic;
using System.Linq;


public class NetworkRuntimeStats
{
    public string Name { get; init; } = "";
    public List<double> RespondTimes { get; } = new();

    public void AddRespondTime(double time)
    {
        RespondTimes.Add(time);
    }

    public double MeanRespondTime => RespondTimes.Count > 0 ? RespondTimes.Average() : 0.0;
    public double VarianceRespondTime
    {
        get
        {
            if (RespondTimes.Count == 0) return 0.0;
            var mean = MeanRespondTime;
            return RespondTimes.Average(t => Math.Pow(t - mean, 2));
        }
    }

    public double TailProbability(double t)
    {
        if (RespondTimes.Count == 0) return 0.0;
        return (double)RespondTimes.Count(x => x > t) / RespondTimes.Count;
    }
}
