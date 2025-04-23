# What needs to be done

## Simulation Core
Discrete Event Simulation
Event Queue

## Interpreter / Execution Engine
Parses and executes AQL
Needs to support language constructs like:
* Queue definitions
* Customer arrival logic
* Service time distributions
* Routing logic

## Queue Model Components
* Customer Entity: Data structure representing a unit moving through the system.
* Server Objects: Serve customers; have service times availability status, etc.

## Simulation World State
Keeps track of all queues, servers, customers, and metrics (like queue lengths, wait times, etc)

## Metrics Collector
Track statistics like:
* Average wait time
* Server utilization
* Queue lengths over time
* Throughput
(This could support live updates but that will not be done because of time)

## Random Distribution Support
* Service and arrival times mainly use the following distributions: exponential, normal, etc.
* Build or plug in a library to generate values from there

## Visualization or Debug Output (Don't think we will have time for this)
* A CLI or GUI that shows queue states, customer movement, etc.
* Potential configuration options to facilitate debugging
* maybe a rewind