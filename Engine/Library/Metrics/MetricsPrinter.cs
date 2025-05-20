namespace SimEngine.Metrics;

using System;

public static class MetricsPrinter
{
    public static void Print(SimulationStats stats)
    {
        Console.WriteLine("\n--- Simulation Summary ---");
        Console.WriteLine($"Total Simulation Time:    {stats.TotalSimulationTime:F2}");
        Console.WriteLine($"Total Entities Created:   {stats.TotalEntitiesCreated}");
        Console.WriteLine($"Arrival Rate (λ):         {stats.ArrivalRate:F2}");
        Console.WriteLine($"Mean Interarrival Time:   {stats.MeanInterarrivalTime:F2}");

        Console.WriteLine("\n--- Queue Statistics ---");
        foreach (var q in stats.QueueStats)
        {
            double serviceRate = q.AvgServiceTime > 0 ? 1.0 / q.AvgServiceTime : 0.0;
            double expectedN = q.TotalServiceTime / stats.TotalSimulationTime;
            double expectedNq = q.TotalWaitingTime / stats.TotalSimulationTime;

            Console.WriteLine($"Queue: {q.Name}");
            Console.WriteLine($"  Total Arrived:       {q.TotalArrived}");
            Console.WriteLine($"  Total Served:        {q.TotalServed}");
            Console.WriteLine($"  Dropped Entities:    {q.DroppedEntities}");
            Console.WriteLine($"  Drop Rate:           {q.DropRate:P2}");
            Console.WriteLine($"  Avg Wait Time:       {q.AvgWaitTime:F2}");
            Console.WriteLine($"  Avg Service Time:    {q.AvgServiceTime:F2}");
            Console.WriteLine($"  Service Rate (μ):    {serviceRate:F2}");
            Console.WriteLine($"  Expected N:          {expectedN:F2}");
            Console.WriteLine($"  Expected Nq:         {expectedNq:F2}");
            Console.WriteLine($"  Utilization (ρ):     {q.Utilization:P2}");
            Console.WriteLine($"  Throughput (X):      {q.Throughput:F2}\n");
        }

        Console.WriteLine("\n--- Network Statistics ---");
        foreach (var n in stats.NetworkStats)
        {
            Console.WriteLine($"Network: {n.Name}");
            Console.WriteLine($"  Mean Response Time:  {n.MeanRespondTime:F2}");
            Console.WriteLine($"  Variance:            {n.VarianceRespondTime:F2}");
            Console.WriteLine($"  P(RespondTime > 5):  {n.TailProbability(5):P2}\n");
        }
    }
}