


using System.Diagnostics.CodeAnalysis;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Routes;
using Interpreter.AST.Nodes.Statements;
using Interpreter.Utilities.Modules;
using SimEngine.Core;
using SimEngine.Networks;

namespace Interpreter.SemanticAnalysis;

public class InterpreterClass(ProgramNode node)
{
    public InterpretationEnvironment GlobalEnvironment => globalEnvironment;
    InterpretationEnvironment globalEnvironment = InterpretationEnvironment.Empty(node);

    Table<FunctionStateTuple> FunctionState => globalEnvironment.FunctionState;
    Table<object> VariableState => globalEnvironment.VariableState;
    Table<NetworkDeclarationNode> NetworkState => globalEnvironment.NetworkState;
    public SimulationEngineAPI? LastEngine { get; private set; }

    internal NetworkDefinitionManager QueueableManager => globalEnvironment.QueueableManager;

    public InterpretationEnvironment StartInterpretation()
    {
        try
        {
            InterpretProgram(globalEnvironment.Root);
        }
        catch (Exception ex)
        {
            globalEnvironment.SetError(ex.Message);
            
        }

        return globalEnvironment;
    }

    public void InterpretProgram(ProgramNode node)
    {
        if (node is ImportNode importNode)
        {
            InterpretImport(importNode);
        }
        else if (node is DefinitionProgramNode definitionNode)
        {
            InterpretDefinition(definitionNode.Definition);
        }

        else
        {
            throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
        }
    }

    public void InterpretImport(ImportNode node)
    {
        ProgramNode astRoot = ModuleLoader.LoadModuleByName(node.Namespace.Identifier);
        InterpretationEnvironment dependency = new InterpreterClass(astRoot).StartInterpretation();
        globalEnvironment.ModuleDependencies.ForceBind(node.Namespace.Identifier, dependency);

        if (node.NextProgram is not null)
        {
            InterpretProgram(node.NextProgram);
        }
    }

    public void InterpretDefinition(DefinitionNode node)
    {
        if (node is DefinitionCompositionNode compositionNode)
        {
            InterpretDefinitionComposition(compositionNode);
        }
        else if (node is SimulateNode simulateNode)
        {
            InterpretSimulate(simulateNode);
        }

        else
        {
            throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
        }
    }

    public void InterpretDefinitionComposition(DefinitionCompositionNode node)
    {
        if (node is FunctionNode functionNode)
        {
            InterpretFunctionDeclaration(functionNode);
        }
        else if (node is ConstDeclarationNode constNode)
        {
            InterpretConstDeclaration(constNode);
        }
        else if (node is NetworkDefinitionNode networkNode)
        {
            InterpretNetwork(networkNode.Network);
        }

        if (node.NextDefinition is not null)
        {
            InterpretDefinition(node.NextDefinition);
        }
    }

    public void InterpretFunctionDeclaration(FunctionNode functionNode)
    {
        FunctionStateTuple functionState = new(function: functionNode, variableState: VariableState);
        FunctionState.ForceBind(functionNode.Identifier.Identifier, functionState);
    }

    public void InterpretConstDeclaration(ConstDeclarationNode constNode)
    {
        object value = InterpretExpression(constNode.Expression, shadowVariableState: null);
        VariableState.ForceBind(constNode.Identifier.Identifier, value);
    }


