using System;

class Program
{
    static void Main()
    {
        var engine = new SimulationEngineAPI();

        Func<double> Exp(double rate) => () => -Math.Log(1 - Random.Shared.NextDouble()) / rate;

        engine.CreateQueue("Q1", 2, 10, Exp(0.2), Exp(0.1));
        engine.CreateQueue("Q2", 1, 10, Exp(0.15));
        engine.ConnectQueues("Q1", "Q2");
        engine.SetSimulationParameters(5000, 5);
        engine.RunSimulation();

        var results = engine.GetMetrics();
        MetricsPrinter.Print(results);
    }
}
