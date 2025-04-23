# What needs to be done

## Simulation Core
Discrete Event Simulation
Event Queue

## Interpreter / Execution Engine
Pares and executes AQL
Needs to support anguage construsts like:
* Queue definitions
* Customer arrival logic
* Service time distributions
* Routing logic

## Queue Model Components
* Custmer Entity: Data structure representing a unit moving hrough the system.
* Queue Objects: Hold customers; manage entry/exit logic.
* Server Objects: Serve customers; have service times avaliability status, etc.

## Simulation World State
keeps track of all queues, servers, customers, and matrics (like queue lenghts, wait times, etc)

## Metrics Collector
Track statistics like:
* Averege wait time
* Server utilization
* Queue lengths over time
* Throughput
(This could support live updates but that will we not be doing because of time)

## Random Distribution Support
* Service and arrival times mainly uses the follow distributions exponential, normal, etc.
* Build or plug in a library to generate values from there

## Visualization or Debug Ouput (Don't think we will have time for this)
* A CLI or GUI that shows queue states, customer movement, etc.
* maybe a way to easily changes certain things for easy debugging
* maybe a rewind