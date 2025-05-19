


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

namespace Interpreter.SemanticAnalysis;

public class InterpreterClass(ProgramNode node)
{
    public InterpretationEnvironment GlobalEnvironment => globalEnvironment;
    InterpretationEnvironment globalEnvironment = InterpretationEnvironment.Empty(node);

    Table<FunctionStateTuple> FunctionState => globalEnvironment.FunctionState;
    Table<object> VariableState => globalEnvironment.VariableState;
    Table<NetworkDeclarationNode> NetworkState => globalEnvironment.NetworkState;

    QueueableManager QueueableManager => globalEnvironment.QueueableManager;

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

        throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
    }

    public void InterpretImport(ImportNode node)
    {
        InterpretationEnvironment dependency = ModuleLoader.LoadModuleByName(node.Namespace.Identifier);
        globalEnvironment.ModuleDependencies.ForceBind(node.Namespace.Identifier, dependency);
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

        throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
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

        throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
    }

    public void InterpretQueueDeclaration(QueueDeclarationNode node)
    {
        int capacity = (int)InterpretExpression(node.Capacity, null);
        int servers = (int)InterpretExpression(node.Servers, null);
        double service() => (double)InterpretExpression(node.Service, null);
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

        IEnumerable<Queue> inputs = node.Inputs.Select(QueueableManager.IdentifierToInstantQueue);
        IEnumerable<Queue> outputs = node.Outputs.Select(QueueableManager.IdentifierToInstantQueue);

        IEnumerable<Queueable> newInstances = node.Instances.Select(QueueableManager.GetNewInstance);

        List<Route> routes = [];
        foreach (RouteDefinitionNode routeDefinitionNode in node.Routes)
        {
            routes.AddRange(QueueableManager.GetRoute(routeDefinitionNode, inputs, outputs, [.. newInstances], this));
        }

        QueueableManager.Queueables.Add(
            new Network(
                node.Identifier.Identifier,
                inputs,
                outputs,
                newInstances,
                routes
            )
        );
    }

    public void InterpretSimulate(SimulateNode simulateNode)
    {
        SimulationEngineAPI engineAPI = new();
        engineAPI.SetSeed(Random.Shared.Next());

        int untilTime = (int)InterpretExpression(simulateNode.TerminationCriteria, shadowVariableState: null);
        int runCount = (int)InterpretExpression(simulateNode.Runs, shadowVariableState: null);

        engineAPI.SetSimulationParameters(untilTime: untilTime, runCount: runCount);

        IdentifierNode networkIdentifier = simulateNode.NetworkIdentifier;
        Queueable queueable = QueueableManager.FindQueueable(networkIdentifier.FirstIdentifier);

        if (networkIdentifier is QualifiedIdentifierNode qualifiedIdentifierNode)
        {
            queueable = queueable.FindQueueable(qualifiedIdentifierNode.RightIdentifier.Identifier);
        }

        CreateQueueableInEngine(engineAPI, queueable, networkIdentifier.FirstIdentifier);
    }

    public void CreateQueueableInEngine(SimulationEngineAPI engineAPI, Queueable queueable, string thisNetworkName)
    {
        string queueableName = string.Join('.', thisNetworkName, queueable.Name);
        if (queueable is Queue queue)
        {
            CreateQueueInEngine(engineAPI, queue, queueableName);
        }
        else if (queueable is Network network)
        {
            CreateNetworkInEngine(engineAPI, network, queueableName);
        }
    }

    public static void CreateQueueInEngine(SimulationEngineAPI engineAPI, Queue queue, string queueName)
    {
        engineAPI.CreateQueueNode(
            queueName,
            queue.Servers,
            queue.Capacity,
            queue.Service
        );
    }

    public void CreateNetworkInEngine(SimulationEngineAPI engineAPI, Network network, string networkName)
    {
        foreach (Queue queue in network.Inputs)
        {
            CreateQueueInEngine(engineAPI, queue, networkName);
        }
        foreach (Queue queue in network.Outputs)
        {
            CreateQueueInEngine(engineAPI, queue, networkName);
        }
        foreach (Queueable queueable in network.NewInstances)
        {
            CreateQueueableInEngine(engineAPI, queueable, networkName);
        }

        int index = 0;
        foreach (Route route in network.Routes)
        {
            CreateRouteInEngine(engineAPI, route, networkName, index);
            index++;
        }
    }

    public void CreateRouteInEngine(SimulationEngineAPI engineAPI, Route route, string networkName, int index)
    {
        if (route is FuncRoute funcRoute)
        {
            CreateFunctionRouteInEngine(engineAPI, funcRoute, networkName, index);
        }
        else if (route is QueueRoute queueRoute)
        {
            CreateQueueRouteInEngine(engineAPI, queueRoute, networkName);
        }
    }

    public static void CreateFunctionRouteInEngine(SimulationEngineAPI engineAPI, FuncRoute funcRoute, string networkName, int index)
    {
        string dispatcherIdentifier = "dispatcher" + index;
        string dispatcherName = string.Join('.', networkName, dispatcherIdentifier);
        engineAPI.CreateDispatcherNode(
            dispatcherName,
            funcRoute.FromRate
        );

        // Maybe need to find the network which contains the to queue
        string queueName = string.Join('.', networkName, funcRoute.To);
        engineAPI.ConnectNode(dispatcherName, queueName, funcRoute.To.Weight);
    }

    private static void CreateQueueRouteInEngine(SimulationEngineAPI engineAPI, QueueRoute queueRoute, string networkName)
    {
        // Maybe need to find the network which contains the from and to queues
        string fromQueueName = string.Join('.', networkName, queueRoute.FromQueue.Name);
        string toQueueName = string.Join('.', networkName, queueRoute.To.ToQueue);

        engineAPI.ConnectNode(fromQueueName, toQueueName, queueRoute.To.Weight);
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

    public double InterpretNegativeNode(NegativeNode negativeNode, Table<object>? shadowVariableState)
    {
        object innerValue = InterpretExpression(negativeNode.Inner, shadowVariableState);

        return innerValue switch
        {
            int intValue => -intValue,
            double doubleValue => -doubleValue,
            _ => throw new InterpretationException($"{nameof(negativeNode)} unhandled (Line {negativeNode.LineNumber})"),
        };
    }

    public double InterpretMultiplyNode(MultiplyNode multiplyNode, Table<object>? shadowVariableState)
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
            (int left, int right) => left / right,
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
        }

        throw new InterpretationException($"{nameof(andNode)} unhandled (Line {andNode.LineNumber})");
    }

    public bool InterpretNotNode(NotNode notNode, Table<object>? shadowVariableState)
    {
        object innerValue = InterpretExpression(notNode.Inner, shadowVariableState);

        if (innerValue is bool boolValue)
        {
            return !boolValue;
        }

        throw new InterpretationException($"{nameof(notNode)} unhandled (Line {notNode.LineNumber})");
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
        }

        throw new InterpretationException($"{nameof(functionCallNode)} unhandled (Line {functionCallNode.LineNumber})");
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

        throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
    }

    public static object InterpretEnvironmentIdentifier(SingleIdentifierNode node, InterpretationEnvironment environment)
    {
        if (environment.FunctionState.Lookup(node.Identifier, out FunctionStateTuple function))
        {
            return function;
        }
        else if (environment.VariableState.Lookup(node.Identifier, out object? variable))
        {
            return variable;
        }
        else if (environment.NetworkState.Lookup(node.Identifier, out NetworkDeclarationNode? network))
        {
            return network;
        }
        else if (environment.ModuleDependencies.Lookup(node.Identifier, out InterpretationEnvironment moduleDependency))
        {
            return moduleDependency;
        }

        throw new InterpretationException($"{nameof(node)} unhandled (Line {node.LineNumber})");
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