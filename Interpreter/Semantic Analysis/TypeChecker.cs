using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Networks;
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
            
            // bind function identifier to E
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
            TypeCheckNetwork(netNode, errors);
        }
        else if (defNode is SimulateNode simNode)
        {
            // x in domain Gamma
            if (!localNetworkScopesEnvironment.Lookup(GetIdentifier(simNode.NetworkIdentifier), out Table<Node>? _)) errors.Add("Network identifier not found");
            

            // Expression is int
            if (FindExpressionType(simNode.Runs) is not IntTypeNode) errors.Add("Expression for runs must be of type int");
        }
    }

    private void TypeCheckNetwork(NetworkDefinitionNode networkDefinitionNode, List<string> errors) 
    {
        if (networkDefinitionNode.Network is NetworkDeclarationNode networkDeclarationNode) 
        {
            /// x not in dom(E)
            /// Type check input identifiers
            /// Type check output identifiers
            /// Type check instances
            /// Type check routes
            /// Type check metrics
            /// Save in network Environment and 
            /// IsValid must be true?
            /// Type check remaining children
        }
        else if (networkDefinitionNode.Network is QueueDeclarationNode queueDeclarationNode)
        {
            /// x not in dom(E)
            /// service expression must give a value? This must mean that it have to either be int or double
            /// capacity expression must be integer value
            /// number of servers expression must be integer value
            /// metrics must be metric array type
            /// next definition should be type checked with identifier binded to the queue in E

            // x not in dom(E)
            if (environment.Lookup(GetIdentifier(queueDeclarationNode.Identifier), out Node? _)) errors.Add("Identifier already declared");

            // service expression...
            if (FindExpressionType(queueDeclarationNode.Service) is not IntTypeNode && FindExpressionType(queueDeclarationNode.Service) is not DoubleTypeNode) errors.Add("Service expression must be int or double");

            // capacity expression...
            if (FindExpressionType(queueDeclarationNode.Capacity) is not IntTypeNode) errors.Add("Capacity expression must be int");

            // servers expression...
            if (FindExpressionType(queueDeclarationNode.NumberOfServers) is not IntTypeNode) errors.Add("Number of servers expression must be int");

            // capacity expression...
            if (queueDeclarationNode.Metrics is not List<MetricNode>) errors.Add("Queue metrics must a list of metrics");
            //TypeCheckMetricList(queueDeclarationNode.Metrics, errors);

        }

        if (networkDefinitionNode.NextDefinition is not null) TypeCheckDefinitionNode(networkDefinitionNode.NextDefinition, errors);
    }

    

    private void TypeCheckStatementNode(StatementNode statementNode, List<String> errors, Table<Node>? localEnvironment)
    {
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
            AddNode node => (FindExpressionType(node.Left) == FindExpressionType(node.Right) 
                ? ((FindExpressionType(node.Left) is DoubleTypeNode || FindExpressionType(node.Right) is DoubleTypeNode)
                    ? new DoubleTypeNode(0)
                    : ((FindExpressionType(node.Left) is IntTypeNode || FindExpressionType(node.Right) is IntTypeNode)
                        ? new IntTypeNode(0)
                        : throw new("Expression must be int or double")))
                : throw new("Error: Expression not found")),
            AndNode node => (FindExpressionType(node.Left) is BoolTypeNode) && (FindExpressionType(node.Right) is BoolTypeNode) ? new BoolTypeNode(0) : throw new("Expressions must evaluate to bool"),
            DivisionNode node => (FindExpressionType(node.Left) == FindExpressionType(node.Right) 
                ? ((FindExpressionType(node.Left) is DoubleTypeNode || FindExpressionType(node.Right) is DoubleTypeNode)
                    ? new DoubleTypeNode(0)
                    : ((FindExpressionType(node.Left) is IntTypeNode || FindExpressionType(node.Right) is IntTypeNode)
                        ? new IntTypeNode(0)
                        : throw new("Expression must be int or double")))
                : throw new("Error: Expression not found")),
            EqualNode node => FindExpressionType(node.Left) == FindExpressionType(node.Right) ? new BoolTypeNode(0) : new BoolTypeNode(0),
            FunctionCallNode node => GetReturnTypeOfFunctionCall(node),
            IdentifierExpressionNode node => environment.Lookup(GetIdentifier(node.Identifier), out Node? typeNode) 
                ? typeNode 
                : (constEnvironment.Lookup(GetIdentifier(node.Identifier), out Node? constTypeNode) 
                    ? constTypeNode 
                    : throw new("Error: Expression not found")),
            LessThanNode node => FindExpressionType(node.Left) == FindExpressionType(node.Right) ? new BoolTypeNode(0) : new BoolTypeNode(0),
            MultiplyNode node => (FindExpressionType(node.Left) == FindExpressionType(node.Right) 
                ? ((FindExpressionType(node.Left) is DoubleTypeNode || FindExpressionType(node.Right) is DoubleTypeNode)
                    ? new DoubleTypeNode(0)
                    : ((FindExpressionType(node.Left) is IntTypeNode || FindExpressionType(node.Right) is IntTypeNode)
                        ? new IntTypeNode(0)
                        : throw new("Expression must be int or double")))
                : throw new("Error: Expression not found")),
            NegativeNode node => (FindExpressionType(node.Inner) is IntTypeNode || FindExpressionType(node.Inner) is DoubleTypeNode) ? node.Inner : throw new("Expression not int or double"),
            NotNode node => (FindExpressionType(node.Inner) is BoolTypeNode ? node.Inner : throw new("Expression must evaluate to bool")),
            ParenthesesNode node => (FindExpressionType(node.Inner)),

            // Literals
            ArrayLiteralNode node => new ArrayTypeNode(0, (TypeNode)FindExpressionType(node.Elements[0])), //Make sure each element is same type
            BoolLiteralNode => new BoolTypeNode(0),
            DoubleLiteralNode => new DoubleTypeNode(0),
            IntLiteralNode => new IntTypeNode(0),
            LiteralNode => new StringTypeNode(0),
            _ => throw new("Error: Expression not found")
        };
        throw new NotImplementedException();
    }

    private TypeNode GetReturnTypeOfFunctionCall(FunctionCallNode node) 
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
