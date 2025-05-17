using System;
using SimEngine.Core;
using SimEngine.Metrics;
using SimEngine.Networks;

class Program
{
    static void Main()
    {
        var engine = new SimulationEngineAPI();

        // Use a fixed seed for deterministic runs
        engine.SetSeed(1234);

        // Utility distributions
        Func<double> Exp(double rate) => () => -Math.Log(1 - engine.RandomGenerator.NextDouble()) / rate;
        Func<double> Const(double value) => () => value;

        // Define the Kitchen network
        var kitchen = new NetworkDefinition { Name = "Kitchen" };
        kitchen.AddEntryPoint("In");
        kitchen.AddQueue("PrepStation", 1, 10, Exp(1.5));
        kitchen.AddQueue("Grill", 1, 5, Exp(1.2));
        kitchen.AddExitPoint("Out");

        kitchen.Connect("In", "PrepStation");
        kitchen.Connect("PrepStation", "Grill");
        kitchen.Connect("Grill", "Out");

        // Define the Service network
        var service = new NetworkDefinition { Name = "Service" };
        service.AddEntryPoint("Start");
        service.AddQueue("Cashier", 2, 15, Exp(1.0));
        service.AddExitPoint("End");

        service.Connect("Start", "Cashier");
        service.Connect("Cashier", "End");

        // Define the full Restaurant network
        var restaurant = new NetworkDefinition { Name = "Restaurant" };
        restaurant.AddSubNetwork(kitchen);
        restaurant.AddSubNetwork(service);

        engine.CreateNetwork(restaurant); // must come before cross-network connection
        engine.ConnectNode("Restaurant.Kitchen.Out", "Restaurant.Service.Start");
                

        // Dispatcher sends customers to Kitchen
        engine.CreateDispatcherNode("Restaurant.D1", Const(0.8));
        engine.ConnectNode("Restaurant.D1", "Restaurant.Kitchen.In");

        // Final delivery point (queue outside the network)
        engine.CreateQueueNode("Delivery", 1, 20, Exp(0.5));
        engine.ConnectNode("Restaurant.Service.End", "Delivery");

        // Run the simulation
        engine.SetSimulationParameters(3000, 3);
        engine.RunSimulation();

        // Print metrics
        var stats = engine.GetSimulationStats();
        MetricsPrinter.Print(stats);
    }
}
