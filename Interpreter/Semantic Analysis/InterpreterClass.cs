


using System.Diagnostics.CodeAnalysis;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Statements;
using Interpreter.Utilities.Modules;

namespace Interpreter.SemanticAnalysis;
public class InterpreterClass
{
    InterpretationEnvironment globalEnvironment;

    Table<FunctionStateTuple> FunctionState => globalEnvironment.FunctionState;
    Table<object> VariableState => globalEnvironment.VariableState;
    Table<NetworkDeclarationNode> NetworkState => globalEnvironment.NetworkState;


    public InterpretationEnvironment StartInterpretation(ProgramNode node)
    {
        globalEnvironment = InterpretationEnvironment.Empty(node);
        try
        {
            InterpretProgram(node);
        }
        catch (Exception ex)
        {
            globalEnvironment.Errors.Add(ex.Message);
        }

        return globalEnvironment;
    }

    private void InterpretProgram(ProgramNode node)
    {
        if (node is ImportNode importNode)
        {
            Console.WriteLine("We imported" + importNode.Namespace.Identifier);
        }
        else if (node is DefinitionProgramNode definitionNode)
        {
            InterpretDefinition(definitionNode.Definition);
        }

        throw new($"{nameof(node)} unhandled (Line {node.LineNumber})");
    }

    public void InterpretImport(ImportNode node)
    {
        InterpretationEnvironment dependency = ModuleLoader.LoadModuleByName(node.Namespace.Identifier);
        globalEnvironment.ModuleDependencies.ForceBind(node.Namespace.Identifier, dependency);
    }

    private void InterpretDefinition(DefinitionNode node)
    {
        if (node is DefinitionCompositionNode compositionNode)
        {
            InterpretDefinitionComposition(compositionNode);
        }
        else if (node is SimulateNode simulateNode)
        {
            InterpretSimulate(simulateNode);
        }

        throw new($"{nameof(node)} unhandled (Line {node.LineNumber})");
    }

    private void InterpretDefinitionComposition(DefinitionCompositionNode node)
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

    private void InterpretFunctionDeclaration(FunctionNode functionNode)
    {
        FunctionStateTuple functionState = new(function: functionNode, variableState: VariableState);
        FunctionState.ForceBind(functionNode.Identifier.Identifier, functionState);
    }

    private void InterpretConstDeclaration(ConstDeclarationNode constNode)
    {
        object value = InterpretExpression(constNode.Expression, shadowVariableState: null);
        VariableState.ForceBind(constNode.Identifier.Identifier, value);
    }


    private object? InterpretStatement(StatementNode node, Table<object> shadowVariableState)
    {
        return node switch
        {
            StatementCompositionNode castNode => InterpretCompositionStatement(castNode, shadowVariableState),
            ReturnNode castNode => InterpretExpression(castNode.Expression, shadowVariableState),
            SkipNode => null,
            _ => throw new($"{nameof(node)} unhandled (Line {node.LineNumber})"),
        };
    }

    private object? InterpretCompositionStatement(StatementCompositionNode node, Table<object> shadowVariableState)
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

    private void InterpretVariableDeclaration(VariableDeclarationNode node, Table<object> shadowVariableState)
    {
        object value = InterpretExpression(node.Expression, shadowVariableState);
        shadowVariableState.ForceBind(node.Identifier.Identifier, value);
    }

    private void InterpretAssignment(AssignNode node, Table<object> shadowVariableState)
    {
        object value = InterpretExpression(node.Expression, shadowVariableState);
        shadowVariableState.ForceBind(node.Identifier.Identifier, value);
    }

    private object? InterpretIfElseNode(IfElseNode node, Table<object> shadowVariableState)
    {
        object condition = InterpretExpression(node.Condition, shadowVariableState);

        return (bool)condition
            ? InterpretStatement(node.IfBody, shadowVariableState)
            : InterpretStatement(node.ElseBody, shadowVariableState);
    }

    private object? InterpretWhileNode(WhileNode node, Table<object> shadowVariableState)
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

