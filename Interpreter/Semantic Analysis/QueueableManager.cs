


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Routes;

namespace Interpreter.SemanticAnalysis;

public class NetworkDefinitionManager(InterpretationEnvironment interpretationEnvironment)
{
    public List<Queueable> Queueables = [];

    InterpretationEnvironment InterpretationEnvironment { get; } = interpretationEnvironment;

    public NetworkEntity FindNetworkEntityOrThrow(IdentifierNode identifierNode) =>
        TryFindNetworkEntity(identifierNode)
        ?? throw new InterpretationException($"Queueable '{identifierNode.FirstIdentifier}' not found (Line: {identifierNode.LineNumber})");

    public NetworkEntity? TryFindNetworkEntity(IdentifierNode identifierNode)
    {
        NetworkEntity? returnValue = null;
        
        string firstIdentifier = identifierNode.FirstIdentifier;
        // Is the identifier referencing a local Queueable?
        
        Queueable? queueable = FindQueueableOrDefault(firstIdentifier);
        
        if (identifierNode is QualifiedIdentifierNode qualifiedIdentifierNode)
        {
            
            if (queueable is null)
            {
                
                // Is the identifier referencing a Queueable in an imported dependency?
                if (InterpretationEnvironment.ModuleDependencies.Lookup(firstIdentifier, out InterpretationEnvironment dependency))
                {
                    queueable = dependency.QueueableManager.FindQueueableOrDefault(qualifiedIdentifierNode.RightIdentifier.Identifier);
                }
            }
            else
            {
                // Is the identifier referencing a network entity in the Queueable?
                if (queueable is Network network)
                {
                    
                    returnValue = network.FindNetworkEntity(qualifiedIdentifierNode.RightIdentifier, this);
                }
            }
        }

        // If the identifier did not reference a network entity in the Queueable, assign what we found
        returnValue ??= queueable;

        return returnValue;
    }

    public Queueable? FindQueueableOrDefault(string name) =>
        Queueables.FirstOrDefault(q => q.Name == name);

    public static NetworkInputOrOutput GetInputOrOutput(SingleIdentifierNode identifierNode)
    {
        return new(identifierNode.Identifier);
    }

    public Queueable GetNewInstance(InstanceDeclaration instanceDeclaration)
    {
        NetworkEntity instance = FindNetworkEntityOrThrow(instanceDeclaration.ExistingInstance);

        if (instance is not Queueable queueable)
        {
            throw new InterpretationException($"Invalid type '{instance.GetType()}' (Line: {instanceDeclaration.LineNumber})");
        }

        string newName = instanceDeclaration.NewInstance.Identifier;
        return GetNewInstance(queueable, newName);
    }

    public Queueable GetNewInstance(Queueable existingQueue, string newName)
    {
        if (existingQueue is Queue queue)
        {
            return new Queue(
                name: newName,
                servers: queue.Servers,
                capacity: queue.Capacity,
                service: queue.Service,
                metrics: queue.Metrics
            );
        }
        else if (existingQueue is Network network)
        {
            return new Network(
                name: newName,
                inputs: network.Inputs,
                outputs: network.Outputs,
                newInstances: network.NewInstances,
                routes: network.Routes
            );
        }
        else
        {
            throw new InterpretationException($"Invalid type '{existingQueue.GetType()}'");
        }
    }

    public List<Route> GetRoutes(RouteDefinitionNode routeDefinition, Network thisNetwork, Func<ExpressionNode, double> interpretExpression)
    {
        if (routeDefinition.From is FunctionCallNode or LiteralNode)
        {
            double GetRate() => interpretExpression.Invoke(routeDefinition.From);

            FuncRoute CreateFuncRoute(double weight, NetworkEntity routeTo, string toName) =>
                new(
                    toProbabilityPair: new(weight, routeTo, toName),
                    rate: GetRate
                );

            return GetRoutes(CreateFuncRoute);
        }
        else if (routeDefinition.From is IdentifierExpressionNode identifierExpressionNode)
        {
            NetworkEntity from = thisNetwork.FindNetworkEntity(identifierExpressionNode.Identifier, this);
            if (from is not (Queue or NetworkInputOrOutput))
            {
                throw new InterpretationException($"Route '{routeDefinition.From}' is not a queue (Line: {routeDefinition.LineNumber})");
            }

            NetworkEntityRoute CreateNetworkEntityRoute(double weight, NetworkEntity routeTo, string toName) =>
                new(
                    to: new(weight, routeTo, toName),
                    from: from,
                    fromName: identifierExpressionNode.Identifier.FullIdentifier
                );

            return GetRoutes(CreateNetworkEntityRoute);
        }
        else
        {
            throw new InterpretationException($"Route '{routeDefinition.From}' is not a valid route (Line: {routeDefinition.LineNumber}");
        }

        List<Route> GetRoutes<T>(Func<double, NetworkEntity, string, T> createRoute) where T : Route
        {
            List<Route> routes = [];
            foreach (RouteValuePairNode routeValuePairNode in routeDefinition.To)
            {
                double weight = interpretExpression.Invoke(routeValuePairNode.Probability);
                IdentifierNode routeToIdentifierNode = routeValuePairNode.RouteTo;
                string routeToName = routeToIdentifierNode.FullIdentifier;

                NetworkEntity routeTo = thisNetwork.FindNetworkEntity(routeToIdentifierNode, this);

                routes.Add(
                    createRoute(
                        weight,
                        routeTo,
                        routeToName
                    )
                );
            }
            return routes;
        }
    }
}


