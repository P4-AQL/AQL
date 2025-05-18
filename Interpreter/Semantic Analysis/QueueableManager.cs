


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Routes;

namespace Interpreter.SemanticAnalysis;

public static class QueueableManager
{
    public static List<Queueable> Queueables = [];

    public static Queueable FindQueueable(string name) =>
        FindQueueableOrDefault(name)
        ?? throw new($"Queueable '{name}' not found");

    public static Queueable? FindQueueableOrDefault(string name) =>
        Queueables.FirstOrDefault(q => q.Name == name);

    public static Queueable GetNewInstance(InstanceDeclaration instanceDeclaration)
    {
        Queueable instance = FindQueueable(instanceDeclaration.ExistingInstance.FirstIdentifier);
        string newName = instanceDeclaration.NewInstance.Identifier;

        if (instance is Queue queue)
        {
            return new Queue(
                name: newName,
                servers: queue.Servers,
                capacity: queue.Capacity,
                service: queue.Service,
                metrics: queue.Metrics
            );
        }
        else if (instance is Network network)
        {
            return new Network(
                name: newName,
                inputs: network.Inputs,
                outputs: network.Outputs,
                newInstances: network.NewInstances,
                routes: network.Routes
            );
        }

        throw new($"Invalid instance type '{instance.GetType()}' (Line: {instance})");
    }

    public static Queue IdentifierToInstantQueue(SingleIdentifierNode identifierNode) =>
        new(
            identifierNode.Identifier,
            servers: 1,
            int.MaxValue,
            service: () => 0,
            metrics: []
        );

    public static List<Route> GetRoute(RouteDefinitionNode routeDefinition, IEnumerable<Queue> inputs, IEnumerable<Queue> outputs, List<Queueable> newInstances, InterpreterClass interpreter)
    {
        List<Route> routes = [];
        foreach (RouteValuePairNode routeValuePairNode in routeDefinition.To)
        {
            double weight = (double)interpreter.InterpretExpression(routeValuePairNode.Probability, shadowVariableState: null);
            IdentifierNode routeToIdentifierNode = routeValuePairNode.RouteTo;

            Queueable routeToQueueable = FindQueueableFromIdentifier(inputs, outputs, newInstances, routeToIdentifierNode);

            if (routeValuePairNode.RouteTo is QualifiedIdentifierNode qualifiedIdentifierNode)
            {
                routeToQueueable = routeToQueueable.FindQueueable(qualifiedIdentifierNode.RightIdentifier.Identifier);
            }

            if (routeToQueueable is not Queue routeToQueue)
            {
                throw new($"Route '{routeToIdentifierNode.FirstIdentifier}' is not a queue (Line: {routeToIdentifierNode.LineNumber})");
            }

            if (routeDefinition.From is FunctionCallNode || routeDefinition.From is LiteralNode)
            {
                double GetRate() => (double)interpreter.InterpretExpression(routeDefinition.From, shadowVariableState: null);

                routes.Add(
                    new FuncRoute(
                        to: new(weight, routeToQueue),
                        rate: GetRate
                    )
                );
            }
            else if (routeDefinition.From is IdentifierExpressionNode identifierExpressionNode)
            {
                Queueable fromQueueable = FindQueueableFromIdentifier(inputs, outputs, newInstances, identifierExpressionNode.Identifier);
                if (fromQueueable is not Queue fromQueue)
                {
                    throw new($"Route '{routeDefinition.From}' is not a queue (Line: {routeDefinition.LineNumber})");
                }

                routes.Add(
                    new QueueRoute(
                        to: new(weight, routeToQueue),
                        queue: fromQueue
                    )
                );
            }
            else
            {
                throw new($"Route '{routeDefinition.From}' is not a valid route (Line: {routeDefinition.LineNumber})");
            }
        }
        return routes;
    }

    private static Queueable FindQueueableFromIdentifier(IEnumerable<Queue> inputs, IEnumerable<Queue> outputs, List<Queueable> newInstances, IdentifierNode routeToIdentifierNode)
    {
        // Look for the queueable referencing creation of a new instance
        Queueable? newInstanceToCreate = FindQueueableOrDefault(routeToIdentifierNode.FirstIdentifier);
        if (newInstanceToCreate is not null)
        {
            newInstances.Add(newInstanceToCreate);
            return newInstanceToCreate;
        }

        // Look for the queueable in the network
        return inputs.FirstOrDefault(q => q.Name == routeToIdentifierNode.FirstIdentifier) // Referencing input
            ?? outputs.FirstOrDefault(q => q.Name == routeToIdentifierNode.FirstIdentifier) // Referencing output
            ?? newInstances.FirstOrDefault(q => q.Name == routeToIdentifierNode.FirstIdentifier) // Referencing new instance already in network
            ?? throw new($"Queueable '{routeToIdentifierNode.FirstIdentifier}' not found (Line: {routeToIdentifierNode.LineNumber})");
    }
}

public abstract class Queueable(string name)
{
    public string Name { get; } = name;

    public abstract Queueable FindQueueable(string name);
}

public class Network(string name, IEnumerable<Queue> inputs, IEnumerable<Queue> outputs, IEnumerable<Queueable> newInstances, IEnumerable<Route> routes) : Queueable(name)
{
    public IReadOnlyList<Queue> Inputs { get; } = [.. inputs];
    public IReadOnlyList<Queue> Outputs { get; } = [.. outputs];
    public IReadOnlyList<Queueable> NewInstances { get; } = [.. newInstances];
    public IReadOnlyList<Route> Routes { get; } = [.. routes];

    public override Queueable FindQueueable(string name) =>
        Inputs.FirstOrDefault(q => q.Name == name) ??
        Outputs.FirstOrDefault(q => q.Name == name) ??
        NewInstances.FirstOrDefault(q => q.Name == name) ??
        throw new($"Queueable '{name}' not found in network '{Name}'");
}

public class NewQueuesInstance(Queue existingQueue, string instanceName)
{
    public Queue ExistingQueue { get; } = existingQueue;
    public string InstanceName { get; } = instanceName;
}

public class Queue(string name, int servers, int capacity, Func<double> service, IEnumerable<string> metrics) : Queueable(name)
{
    public int Servers { get; } = servers;
    public int Capacity { get; } = capacity;
    public Func<double> Service { get; } = service;
    public IReadOnlyList<string> Metrics { get; } = [.. metrics];

    public override Queueable FindQueueable(string name) =>
        throw new($"Queue '{Name}' has no sub-queueables");
}

public class Route(RouteTo to)
{
    public RouteTo To { get; } = to;
}

public class FuncRoute(RouteTo to, Func<double> rate) : Route(to)
{
    public Func<double> FromRate { get; } = rate;
}

public class QueueRoute(RouteTo to, Queue queue) : Route(to)
{
    public Queue FromQueue { get; } = queue;
}

public class RouteTo(double weight, Queue to)
{
    public double Weight { get; } = weight;
    public Queue ToQueue { get; } = to;
}