    private void InterpretNetwork(NetworkNode node)
    {
        if (node is QueueDeclarationNode queue)
        {
            InterpretQueueDeclaration(queue);
        }
        else if (node is NetworkDeclarationNode network)
        {
            InterpretNetworkDeclaration(network);
        }

        throw new($"{nameof(node)} unhandled (Line {node.LineNumber})");
    }

    private void InterpretQueueDeclaration(QueueDeclarationNode node)
    {
        int capacity = (int)InterpretExpression(node.Capacity, null);
        int servers = (int)InterpretExpression(node.NumberOfServers, null);
        object service() => InterpretExpression(node.Service, null);
        IEnumerable<string> metrics = node.Metrics.Select(metric => metric.Name);

        QueueTuple queueTuple = new()
        {
            Capacity = capacity,
            Servers = servers,
            Service = service,
            Metrics = metrics,
        };

        VariableState.ForceBind(node.Identifier.Identifier, queueTuple);
    }

    private void InterpretNetworkDeclaration(NetworkDeclarationNode node)
    {
        NetworkState.ForceBind(node.Identifier.Identifier, node);

        SimulationEngineAPI engineAPI = new();
        foreach (InstanceDeclaration instanceDeclaration in node.Instances)
        {
            CreateQueueInEngine(engineAPI, node.Identifier.Identifier, instanceDeclaration);
        }
    }

    private void CreateQueueInEngine(SimulationEngineAPI engineAPI, string networkIdentifier, InstanceDeclaration instance)
    {
        object existingNetwork = InterpretIdentifier(instance.ExistingInstance, shadowVariableState: null);
        if (existingNetwork is not NetworkNode existingNetworkNode)
        {
            throw new($"Not a valid instance! (Line: {instance.LineNumber})");
        }

        string queueFullPath = networkIdentifier + "." + instance.NewInstance.Identifier;
        CreateQueueInEngine(engineAPI, queueFullPath, existingNetworkNode);
    }

    private void CreateQueueInEngine(SimulationEngineAPI engineAPI, string networkIdentifier, NetworkNode network)
    {
        if (network is NetworkDeclarationNode networkDeclarationNode)
        {

        }
        else if (network is QueueDeclarationNode queueDeclarationNode)
        {

        }
    }

    private void InterpretSimulate(SimulateNode simulateNode)
    {
        throw new NotImplementedException();
    }

    private object InterpretExpression(ExpressionNode node, Table<object>? shadowVariableState)
    {
        return node switch
        {
            IdentifierExpressionNode identifierExpression => InterpretIdentifier(identifierExpression.Identifier, shadowVariableState),
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
            _ => throw new($"{nameof(node)} unhandled (Line {node.LineNumber})"),
        };
    }

    private double InterpretNegativeNode(NegativeNode negativeNode, Table<object>? shadowVariableState)
    {
        object innerValue = InterpretExpression(negativeNode.Inner, shadowVariableState);

        return innerValue switch
        {
            int intValue => -intValue,
            double doubleValue => -doubleValue,
            _ => throw new($"{nameof(negativeNode)} unhandled (Line {negativeNode.LineNumber})"),
        };
    }