public abstract class NetworkEntity(string name)
{
    public NetworkEntity? parent;
    public string Name { get; } = name;
}

public abstract class Queueable(string name) : NetworkEntity(name)
{

}

public class Network(string name, IEnumerable<NetworkInputOrOutput> inputs, IEnumerable<NetworkInputOrOutput> outputs, IEnumerable<Queueable> newInstances) : Queueable(name)
{
    public IReadOnlyList<NetworkInputOrOutput> Inputs = [.. inputs];
    public IReadOnlyList<NetworkInputOrOutput> Outputs = [.. outputs];
    public List<Queueable> NewInstances = [.. newInstances];
    public IReadOnlyList<Route>? Routes;

    public Network(string name, IEnumerable<NetworkInputOrOutput> inputs, IEnumerable<NetworkInputOrOutput> outputs, IEnumerable<Queueable> newInstances, IEnumerable<Route>? routes) : this(name, inputs, outputs, newInstances)
    {
        if (routes is not null)
        {
            Routes = [.. routes];
        }
    }

    public NetworkEntity FindNetworkEntity(IdentifierNode identifierNode, NetworkDefinitionManager networkDefinitionManager)
    {
        // Is the identifier referencing a local input or output?
        NetworkEntity? entity = Inputs.FirstOrDefault(q => q.Name == identifierNode.FirstIdentifier)
            ?? Outputs.FirstOrDefault(q => q.Name == identifierNode.FirstIdentifier);

        // Is the identifier referencing a local new instance?
        entity ??= NewInstances.FirstOrDefault(q => q.Name == identifierNode.FirstIdentifier);

        // Is the identifier locally defined but qualified?
        if (entity is not null && identifierNode is QualifiedIdentifierNode qualifiedIdentifierNode)
        {
            if (entity is not Network network)
            {
                throw new InterpretationException($"Invalid identifier: '{qualifiedIdentifierNode.LeftIdentifier.Identifier}' was not a network, could not get '{qualifiedIdentifierNode.RightIdentifier.Identifier}' (Line: {identifierNode.LineNumber})");
            }
            entity = network.FindNetworkEntity(qualifiedIdentifierNode.RightIdentifier, networkDefinitionManager);
        }

        // Is the identifier referencing a global definition?
        if (entity is null)
        {
            entity = networkDefinitionManager.FindNetworkEntityOrThrow(identifierNode);
            if (entity is Queueable queueable)
            {
                NewInstances.Add(networkDefinitionManager.GetNewInstance(queueable, queueable.Name));
            }
        }

        return entity;
    }
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
}

public class NetworkInputOrOutput(string name) : NetworkEntity(name)
{

}

public class Route(RouteToProbabilityPair toProbabilityPair)
{
    public RouteToProbabilityPair ToProbabilityPair { get; } = toProbabilityPair;
}

public class FuncRoute(RouteToProbabilityPair toProbabilityPair, Func<double> rate) : Route(toProbabilityPair)
{
    public Func<double> FromRate { get; } = rate;
}

public class NetworkEntityRoute(RouteToProbabilityPair to, NetworkEntity from, string fromName) : Route(to)
{
    public NetworkEntity From { get; } = from;
    public string FromName { get; } = fromName;
}

public class RouteToProbabilityPair(double weight, NetworkEntity to, string toName)
{
    public double Weight { get; } = weight;
    public NetworkEntity To { get; } = to;
    public string ToName { get; } = toName;
}