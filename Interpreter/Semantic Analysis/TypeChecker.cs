using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public class TypeChecker
{
    // env for definitions and localEnv for statements? Return localEnv as new so it is not referenced
    public List<string> TypeCheckNode(Node node, Table<TypeNode> globalEnvironment, Table<Table<TypeNode>> localNetworkScopesEnvironment, List<string> errors, Table<TypeNode>? localEnv = null)
    {
        // Check the node and create localEnv if neccesary
        if (node is DefinitionNode defNode)
        {
            // We don't need localEnv because these definition can only be global.
            TypeCheckDefinitionNode(defNode, globalEnvironment, localNetworkScopesEnvironment, errors);

        }
        else if (node is StatementNode stmtNode)
        {

        }


        // Return errors to parent node type check
        return errors;
    }

    private void TypeCheckChildren(Node parentNode, Table<TypeNode> globalEnvironment, Table<Table<TypeNode>> localNetworkScopesEnvironment, List<string> errors, Table<TypeNode>? localEnv = null)
    {
        //Type check child nodes
        foreach (Node childNode in parentNode.GetChildren())
        {
            if (localEnv is null)
            {
                // localEnv being null means that we are in global scope
                errors.AddRange(TypeCheckNode(childNode, globalEnvironment, localNetworkScopesEnvironment, errors, null));
            }
            else
            {
                errors.AddRange(TypeCheckNode(childNode, globalEnvironment, localNetworkScopesEnvironment, errors, localEnv));
            }
        }
    }

    private TypeNode GetTypeOfExpression(ExpressionNode expressionNode) {
        throw new NotImplementedException();
    }

    private void TypeCheckDefinitionNode(Node defNode, Table<TypeNode> globalEnvironment, Table<Table<TypeNode>> localNetworkScopesEnvironment, List<string> errors)
    {
        if (defNode is ConstDeclarationNode cdNode)
        {
            // Try binding and error if fail
            if (!globalEnvironment.TryBindIfNotExists(cdNode.Identifier.Identifier, cdNode.Type)) errors.Add("Error: Const already declared.");

            // Check if expression is correct type else add error
            TypeNode exprType = GetTypeOfExpression(cdNode.Expression);
            if (!(cdNode.Type == exprType)) errors.Add("Error: Expression type must match ${cdNode.type}");

            // Check children
            TypeCheckChildren(cdNode, globalEnvironment, localNetworkScopesEnvironment, errors, null);
        }
        else if (defNode is FunctionNode funcNode)
        {
            // Try binding and error if fail
            if (!globalEnvironment.TryBindIfNotExists(funcNode.Identifier.Identifier, funcNode.ReturnType)) errors.Add("Error: Function already declared.");

            // Check children
            TypeCheckChildren(funcNode, globalEnvironment, localNetworkScopesEnvironment, errors, new Table<TypeNode>());

        }
        else if (defNode is NetworkDefinitionNode netNode)
        {
            // Try binding and error if fail
            if (!globalEnvironment.TryBindIfNotExists(netNode.Network.Identifier.Identifier, netNode.Network.CustomType)) errors.Add("Error: Network already declared.");
            // Check children
            TypeCheckChildren(netNode, globalEnvironment, localNetworkScopesEnvironment, errors, new Table<TypeNode>());

        }
        else if (defNode is SimulateNode simNode)
        {
            // Type checking doesn't care about simulate
            // Check children
            TypeCheckChildren(defNode, globalEnvironment, localNetworkScopesEnvironment, errors, null);

        }
    }
}
