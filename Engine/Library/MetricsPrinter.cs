using System;
using System.Collections.Generic;

public static class MetricsPrinter
{
    public static void Print(Dictionary<string, QueueMetrics> metrics)
    {
        foreach (var (name, data) in metrics)
        {
            Console.WriteLine($"\nQueue: {name}");
            Console.WriteLine($"  Arrived (total):   {data.TotalArrived}");
            Console.WriteLine($"  Served (total):    {data.TotalServed}");
            Console.WriteLine($"  Avg Wait Time:     {data.AvgWaitTime:F2}");
            Console.WriteLine($"  Max Queue Length:  {data.MaxQueueLength}");
            Console.WriteLine($"  Utilization:       {data.ServerUtilization:P2}");
            Console.WriteLine($"  Avg Throughput/run:{data.Throughput:F2}");
        }
    }
}
