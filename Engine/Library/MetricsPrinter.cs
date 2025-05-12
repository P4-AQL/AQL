using System;
using System.Collections.Generic;

public static class MetricsPrinter
{
    public static void Print(Dictionary<string, QueueMetrics> metrics)
    {
        foreach (var (name, data) in metrics)
        {
            Console.WriteLine($"\nQueue: {name}");
            Console.WriteLine($"  Arrived (total):      {data.TotalArrived}");
            Console.WriteLine($"  Served (total):       {data.TotalServed}");
            Console.WriteLine($"  Avg Wait Time:        {data.AvgWaitTime:F2}");
            Console.WriteLine($"  Max Queue Length:     {data.MaxQueueLength}");
            Console.WriteLine($"  Utilization:          {data.ServerUtilization:P2}");
            Console.WriteLine($"  Avg Throughput/run:   {data.Throughput:F2}");
            Console.WriteLine($"  Max Dropped:          {data.MaxDroppedEntities}");
            Console.WriteLine($"  Entity Dropped Rate:  {data.EntitiesDroppedRate:p2}");
        }
    }

    public static void PrintNetworkMetrics(Dictionary<string, NetworkMetrics> metrics)
{
    foreach (var (name, data) in metrics)
    {
        Console.WriteLine($"\nNetwork: {name}");
        Console.WriteLine($"  Entered:              {data.Entered}");
        Console.WriteLine($"  Exited:               {data.Exited}");
        Console.WriteLine($"  Avg Time in Network:  {data.AvgTimeInNetwork:F2}");
    }
}
}
