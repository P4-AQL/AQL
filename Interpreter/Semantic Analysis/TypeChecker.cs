using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Statements;
using Interpreter.AST.Nodes.Types;

namespace Interpreter.SemanticAnalysis;
public class TypeChecker
{
    // E
    Table<Node> environment = new();
    Table<Node> constEnvironment = new();

    // Gamma
    Table<Table<Node>> localNetworkScopesEnvironment = new();

    // env for definitions and localEnv for statements? Return localEnv as new so it is not referenced
    public List<string> TypeCheckNode(Node node, List<string> errors)
    {

        if (node is ProgramNode programNode){
            TypeCheckProgramNode(programNode, errors);
        }
        else if (node is DefinitionNode defNode)
        {
            // We don't need localEnv because these definition can only be global.
            TypeCheckDefinitionNode(defNode, errors);

        }
        else if (node is StatementNode statementNode)
        {
            TypeCheckStatementNode(statementNode, errors, null);
        }

        // Return errors to parent node type check
        return errors;
    }

    private void TypeCheckProgramNode(Node programNode, List<String> errors)
    {
        if (programNode is ImportNode)
        {

        }
        else if (programNode is DefinitionProgramNode)
        {

        }
    }

    private void TypeCheckDefinitionNode(DefinitionNode defNode, List<string> errors)
    {
        if (defNode is ConstDeclarationNode cdNode)
        {
            // x not in domain E
            // e : T
            // E[x -> T]

            // Check if expression is correct type else add error
            if (FindExpressionType(cdNode.Expression) != cdNode.Type) errors.Add("Error: Expression type must match declaration type.");

            // Try binding and error if fail
            if (!environment.TryBindIfNotExists(cdNode.Identifier.Identifier, cdNode.Type)) errors.Add("Error: Identifier already declared.");

            /// Type check children
            if (cdNode.NextDefinition is not null) TypeCheckDefinitionNode(cdNode.NextDefinition, errors);
            TypeCheckNode(cdNode.Expression, errors);

        }
        else if (defNode is FunctionNode funcNode)
        {
            // x not in dom(E)
            // bind function identifier to E1
            // bind function parameters to new E and pass them to S
            // pass new E to next definition (also happens later automatically)
            
            // bind function parameters to E1 and pass them to S
            if (environment.TryBindIfNotExists(funcNode.Identifier.Identifier, funcNode)) {
                // E1
                Table<Node> newEnvironment = environment;

                // bind function parameters
                foreach (TypeAndIdentifier parameter in funcNode.Parameters)
                {
                    if (!newEnvironment.TryBindIfNotExists(parameter.Identifier.Identifier, parameter.Type)) errors.Add("Error: Parameter identifier already exist");
                }

                /// type check children

                // pass E1 to S
                TypeCheckStatementNode(funcNode.Body, errors, newEnvironment);
                // pass E to D
                if (funcNode.NextDefinition is not null) {
                    TypeCheckDefinitionNode(funcNode.NextDefinition, errors);
                }
                
            }
            else {
                errors.Add("Error: Identifier already declared.");
            }
            
        }
        else if (defNode is NetworkDefinitionNode netNode)
        {
            // Try binding and error if fail
            if (!environment.TryBindIfNotExists(netNode.Network.Identifier.Identifier, netNode.Network.CustomType)) errors.Add("Error: Network already declared.");

        }
        else if (defNode is SimulateNode simNode)
        {
            // x in domain Gamma
            if (!localNetworkScopesEnvironment.Lookup(GetIdentifier(simNode.NetworkIdentifier), out Table<Node>? _)) errors.Add("Network identifier not found");
            

            // Expression is int
            if (FindExpressionType(simNode.Runs) is not IntTypeNode) errors.Add("Expression for runs must be of type int");
        }
    }