    public object? InterpretStatement(StatementNode node, Table<object> shadowVariableState)
    {
        return node switch
        {
            StatementCompositionNode castNode => InterpretCompositionStatement(castNode, shadowVariableState),
            ReturnNode castNode => InterpretExpression(castNode.Expression, shadowVariableState),
            SkipNode => null,
            _ => throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})"),
        };
    }

    public object? InterpretCompositionStatement(StatementCompositionNode node, Table<object> shadowVariableState)
    {

        object? @return = null;
        if (node is VariableDeclarationNode declarationNode)
        {
            InterpretVariableDeclaration(declarationNode, shadowVariableState);
        }
        else if (node is AssignNode assignNode)
        {
            InterpretAssignment(assignNode, shadowVariableState);
        }
        else if (node is IfElseNode ifElseNode)
        {
            @return = InterpretIfElseNode(ifElseNode, shadowVariableState);
        }
        else if (node is WhileNode whileNode)
        {
            @return = InterpretWhileNode(whileNode, shadowVariableState);
        }

        if (@return is not null)
        {
            return @return;
        }

        if (node.NextStatement is not null)
        {
            return InterpretStatement(node.NextStatement, shadowVariableState);
        }
        return null;
    }

    public void InterpretVariableDeclaration(VariableDeclarationNode node, Table<object> shadowVariableState)
    {
        object value = InterpretExpression(node.Expression, shadowVariableState);
        shadowVariableState.ForceBind(node.Identifier.Identifier, value);
    }

    public void InterpretAssignment(AssignNode node, Table<object> shadowVariableState)
    {
        object value = InterpretExpression(node.Expression, shadowVariableState);
        shadowVariableState.ForceBind(node.Identifier.Identifier, value);
    }

    public object? InterpretIfElseNode(IfElseNode node, Table<object> shadowVariableState)
    {
        object condition = InterpretExpression(node.Condition, shadowVariableState);

        return (bool)condition
            ? InterpretStatement(node.IfBody, shadowVariableState)
            : InterpretStatement(node.ElseBody, shadowVariableState);
    }

    public object? InterpretWhileNode(WhileNode node, Table<object> shadowVariableState)
    {

        while ((bool)InterpretExpression(node.Condition, shadowVariableState))
        {
            object? @return = InterpretStatement(node.Body, shadowVariableState);
            if (@return is not null)
            {
                return @return;
            }
        }

        return null;
    }

    public void InterpretNetwork(NetworkNode node)
    {
        if (node is QueueDeclarationNode queue)
        {
            InterpretQueueDeclaration(queue);
        }
        else if (node is NetworkDeclarationNode network)
        {
            InterpretNetworkDeclaration(network);
        }

        else
        {
            throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
        }
    }

    public void InterpretQueueDeclaration(QueueDeclarationNode node)
    {
        int capacity = (int)InterpretExpression(node.Capacity, null);
        int servers = (int)InterpretExpression(node.Servers, null);
        double service() => Convert.ToDouble(InterpretExpression(node.Service, null));
        IEnumerable<string> metrics = node.Metrics.Select(metric => metric.Name);

        QueueTuple queueTuple = new()
        {
            Capacity = capacity,
            Servers = servers,
            Service = service,
            Metrics = metrics,
        };

        VariableState.ForceBind(node.Identifier.Identifier, queueTuple);

        QueueableManager.Queueables.Add(
            new Queue(
                node.Identifier.Identifier,
                servers,
                capacity,
                service,
                metrics
            )
        );
    }

    public void InterpretNetworkDeclaration(NetworkDeclarationNode node)
    {
        NetworkState.ForceBind(node.Identifier.Identifier, node);

        IEnumerable<NetworkInputOrOutput> inputs = node.Inputs.Select(NetworkDefinitionManager.GetInputOrOutput);
        IEnumerable<NetworkInputOrOutput> outputs = node.Outputs.Select(NetworkDefinitionManager.GetInputOrOutput);

        IEnumerable<Queueable> newInstances = node.Instances.Select(QueueableManager.GetNewInstance);

        Network network = new(
            node.Identifier.Identifier,
            inputs,
            outputs,
            newInstances
        );

        double InterpretExpressionAssumeDouble(ExpressionNode expressionNode) => Convert.ToDouble(InterpretExpression(expressionNode, shadowVariableState: null));

        List<Route> routes = [];
        foreach (RouteDefinitionNode routeDefinitionNode in node.Routes)
        {
            routes.AddRange(QueueableManager.GetRoutes(routeDefinitionNode, network, InterpretExpressionAssumeDouble));
        }

        network.Routes = routes;

        QueueableManager.Queueables.Add(network);
    }

    public void InterpretSimulate(SimulateNode simulateNode)
    {
        SimulationEngineAPI engineAPI = new();
        engineAPI.SetSeed(Random.Shared.Next());
        int untilTime = (int)InterpretExpression(simulateNode.TerminationCriteria, shadowVariableState: null);
        int runCount = (int)InterpretExpression(simulateNode.Runs, shadowVariableState: null);

        engineAPI.SetSimulationParameters(untilTime: untilTime, runCount: runCount);

        NetworkEntity networkEntity = QueueableManager.FindNetworkEntityOrThrow(simulateNode.NetworkIdentifier);
        

        if (networkEntity is not Queueable queueable)
        {
            throw new InterpretationException($"Network '{simulateNode.NetworkIdentifier.FullIdentifier}' is not a network (Line: {simulateNode.NetworkIdentifier.LineNumber})");
        }

        CreateQueueableInEngine(engineAPI, queueable, parent: null);

        engineAPI.RunSimulation();
        engineAPI.PrintMetric(engineAPI.GetSimulationStats());
    }

    public List<Action> CreateQueueableInEngine(SimulationEngineAPI engineAPI, Queueable queueable, NetworkDefinition? parent)
    {
        if (queueable is Queue queue)
        {
            if (parent is null)
            {
                throw new InterpretationException($"Queue '{queue.Name}' must be part of a network (At Simulate)");
            }

            CreateQueueInEngine(engineAPI, queue, parent);
            return [];
        }
        else if (queueable is Network network)
        {
            return CreateNetworkInEngine(engineAPI, network, parent);
        }
        else
        {
            throw new InterpretationException($"Queueable '{queueable.Name}' is not a valid queueable (At Simulate)");
        }
    }

    public static void CreateQueueInEngine(SimulationEngineAPI engineAPI, Queue queue, NetworkDefinition networkDefinition)
    {
        networkDefinition.AddQueue(queue.Name, queue.Servers, queue.Capacity, queue.Service);
    }

    public List<Action> CreateNetworkInEngine(SimulationEngineAPI engineAPI, Network network, NetworkDefinition? parent)
    {
        NetworkDefinition networkDefinition = new(parent)
        {
            Name = network.Name
        };

        List<Action> routesToCreate = [];

        foreach (NetworkInputOrOutput input in network.Inputs)
        {
            networkDefinition.AddEntryPoint(input.Name);
        }
        foreach (NetworkInputOrOutput output in network.Outputs)
        {
            networkDefinition.AddExitPoint(output.Name);
        }
        
        foreach (Queueable queueable in network.NewInstances)
        {
            routesToCreate.AddRange(CreateQueueableInEngine(engineAPI, queueable, networkDefinition));
        }

        int index = 0;
        if (network.Routes is not null)
        {
            foreach (Route route in network.Routes)
            {
                routesToCreate.Add(CreateRouteInEngine(engineAPI, networkDefinition, route, index));
                index++;
            }
        }

        if (parent is null)
        {
            foreach (Action routeToCreate in routesToCreate)
            {
                routeToCreate.Invoke();
            }

            

            engineAPI.CreateNetwork(networkDefinition);
        }
        else
        {
            parent.AddSubNetwork(networkDefinition);
        }
        return routesToCreate;
    }

    public Action CreateRouteInEngine(SimulationEngineAPI engineAPI, NetworkDefinition networkDefinition, Route route, int index)
    {
        
        if (route is FuncRoute funcRoute)
        {
            return CreateFunctionRouteInEngine(engineAPI, funcRoute, networkDefinition, index);
        }
        else if (route is NetworkEntityRoute networkEntityRoute)
        {
            return CreateNetworkEntityRouteInEngine(networkEntityRoute, networkDefinition);
        }
        else
        {
            throw new InterpretationException($"Route '{route}' is not a valid route (At Simulate)");
        }
    }

    public static Action CreateFunctionRouteInEngine(SimulationEngineAPI engineAPI, FuncRoute funcRoute, NetworkDefinition networkDefinition, int index)
    {
        string dispatcherName = string.Join('.', networkDefinition.FullName, "@ dispatcher @", index);
        engineAPI.CreateDispatcherNode(
            dispatcherName,
            funcRoute.FromRate
        );

        networkDefinition.AddDispatcher(dispatcherName, funcRoute.FromRate);

        string ToName = string.Join('.', networkDefinition.FullName, funcRoute.ToProbabilityPair.To.Name);

        return () => networkDefinition.Connect(
            dispatcherName,
            ToName,
            funcRoute.ToProbabilityPair.Weight
        );
    }

    private static Action CreateNetworkEntityRouteInEngine(NetworkEntityRoute networkEntityRoute, NetworkDefinition networkDefinition)
    {
        string fromName = string.Join('.', networkDefinition.FullName, networkEntityRoute.FromName);
        string toName = string.Join('.', networkDefinition.FullName, networkEntityRoute.ToProbabilityPair.ToName);
        
        
        return () => networkDefinition.Connect(fromName, toName, networkEntityRoute.ToProbabilityPair.Weight);
    }

    public object InterpretExpression(ExpressionNode node, Table<object>? shadowVariableState)
    {
        return node switch
        {
            IdentifierExpressionNode identifierExpression => InterpretAnyIdentifier(identifierExpression.Identifier, shadowVariableState),
            NegativeNode negativeNode => InterpretNegativeNode(negativeNode, shadowVariableState),
            MultiplyNode multiplyNode => InterpretMultiplyNode(multiplyNode, shadowVariableState),
            DivisionNode divisionNode => InterpretDivisionNode(divisionNode, shadowVariableState),
            AddNode addNode => InterpretAddNode(addNode, shadowVariableState),
            LessThanNode lessThanNode => InterpretLessThanNode(lessThanNode, shadowVariableState),
            EqualNode equalNode => InterpretEqualNode(equalNode, shadowVariableState),
            AndNode andNode => InterpretAndNode(andNode, shadowVariableState),
            NotNode notNode => InterpretNotNode(notNode, shadowVariableState),
            BoolLiteralNode boolNode => boolNode.Value,
            IntLiteralNode intNode => intNode.Value,
            DoubleLiteralNode doubleNode => doubleNode.Value,
            ArrayLiteralNode arrayNode => InterpretArrayNode(arrayNode, shadowVariableState),
            IndexingNode indexingNode => InterpretIndexingNode(indexingNode, shadowVariableState),
            FunctionCallNode functionCallNode => InterpretFunctionCallNode(functionCallNode, shadowVariableState),
            ParenthesesNode parenthesesNode => InterpretExpression(parenthesesNode.Inner, shadowVariableState),
            _ => throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})"),
        };
    }

    public object InterpretNegativeNode(NegativeNode negativeNode, Table<object>? shadowVariableState)
    {
        object innerValue = InterpretExpression(negativeNode.Inner, shadowVariableState);

        return innerValue switch
        {
            int intValue => -intValue,
            double doubleValue => -doubleValue,
            _ => throw new InterpretationException($"{nameof(negativeNode)} unhandled (Line {negativeNode.LineNumber})"),
        };
    }

    public object InterpretMultiplyNode(MultiplyNode multiplyNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(multiplyNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(multiplyNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left * right,
            (int left, double right) => left * right,
            (double left, int right) => left * right,
            (double left, double right) => left * right,
            (_, _) => throw new InterpretationException($"{nameof(multiplyNode)} unhandled (Line {multiplyNode.LineNumber})"),
        };
    }

    public object InterpretDivisionNode(DivisionNode divisionNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(divisionNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(divisionNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => (int)((int)left / (int)right),
            (int left, double right) => left / right,
            (double left, int right) => left / right,
            (double left, double right) => left / right,
            (_, _) => throw new InterpretationException($"{nameof(divisionNode)} unhandled (Line {divisionNode.LineNumber})"),
        };
    }

    public object InterpretAddNode(AddNode addNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(addNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(addNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left + right,
            (int left, double right) => left + right,
            (double left, int right) => left + right,
            (double left, double right) => left + right,
            (_, _) => throw new InterpretationException($"{nameof(addNode)} unhandled (Line {addNode.LineNumber})"),
        };
    }

    public bool InterpretLessThanNode(LessThanNode lessThanNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(lessThanNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(lessThanNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left < right,
            (int left, double right) => left < right,
            (double left, int right) => left < right,
            (double left, double right) => left < right,
            (_, _) => throw new InterpretationException($"{nameof(lessThanNode)} unhandled (Line {lessThanNode.LineNumber})"),
        };
    }

    public bool InterpretEqualNode(EqualNode equalNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(equalNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(equalNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left == right,
            (int left, double right) => left == right,
            (double left, int right) => left == right,
            (double left, double right) => left == right,
            (bool left, bool right) => left == right,
            (_, _) => throw new InterpretationException($"{nameof(equalNode)} unhandled (Line {equalNode.LineNumber})"),
        };
    }

    public bool InterpretAndNode(AndNode andNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(andNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(andNode.Right, shadowVariableState);

        if (leftValue is bool leftBool)
        {
            if (leftBool == false) return false;

            else if (rightValue is bool rightBool)
            {
                return rightBool;
            }
            else
            {
                throw new InterpretationException($"{nameof(andNode)} unhandled (Line {andNode.LineNumber})");
            }
        }

        else
        {
            throw new InterpretationException($"{nameof(andNode)} unhandled (Line {andNode.LineNumber})");
        }
    }

    public bool InterpretNotNode(NotNode notNode, Table<object>? shadowVariableState)
    {
        object innerValue = InterpretExpression(notNode.Inner, shadowVariableState);

        if (innerValue is bool boolValue)
        {
            return !boolValue;
        }

        else
        {
            throw new InterpretationException($"{nameof(notNode)} unhandled (Line {notNode.LineNumber})");
        }
    }

    public object[] InterpretArrayNode(ArrayLiteralNode arrayNode, Table<object>? shadowVariableState)
    {
        object[] array = new object[arrayNode.Elements.Count];

        int index = 0;
        foreach (ExpressionNode expression in arrayNode.Elements)
        {
            array[index] = InterpretExpression(expression, shadowVariableState);

            index++;
        }

        return array;
    }

    public object InterpretIndexingNode(IndexingNode indexingNode, Table<object>? shadowVariableState)
    {
        object target = InterpretAnyIdentifier(indexingNode.Target, shadowVariableState);
        object index = InterpretExpression(indexingNode.Index, shadowVariableState);

        return (target, index) switch
        {
            (object[] array, int indexValue) => array[indexValue],
            (_, _) => throw new InterpretationException($"{nameof(indexingNode)} unhandled (Line {indexingNode.LineNumber})"),
        };
    }

    public object InterpretFunctionCallNode(FunctionCallNode functionCallNode, Table<object>? shadowVariableState)
    {
        List<object> parameterValues = [];
        foreach (ExpressionNode expression in functionCallNode.ActualParameters)
        {
            object value = InterpretExpression(expression, shadowVariableState);
            if (value is not null)
            {
                parameterValues.Add(value);
            }
        }

        object identifierValue = InterpretAnyIdentifier(functionCallNode.Identifier, shadowVariableState);

        if (identifierValue is FunctionStateTuple state)
        {
            Table<object>? shadowState = new(VariableState);

            int index = 0;
            foreach (TypeAndIdentifier parameter in state.Function.Parameters)
            {
                shadowState.ForceBind(parameter.Identifier.Identifier, parameterValues[index]);
                index++;
            }

            object? @return = InterpretStatement(state.Function.Body, shadowState);
            if (@return is not null)
            {
                return @return;
            }
            else
            {
                throw new InterpretationException($"Function '{functionCallNode.Identifier.FullIdentifier}' did not return a value (Line: {functionCallNode.Identifier.LineNumber})");
            }
        }

        else
        {
            throw new InterpretationException($"{nameof(functionCallNode)} unhandled (Line {functionCallNode.LineNumber})");
        }
    }

    public object InterpretAnyIdentifier(IdentifierNode node, Table<object>? shadowVariableState)
    {
        if (node is SingleIdentifierNode singleIdentifierNode)
        {
            if (shadowVariableState is not null && LookupVariableHelper(singleIdentifierNode.Identifier, shadowVariableState, out object? @out))
            {
                return @out;
            }
            else
            {
                return InterpretEnvironmentIdentifier(singleIdentifierNode, globalEnvironment);
            }
        }
        else if (node is QualifiedIdentifierNode qualifiedIdentifierNode)
        {
            SingleIdentifierNode leftIdentifier = qualifiedIdentifierNode.LeftIdentifier;
            SingleIdentifierNode rightIdentifier = qualifiedIdentifierNode.RightIdentifier;
            object leftValue = InterpretEnvironmentIdentifier(leftIdentifier, globalEnvironment);
            if (leftValue is InterpretationEnvironment dependency)
            {
                return InterpretEnvironmentIdentifier(rightIdentifier, dependency);
            }
            else if (leftValue is NetworkDeclarationNode networkNode)
            {
                return InterpretNetworkIdentifier(rightIdentifier, networkNode);
            }
            else
            {
                throw new InterpretationException($"Invalid use of identifier '{qualifiedIdentifierNode.FullIdentifier}' (Line: {qualifiedIdentifierNode.LineNumber})");
            }
        }

        else
        {
            throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
        }
    }

    public static object InterpretEnvironmentIdentifier(SingleIdentifierNode node, InterpretationEnvironment environment)
    {
        if (environment.FunctionState.Lookup(node.Identifier, out FunctionStateTuple function))
        {
            return function!;
        }
        else if (environment.VariableState.Lookup(node.Identifier, out object? variable))
        {
            return variable!;
        }
        else if (environment.NetworkState.Lookup(node.Identifier, out NetworkDeclarationNode? network))
        {
            return network!;
        }
        else if (environment.ModuleDependencies.Lookup(node.Identifier, out InterpretationEnvironment moduleDependency))
        {
            return moduleDependency!;
        }

        else
        {
            throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
        }
    }

    public object InterpretNetworkIdentifier(SingleIdentifierNode identifier, NetworkDeclarationNode network)
    {
        IdentifierNode? networkInstance = network.Instances.FirstOrDefault(instance => instance.NewInstance.Identifier == identifier.Identifier)?.ExistingInstance;

        if (networkInstance is not null)
        {
            object interpretedInstance = InterpretAnyIdentifier(networkInstance, shadowVariableState: null);

            if (IsNetworkOrQueueTuple(interpretedInstance))
            {
                return interpretedInstance;
            }
            else
            {
                throw new InterpretationException($"Identifier error occured for '{networkInstance.FullIdentifier}' (Line: {networkInstance.LineNumber})");
            }
        }

        else
        {
            throw new InterpretationException($"Identifier error occured for '{identifier.Identifier}' (Line: {identifier.LineNumber})");
        }
    }

    public static bool IsNetworkOrQueueTuple(object @object) => @object is NetworkDeclarationNode || @object is QueueTuple;

    public bool LookupVariableHelper(string identifier, Table<object>? shadowVariableState, [MaybeNullWhen(false)] out object @out)
    {
        if (shadowVariableState is not null)
        {
            if (shadowVariableState.Lookup(identifier, out @out))
            {
                if (@out is not null)
                {
                    return true;
                }
            }
        }

        return VariableState.Lookup(identifier, out @out);
    }
}