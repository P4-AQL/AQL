using System;
using SimEngine.Core;
using SimEngine.Networks;


class Program
{
    static void Main()
    {
        var engine = new SimulationEngineAPI();
        engine.SetSeed(42); // or whatever seed you use

        var topNet = new NetworkDefinition { Name = "someAdvancedNetwork" };

        // Entry/exit points
        topNet.AddEntryPoint("firstInput");
        topNet.AddExitPoint("firstOutput");

        // Subnetwork: someAdvancedNetwork.basicNetwork
        var basicNet = new NetworkDefinition(topNet) { Name = "basicNetwork" };
        basicNet.AddEntryPoint("firstInput");
        basicNet.AddExitPoint("singleOutput");

        topNet.AddSubNetwork(basicNet);

        // Queues in basicNetwork
        basicNet.AddQueue("secQueue1", 5, 95, () => engine.Exponential(10));
        basicNet.AddQueue("secQueue2", 5, 95, () => engine.Exponential(10));

        // Route in basicNetwork
        basicNet.Connect("firstInput", "secQueue1", 1);
        basicNet.Connect("secQueue1", "secQueue2", 1);
        basicNet.Connect("secQueue2", "singleOutput", 1);

        // Queue: testQueue1 in someAdvancedNetwork
        topNet.AddQueue("testQueue1", 7, 10, () => 20);

        // Dispatcher for 250 constant arrival rate
        string dispatcher = "someAdvancedNetwork.@ dispatcher @.0";
        engine.CreateDispatcherNode(dispatcher, () => 250, "someAdvancedNetwork");

        // Connect dispatcher to firstInput
        engine.ConnectNode(dispatcher, "someAdvancedNetwork.firstInput", 1);

        // Routes in someAdvancedNetwork
        topNet.Connect("firstInput", "basicNetwork.firstInput", 1);
        topNet.Connect("basicNetwork.singleOutput", "testQueue1", 1);
        topNet.Connect("testQueue1", "firstOutput", 1);

        // Register top-level network
        engine.CreateNetwork(topNet);

        // Run
        engine.SetSimulationParameters(untilTime: 500, runCount: 5);
        engine.RunSimulation();
        engine.PrintMetric(engine.GetSimulationStats());
    }
}
