using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public class TypeChecker
{
    // env for definitions and localEnv for statements? Return localEnv as new so it is not referenced
    public List<string> TypeCheckNode(Node node, Environment env, Environment? localEnv = null)
    {
        List<string> errors = [];

        // Check the node and create localEnv if neccesary
        if (node is DefinitionNode defNode)
        {
            TypeCheckDefinitionNode(env, errors, defNode);
        }
        else if (node is StatementNode stmtNode)
        {

        }


        // Return errors to parent node type check
        return errors;
    }

    private void TypeCheckChildren(Node parentNode, Environment env, Environment? localEnv, List<string> errors)
    {
        //Type check child nodes
        foreach (Node childNode in parentNode.GetChildren())
        {
            if (localEnv is null)
            {
                //localEnv being null means that we are in global scope
                errors.AddRange(TypeCheckNode(childNode, env, null));
            }
            else
            {
                errors.AddRange(TypeCheckNode(childNode, env, localEnv));
            }

        }
    }

    private void TypeCheckDefinitionNode(Environment env, List<string> errors, DefinitionNode defNode)
    {
        if (defNode is ConstDeclarationNode cdNode)
        {
            // Try binding and error if fail
            if (!env.TryBind(cdNode.Identifier.Identifier, cdNode.Type)) errors.Add("Error: Const already declared.");
            //Check further
            TypeCheckChildren(cdNode, env, null, errors);
        }
        else if (defNode is FunctionNode funcNode)
        {
            // Try binding and error if fail
            if (!env.TryBind(funcNode.Identifier.Identifier, funcNode)) errors.Add("Error: Const already declared.");

        }
        else if (defNode is NetworkDefinitionNode netNode)
        {
            // Try binding and error if fail
            if (!env.TryBind(netNode.Identifier.Identifier, cdNode.Type)) errors.Add("Error: Const already declared.");

        }
        else if (defNode is SimulateNode simNode)
        {
            // Try binding and error if fail
            if (!env.TryBind(simNode.Identifier.Identifier, cdNode.Type)) errors.Add("Error: Const already declared.");

        }
    }
}
