using System;
using SimEngine.Core;
using SimEngine.Metrics;

class Program
{
    static void Main()
    {
        // We start by defining the engine to the a new instance of the SimulationEnigneAPI()
        var engine = new SimulationEngineAPI();
        
        // Set the seed for the random number generator shall be a INT
        engine.SetSeed(1234);


        // This are the functions that the creations of the dispatcher and queues need
        Func<double> Exp(double rate) => () => -Math.Log(1 - Random.Shared.NextDouble()) / rate;
        Func<double> Const(double value) => () => value;

        // To use the simulation library your names for dispatchers and queues shall follow the naming convention "NetworkName.NodeName" and shall all be unique to avoid overwrites

        // Creates a dispatcher for new entities in the system, it takes a name and a function that returns a double for the distribution 
        engine.CreateDispatcherNode("Network1.D1", Const(1.0));

        // Creates a queue for the system, it takes a name, the number of the servers i the queue, the max of entities that can be in the queue at the same time and a service distribution that are a function that returns a double
        engine.CreateQueueNode("Network1.Q1", 2, 10, Exp(1.2));
        engine.CreateQueueNode("Network1.Q2", 1, 10, Exp(1.15));
        engine.CreateQueueNode("Network1.Q3", 1, 25, Exp(1.05));

        // Creates a route in the system, it takes the name of the node where the entities comes from and then the name of the queue that the entities shall go to
        engine.ConnectNode("Network1.D1", "Network1.Q1");

        // How to create a route in the system where the queue have two queues it outputs too
        engine.ConnectNode("Network1.Q1", "Network1.Q2", 0.4);
        engine.ConnectNode("Network1.Q1", "Network1.Q3", 0.6);

        // parameters for the simulation time it runs until and how many runs the simulation runs
        engine.SetSimulationParameters(5000, 5);

        // We shall run the simulation after all queues, dispatchers and routes are created and simulation parameters are sat(if the parameters are not sat a set of standard parameters will be used)
        engine.RunSimulation();

        // This are the print statements to get the metrics out with, this will with high likelyhood change
        var stats = engine.GetSimulationStats();
        MetricsPrinter.Print(stats);
    }
}
