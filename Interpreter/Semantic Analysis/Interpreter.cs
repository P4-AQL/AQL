


using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;

namespace Interpreter.SemanticAnalysis;
public class Interpreter
{
    readonly Environment<FunctionState> FunctionState = new();
    readonly Environment<object> VariableState = new();
    readonly Environment<NetworkDeclarationNode> NetworkState = new();

    readonly List<string> Errors = [];

    public void InterpretProgram(ProgramNode node)
    {
        if (node is ImportNode importNode)
        {
            Console.WriteLine("We imported" + importNode.Namespace.Identifier);
        }
        else if (node is DefinitionProgramNode definitionNode)
        {
            InterpretDefinition(definitionNode.Definition);
        }
    }

    private void InterpretDefinition(DefinitionNode node)
    {
        if (node is FunctionNode functionNode)
        {
            FunctionState functionState = new(function: functionNode, variableState: VariableState);
            FunctionState.TryBindIfNotExists(functionNode.Identifier.Identifier, functionState);
        }
        else if (node is ConstDeclarationNode constNode)
        {
            object value = InterpretExpression(constNode.Expression);
        }
        else if (node is NetworkDefinitionNode networkNode)
        {

        }
        else if (node is SimulateNode simulateNode)
        {

        }
    }

    private void InterpretStatement(StatementNode node)
    {

    }

    private void InterpretNetwork(NetworkNode node)
    {

    }

    private object InterpretExpression(ExpressionNode node)
    {
        return node switch
        {
            IdentifierExpressionNode identifierExpression => InterpretIdentifier(identifierExpression.Identifier),
            NegativeNode negativeNode => InterpretNegativeNode(negativeNode),
            MultiplyNode multiplyNode => InterpretMultiplyNode(multiplyNode),
            DivisionNode divisionNode => InterpretDivisionNode(divisionNode),
            AddNode addNode => InterpretAddNode(addNode),
            LessThanNode lessThanNode => InterpretLessThanNode(lessThanNode),
            EqualNode equalNode => InterpretEqualNode(equalNode),
            AndNode andNode => InterpretAndNode(andNode),
            NotNode notNode => InterpretNotNode(notNode),
            BoolLiteralNode boolNode => InterpretBoolNode(boolNode),
            IntLiteralNode intNode => InterpretIntNode(intNode),
            DoubleLiteralNode doubleNode => InterpretDoubleNode(doubleNode),
            ArrayLiteralNode arrayNode => InterpretArrayNode(arrayNode),
            IndexingNode indexingNode => InterpretIndexingNode(indexingNode),
            FunctionCallNode functionCallNode => InterpretFunctionCallNode(functionCallNode),
            ParenthesesNode parenthesesNode => InterpretParenthesesNode(parenthesesNode),
            _ => throw new($"Expression could not be interpreted (Line {node.LineNumber})"),
        };
    }

    private object InterpretNegativeNode(NegativeNode negativeNode)
    {
        object innerValue = InterpretExpression(negativeNode.Inner);

        return innerValue switch
        {
            int intValue => -intValue,
            double doubleValue => -doubleValue,
            _ => throw new($"{nameof(negativeNode)} unhandled (Line {negativeNode.LineNumber})"),
        };
    }

    private object InterpretDivisionNode(DivisionNode divisionNode)
    {
        object leftValue = InterpretExpression(divisionNode.Left);
        object rightValue = InterpretExpression(divisionNode.Right);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left / right,
            (int left, double right) => left / right,
            (double left, int right) => left / right,
            (double left, double right) => left / right,
            (_, _) => throw new($"{nameof(divisionNode)} unhandled (Line {divisionNode.LineNumber})"),
        };
    }

    private object InterpretAddNode(AddNode addNode)
    {
        object leftValue = InterpretExpression(addNode.Left);
        object rightValue = InterpretExpression(addNode.Right);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left + right,
            (int left, double right) => left + right,
            (double left, int right) => left + right,
            (double left, double right) => left + right,
            (_, _) => throw new($"{nameof(addNode)} unhandled (Line {addNode.LineNumber})"),
        };
    }

    private bool InterpretLessThanNode(LessThanNode lessThanNode)
    {
        object leftValue = InterpretExpression(lessThanNode.Left);
        object rightValue = InterpretExpression(lessThanNode.Right);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left < right,
            (int left, double right) => left < right,
            (double left, int right) => left < right,
            (double left, double right) => left < right,
            (_, _) => throw new($"{nameof(lessThanNode)} unhandled (Line {lessThanNode.LineNumber})"),
        };
    }

    private bool InterpretEqualNode(EqualNode equalNode)
    {
        object leftValue = InterpretExpression(equalNode.Left);
        object rightValue = InterpretExpression(equalNode.Right);

        return (leftValue, rightValue) switch
        {
            (int left, int right) => left == right,
            (double left, double right) => left == right,
            (bool left, bool right) => left == right,
            (_, _) => throw new($"{nameof(equalNode)} unhandled (Line {equalNode.LineNumber})"),
        };
    }

    private bool InterpretAndNode(AndNode andNode)
    {
        object leftValue = InterpretExpression(andNode.Left);
        object rightValue = InterpretExpression(andNode.Right);

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

    private bool InterpretNotNode(NotNode notNode)
    {
        object innerValue = InterpretExpression(notNode.Inner);

        if (innerValue is bool boolValue)
        {
            return !boolValue;
        }

        throw new($"{nameof(notNode)} unhandled (Line {notNode.LineNumber})");
    }

    private bool InterpretBoolNode(BoolLiteralNode boolNode)
    {
        return boolNode.Value;
    }

    private int InterpretIntNode(IntLiteralNode intNode)
    {
        return intNode.Value;
    }

    private double InterpretDoubleNode(DoubleLiteralNode doubleNode)
    {
        return doubleNode.Value;
    }

    private object[] InterpretArrayNode(ArrayLiteralNode arrayNode)
    {
        object[] array = new object[arrayNode.Elements.Count];

        int index = 0;
        foreach (ExpressionNode expression in arrayNode.Elements)
        {
            array[index] = InterpretExpression(expression);

            index++;
        }

        return array;
    }

    private object InterpretIndexingNode(IndexingNode indexingNode)
    {
        object target = InterpretIdentifier(indexingNode.Target);
        object index = InterpretExpression(indexingNode.Index);

        return (target, index) switch
        {
            (object[] array, int indexValue) => array[indexValue],
            (_, _) => throw new($"{nameof(indexingNode)} unhandled (Line {indexingNode.LineNumber})"),
        };
    }

    private object InterpretFunctionCallNode(FunctionCallNode functionCallNode)
    {
        throw new NotImplementedException();
    }

    private object InterpretParenthesesNode(ParenthesesNode parenthesesNode)
    {
        throw new NotImplementedException();
    }

    private object InterpretMultiplyNode(MultiplyNode multiplyNode)
    {
        throw new NotImplementedException();
    }

    private void InterpretRoute(RouteNode node)
    {

    }

    private object InterpretIdentifier(IdentifierNode node)
    {
        if (node is SingleIdentifierNode singleIdentifierNode)
        {
            if (VariableState.LookUp(singleIdentifierNode.Identifier, out object? boundValue))
            {
                return boundValue;
            }
            else
            {
                string error = $"{singleIdentifierNode.Identifier} does not exist (Line: {singleIdentifierNode.LineNumber})";
                throw new(error);
            }
        }
        else if (node is QualifiedIdentifierNode qualifiedIdentifierNode)
        {
            throw new NotImplementedException();
        }

        throw new("This is not possible");
    }
}