    private void TypeCheckStatementNode(StatementNode statementNode, List<String> errors, Table<Node>? localEnvironment){
        if (localEnvironment is not null) {
            environment = localEnvironment;
        }

        if (statementNode is AssignNode assignNode){
            // E ⊢ x = e : ok   if 
                // E(x) = T 
                // E ⊢ e : T 
                // T is not const- int, doub, or bool
            environment.Lookup(assignNode.Identifier.Identifier, out Node? nodeType);
            
            if (nodeType is null)
            {
                errors.Add("This identifier is not found");
            }
            else if (FindExpressionType(assignNode.Expression) != nodeType)
            {
                errors.Add("The expression type does not match the idetifier");
            }
            
            if(constEnvironment.Lookup(assignNode.Identifier.Identifier, out Node? _))
            {
                errors.Add("This idetifier is a const");
            }

        }
        else if (statementNode is IfElseNode ifElseNode)
        {
            // E ⊢ if e then S_1 else S_2 : ok 
                // E ⊢ e : bool 
                // E ⊢ S_1 : ok 
                // E ⊢ S_2 : ok
            

        }
        else if (statementNode is ReturnNode returnNode)
        {

        }
        else if (statementNode is SkipNode skipNode)
        {

        }
        else if (statementNode is StatementCompositionNode statementCompositionNode)
        {

        }
        else if (statementNode is VariableDeclarationNode variableDeclarationNode){
            // E ⊢ Tx = e; S 
                // x not in dom(E)
                // E ⊢ e: T
                // E [x ⊢> T] ⊢ S : ok 

        }
        else if (statementNode is WhileNode whileNode)
        {

        }
    }







    private Node FindExpressionType(ExpressionNode expressionNode) 
    {
        return expressionNode switch {
            // Further expressions. Clearly not optimized
            AddNode node => (FindExpressionType(node.Left) == FindExpressionType(node.Right) ? FindExpressionType(node.Left) : throw new("Error: Expression not found")),
            AndNode node => (FindExpressionType(node.Left) == FindExpressionType(node.Right) ? FindExpressionType(node.Left) : throw new("Error: Expression not found")),
            DivisionNode node => (FindExpressionType(node.Left) == FindExpressionType(node.Right) ? FindExpressionType(node.Left) : throw new("Error: Expression not found")),
            EqualNode node => (FindExpressionType(node.Left) == FindExpressionType(node.Right) ? FindExpressionType(node.Left) : throw new("Error: Expression not found")),
            FunctionCallNode node => HandleFunctionCall(node),
            IdentifierExpressionNode node => environment.Lookup(GetIdentifier(node.Identifier), out Node? typeNode) ? typeNode : throw new("Error: Undeclared identifier"),
            LessThanNode node => (FindExpressionType(node.Left) == FindExpressionType(node.Right) ? FindExpressionType(node.Left) : throw new("Error: Expression not found")),
            MultiplyNode node => (FindExpressionType(node.Left) == FindExpressionType(node.Right) ? FindExpressionType(node.Left) : throw new("Error: Expression not found")),
            NegativeNode node => (FindExpressionType(node.Inner)),
            NotNode node => (FindExpressionType(node.Inner)),
            ParenthesesNode node => (FindExpressionType(node.Inner)),

            // Literals
            ArrayLiteralNode node => new ArrayTypeNode(0, (TypeNode)FindExpressionType(node.Elements[0])),
            BoolLiteralNode => new BoolTypeNode(0),
            DoubleLiteralNode => new DoubleTypeNode(0),
            IntLiteralNode => new IntTypeNode(0),
            StringLiteralNode => new StringTypeNode(0),
            LiteralNode => new StringTypeNode(0),
            _ => throw new("Error: Expression not found")
        };
        throw new NotImplementedException();
    }

    private Node HandleFunctionCall(FunctionCallNode node) 
    {
        environment.Lookup(GetIdentifier(node.Identifier), out Node? functionBody);
        if (functionBody is FunctionNode funcNode) {
            return funcNode.ReturnType;
        }
        else {
            throw new("Error: Unexpected node saved at identifier.");
        }
    }

    private string GetIdentifier(IdentifierNode idNode) 
    {
        if (idNode is SingleIdentifierNode node) {
            return node.Identifier;
        }
        else {
            throw new NotImplementedException();
        }
    }

    private bool CheckNodeMatchesLiteral(Node node, LiteralNode expectedLiteral) 
    {
        if (node is FunctionCallNode funcNode) {
            environment.Lookup(funcNode.Identifier.Identifier, out Node? returnType);

            if (returnType is null) return false;
            
            return returnType.GetType() == expectedLiteral.GetType();
        }
        else if (node is IdentifierExpressionNode idNode) {
            if (idNode.Identifier is SingleIdentifierNode singleIdNode) {
                environment.Lookup(singleIdNode.Identifier, out Node? type);

                if (type is null) return false;
                
                return type.GetType() == expectedLiteral.GetType();
            }
            else {
                throw new NotImplementedException("We only look at single identifiers currently.");
            }
        }
        return false;
    }
}
