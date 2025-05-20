using System;
using SimEngine.Core;

class Program
{
    static void Main()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(42);

        Func<double> Exp(double rate) => () => -Math.Log(1 - engine.RandomGenerator.NextDouble()) / rate;
        Func<double> Const(double value) => () => value;

        // --- Pizza Shop ---
        var pizza = new SimEngine.Networks.NetworkDefinition { Name = "PizzaShop" };
        pizza.AddEntryPoint("In");
        pizza.AddQueue("PrepStation", 1, 10, Exp(1.2));
        pizza.AddQueue("PizzaOven", 1, 5, Exp(1.0));
        pizza.AddQueue("Cashier", 1, 8, Exp(0.9));
        pizza.AddExitPoint("Out");
        pizza.Connect("In", "PrepStation");
        pizza.Connect("PrepStation", "PizzaOven");
        pizza.Connect("PizzaOven", "Cashier");
        pizza.Connect("Cashier", "Out");

        // --- Chinese Shop ---
        var chinese = new SimEngine.Networks.NetworkDefinition { Name = "ChineseShop" };
        chinese.AddEntryPoint("In");
        chinese.AddQueue("PrepStation", 1, 10, Exp(1.3));
        chinese.AddQueue("SushiStation", 1, 6, Exp(0.8));
        chinese.AddQueue("RamenStation", 1, 6, Exp(1.0));
        chinese.AddQueue("Cashier", 1, 8, Exp(0.95));
        chinese.AddExitPoint("Out");
        chinese.Connect("In", "PrepStation");
        chinese.Connect("PrepStation", "SushiStation", 0.4);
        chinese.Connect("PrepStation", "RamenStation", 0.6);
        chinese.Connect("SushiStation", "Cashier");
        chinese.Connect("RamenStation", "Cashier");
        chinese.Connect("Cashier", "Out");

        // --- Mall wrapper ---
        var mall = new SimEngine.Networks.NetworkDefinition { Name = "Mall" };
        mall.AddEntryPoint("Entry");
        mall.AddExitPoint("Out");
        mall.AddSubNetwork(pizza);
        mall.AddSubNetwork(chinese);
        mall.Connect("Entry", "PizzaShop.In", 0.5);
        mall.Connect("Entry", "ChineseShop.In", 0.5);
        mall.Connect("PizzaShop.Out", "Out");
        mall.Connect("ChineseShop.Out", "Out");

        engine.CreateNetwork(mall);

        // --- Dispatcher OUTSIDE mall ---
        engine.CreateDispatcherNode("D1", Const(0.8));
        engine.ConnectNode("D1", "Mall.Entry");

        // --- Final delivery queue ---
        engine.CreateQueueNode("Delivery", 1, 30, Exp(1.2));
        engine.ConnectNode("Mall.Out", "Delivery");

        // --- Run simulation ---
        engine.SetSimulationParameters(1000, 1);
        engine.RunSimulation();

        // --- Print results ---
        var stats = engine.GetSimulationStats();
        engine.PrintMetric(stats);
    }
}
