using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public class TypeChecker
{
    Table<TypeNode> environment = new();
    Table<Table<TypeNode>> localNetworkScopesEnvironment = new();
    Table<LiteralNode> constEnvironment = new();

    // env for definitions and localEnv for statements? Return localEnv as new so it is not referenced
    public List<string> TypeCheckNode(Node node, List<string> errors)
    {
        // Check the node and create localEnv if neccesary
        if (node is DefinitionNode defNode)
        {
            // We don't need localEnv because these definition can only be global.
            TypeCheckDefinitionNode(defNode, errors);

        }
        else if (node is StatementNode stmtNode)
        {

        }

        // Check children
        TypeCheckChildren(node, errors);

        // Return errors to parent node type check
        return errors;
    }

    private void TypeCheckChildren(Node parentNode, List<string> errors)
    {
        //Type check child nodes
        foreach (Node childNode in parentNode.GetChildren())
        {
            errors.AddRange(TypeCheckNode(childNode, errors));
        }
    }

    private bool CheckExpressionMatchesLiteral(ExpressionNode expressionNode, LiteralNode expectedLiteral) 
    {
        return expressionNode switch {
            // Further expressions
            AddNode node => (CheckExpressionMatchesLiteral(node.Left, expectedLiteral) && CheckExpressionMatchesLiteral(node.Right, expectedLiteral)),
            AndNode node => (CheckExpressionMatchesLiteral(node.Left, expectedLiteral) && CheckExpressionMatchesLiteral(node.Right, expectedLiteral)),
            DivisionNode node => (CheckExpressionMatchesLiteral(node.Left, expectedLiteral) && CheckExpressionMatchesLiteral(node.Right, expectedLiteral)),
            EqualNode node => (CheckExpressionMatchesLiteral(node.Left, expectedLiteral) && CheckExpressionMatchesLiteral(node.Right, expectedLiteral)),
            FunctionCallNode node => CheckNodeMatchesLiteral(node, expectedLiteral),
            IdentifierExpressionNode node => CheckNodeMatchesLiteral(node, expectedLiteral),
            LessThanNode node => (CheckExpressionMatchesLiteral(node.Left, expectedLiteral) && CheckExpressionMatchesLiteral(node.Right, expectedLiteral)),
            MultiplyNode node => (CheckExpressionMatchesLiteral(node.Left, expectedLiteral) && CheckExpressionMatchesLiteral(node.Right, expectedLiteral)),
            NegativeNode node => (CheckExpressionMatchesLiteral(node.Inner, expectedLiteral)),
            NotNode node => (CheckExpressionMatchesLiteral(node.Inner, expectedLiteral)),
            ParenthesesNode node => (CheckExpressionMatchesLiteral(node.Inner, expectedLiteral)),

            // Literals
            ArrayLiteralNode node => (node == expectedLiteral),
            BoolLiteralNode node => (node == expectedLiteral),
            DoubleLiteralNode node => (node == expectedLiteral),
            IntLiteralNode node => (node == expectedLiteral),
            StringLiteralNode node => (node == expectedLiteral),
            LiteralNode node => (node == expectedLiteral),
            _ => throw new("Error: Expression not found")
        };
        throw new NotImplementedException();
    }

    private bool CheckNodeMatchesLiteral(Node node, LiteralNode expectedLiteral) 
    {
        if (node is FunctionCallNode funcNode) {
            environment.Lookup(funcNode.Identifier.Identifier, out TypeNode? returnType);

            if (returnType is null) return false;
            
            return returnType.GetType() == expectedLiteral.GetType();
        }
        else if (node is IdentifierExpressionNode idNode) {
            if (idNode.Identifier is SingleIdentifierNode singleIdNode) {
                environment.Lookup(singleIdNode.Identifier, out TypeNode? type);

                if (type is null) return false;
                
                return type.GetType() == expectedLiteral.GetType();
            }
            else {
                throw new NotImplementedException("We only look at single identifiers currently.");
            }
        }
        return false;
    }

    private void TypeCheckDefinitionNode(Node defNode, List<string> errors)
    {
        if (defNode is ConstDeclarationNode cdNode)
        {
            // x not in domain E
            // e : T
            // E[x -> T]
            
            // 

            // Check if expression is correct type else add error
            if (cdNode.GetType() != cdNode.Expression.GetType()) errors.Add("Error: Expression type must match declaration type.");

            // Try binding and error if fail
            if (!environment.TryBindIfNotExists(cdNode.Identifier.Identifier, cdNode.Type)) errors.Add("Error: Const already declared.");
            

        }
        else if (defNode is FunctionNode funcNode)
        {
            // Try binding and error if fail
            if (!environment.TryBindIfNotExists(funcNode.Identifier.Identifier, funcNode.ReturnType)) errors.Add("Error: Function already declared.");

            // Check children
            TypeCheckChildren(funcNode, errors);

        }
        else if (defNode is NetworkDefinitionNode netNode)
        {
            // Try binding and error if fail
            if (!environment.TryBindIfNotExists(netNode.Network.Identifier.Identifier, netNode.Network.CustomType)) errors.Add("Error: Network already declared.");
            // Check children
            TypeCheckChildren(netNode, errors);

        }
        else if (defNode is SimulateNode simNode)
        {
            // Type checking doesn't care about simulate
            // Check children
            TypeCheckChildren(defNode, errors);

        }
    }
}
