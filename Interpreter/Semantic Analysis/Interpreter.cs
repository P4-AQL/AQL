


using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;

namespace Interpreter.SemanticAnalysis;
public class Interpreter
{
    Environment globalEnvironment = new();

    public void InterpretProgram(ProgramNode node)
    {
        if (node is ImportNode importNode)
        {

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
            InterpretStatement(functionNode.Body);
        }
        else if (node is ConstDeclarationNode constNode)
        {

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

    private void InterpretExpression(ExpressionNode node)
    {

    }

    private void InterpretRoute(RouteNode node)
    {

    }

    private void InterpretIdentifier(SingleIdentifierNode node)
    {

    }
}