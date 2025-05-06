Simulation sim = new Simulation();
Network network = new Network("Airport", sim);

// Define distributions
var expDistCheckIn = new ExponentialDistribution(0.2);
var expDistSecurity = new ExponentialDistribution(0.1);

// Queues setup (with network parameter)
Queue checkInQueue = new Queue("CheckIn", 2, 100, expDistCheckIn.Next, sim, network);
Queue securityQueue = new Queue("Security", 3, 50, expDistSecurity.Next, sim, network);

// Add queues to network
network.AddQueue(checkInQueue);
network.AddQueue(securityQueue);

// Define routing
network.AddRoute("CheckIn", "Security");

// Schedule initial entity
sim.Schedule(0, () => checkInQueue.Enqueue(new Entity(sim.CurrentTime)));

// Run simulation
sim.Run(untilTime: 1000);