    private double InterpretMultiplyNode(MultiplyNode multiplyNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(multiplyNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(multiplyNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left * right,
            (int left, double right) => left * right,
            (double left, int right) => left * right,
            (double left, double right) => left * right,
            (_, _) => throw new($"{nameof(multiplyNode)} unhandled (Line {multiplyNode.LineNumber})"),
        };
    }

    private object InterpretDivisionNode(DivisionNode divisionNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(divisionNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(divisionNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left / right,
            (int left, double right) => left / right,
            (double left, int right) => left / right,
            (double left, double right) => left / right,
            (_, _) => throw new($"{nameof(divisionNode)} unhandled (Line {divisionNode.LineNumber})"),
        };
    }

    private double InterpretAddNode(AddNode addNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(addNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(addNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left + right,
            (int left, double right) => left + right,
            (double left, int right) => left + right,
            (double left, double right) => left + right,
            (_, _) => throw new($"{nameof(addNode)} unhandled (Line {addNode.LineNumber})"),
        };
    }

    private bool InterpretLessThanNode(LessThanNode lessThanNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(lessThanNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(lessThanNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left < right,
            (int left, double right) => left < right,
            (double left, int right) => left < right,
            (double left, double right) => left < right,
            (_, _) => throw new($"{nameof(lessThanNode)} unhandled (Line {lessThanNode.LineNumber})"),
        };
    }

    private bool InterpretEqualNode(EqualNode equalNode, Table<object>? shadowVariableState)
    {
        object leftValue = InterpretExpression(equalNode.Left, shadowVariableState);
        object rightValue = InterpretExpression(equalNode.Right, shadowVariableState);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left == right,
            (double left, double right) => left == right,
            (bool left, bool right) => left == right,
            (_, _) => throw new($"{nameof(equalNode)} unhandled (Line {equalNode.LineNumber})"),
        };
    }

    private bool InterpretAndNode(AndNode andNode, Table<object>? shadowVariableState)
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

        throw new($"{nameof(andNode)} unhandled (Line {andNode.LineNumber})");
    }

    private bool InterpretNotNode(NotNode notNode, Table<object>? shadowVariableState)
    {
        object innerValue = InterpretExpression(notNode.Inner, shadowVariableState);

        if (innerValue is bool boolValue)
        {
            return !boolValue;
        }

        throw new($"{nameof(notNode)} unhandled (Line {notNode.LineNumber})");
    }

    private object[] InterpretArrayNode(ArrayLiteralNode arrayNode, Table<object>? shadowVariableState)
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

    private object InterpretIndexingNode(IndexingNode indexingNode, Table<object>? shadowVariableState)
    {
        object target = InterpretIdentifier(indexingNode.Target, shadowVariableState);
        object index = InterpretExpression(indexingNode.Index, shadowVariableState);

        return (target, index) switch
        {
            (object[] array, int indexValue) => array[indexValue],
            (_, _) => throw new($"{nameof(indexingNode)} unhandled (Line {indexingNode.LineNumber})"),
        };
    }

    private object InterpretFunctionCallNode(FunctionCallNode functionCallNode, Table<object>? shadowVariableState)
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

        if (FunctionState.Lookup(functionCallNode.Identifier.Identifier, out FunctionStateTuple state))
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

        throw new($"{nameof(functionCallNode)} unhandled (Line {functionCallNode.LineNumber})");
    }

    enum WhosDoingTheInterpreting
    {
        other = 0,
        import = 1,
        networks = 2,
    }

    private object InterpretIdentifier(IdentifierNode node, Table<object>? shadowVariableState, WhosDoingTheInterpreting whosDoingTheInterpreting = WhosDoingTheInterpreting.other)
    {
        if (node is SingleIdentifierNode singleIdentifierNode)
        {
            if (LookupVariableHelper(singleIdentifierNode.Identifier, shadowVariableState, out object? boundValue))
            {
                return boundValue;
            }
        }
        else if (node is QualifiedIdentifierNode qualifiedIdentifierNode)
        {
            if (whosDoingTheInterpreting == WhosDoingTheInterpreting.import)
            {
                InterpretImportIdentifier(qualifiedIdentifierNode);
            }
            else if (whosDoingTheInterpreting == WhosDoingTheInterpreting.networks)
            {

            }
        }

        throw new($"{nameof(node)} unhandled (Line {node.LineNumber})");
    }

    private object InterpretImportIdentifier(QualifiedIdentifierNode node)
    {
        string nodeIdentifier = node.LeftIdentifier.Identifier;
        string expressionIdentifier = node.RightIdentifier.Identifier;
        if (globalEnvironment.ModuleDependencies.Lookup(nodeIdentifier, out InterpretationEnvironment dependency))
        {
            if (dependency.FunctionState.Lookup(expressionIdentifier, out FunctionStateTuple functionDependency))
            {
                return functionDependency;
            }
            else if (dependency.VariableState.Lookup(expressionIdentifier, out object? variableDependency))
            {
                return variableDependency;
            }
            else if (dependency.NetworkState.Lookup(expressionIdentifier, out NetworkDeclarationNode? networkDependency))
            {
                return networkDependency;
            }
        }

        throw new($"{nameof(node)} unhandled (Line {node.LineNumber})");
    }

    private bool LookupVariableHelper(string identifier, Table<object>? shadowVariableState, [MaybeNullWhen(false)] out object @out)
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