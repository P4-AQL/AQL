using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Metrics;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Routes;
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
        if (node is ProgramNode programNode)
        {
            TypeCheckProgramNode(programNode, errors);
        }   
        else errors.Add("Unexpected definition");

        // Return errors to parent node type check
        return errors;
    }

    private void TypeCheckProgramNode(ProgramNode programNode, List<String> errors)
    {
        if (programNode is ImportNode)
        {
            // The interpreter fixes this
        }
        else if (programNode is DefinitionProgramNode definitionProgramNode)
        {
            TypeCheckDefinitionNode(definitionProgramNode.Definition, errors);
        }
        else {
            errors.Add("Unexpected definition");
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
            if (FindExpressionType(cdNode.Expression, errors) != cdNode.Type) errors.Add("Error: Expression type must match declaration type.");

            // Try binding and error if fail
            if (!constEnvironment.TryBindIfNotExists(cdNode.Identifier.Identifier, cdNode.Type)) errors.Add("Error: Identifier already declared.");

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
            if (environment.TryBindIfNotExists(funcNode.Identifier.Identifier, funcNode))
            {
                // E1
                Table<Node> newEnvironment = environment;

                // bind function parameters
                foreach (TypeAndIdentifier parameter in funcNode.Parameters)
                {
                    if (!newEnvironment.TryBindIfNotExists(parameter.Identifier.Identifier, parameter.Type)) errors.Add("Error: Parameter identifier already exist");
                }

                /// type check children

                // pass E1 to S
                TypeCheckStatementNode(funcNode.Body, errors, funcNode.ReturnType, newEnvironment);
                // pass E to D
                if (funcNode.NextDefinition is not null)
                {
                    TypeCheckDefinitionNode(funcNode.NextDefinition, errors);
                }

            }
            else
            {
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
            if (!localNetworkScopesEnvironment.Lookup(GetIdentifier(simNode.NetworkIdentifier)[0], out Table<Node>? _)) errors.Add("Network identifier not found");
            

            // Expression is int
            if (FindExpressionType(simNode.Runs, errors) is not IntTypeNode) errors.Add("Expression for runs must be of type int");
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
            /// Bind Sigma to Gamma and continue with next definition

            // x not in dom(E)
            if (environment.Lookup(GetIdentifier(networkDeclarationNode.Identifier)[0], out Node? _)) errors.Add("Identifier already declared");

            // Make Sigma
            Table<Node> localNetwork = new();

            // input
            TypeCheckInputs(networkDeclarationNode.Inputs, localNetwork, errors);

            // output
            TypeCheckOutputs(networkDeclarationNode.Inputs, localNetwork, errors);

            // instances
            TypeCheckInstances(networkDeclarationNode.Instances, localNetwork, errors);

            // routes
            TypeCheckRoutes(networkDeclarationNode.Routes, localNetwork, errors);

            // metrics
            TypeCheckNetworkMetricList(networkDeclarationNode.Metrics, errors);

            // bind to network env
            localNetworkScopesEnvironment.TryBindIfNotExists(GetIdentifier(networkDeclarationNode.Identifier)[0], localNetwork);

            /// isvalid? Check that there is routing from all inputs, routing from all instances and routing to all outputs
            // routing from all inputs
            foreach (SingleIdentifierNode input in networkDeclarationNode.Inputs)
            {
                string id = GetIdentifier(input)[0];
                bool inputFound = false;

                foreach (RouteDefinitionNode route in networkDeclarationNode.Routes.Cast<RouteDefinitionNode>())
                {
                    if (route.From is IdentifierExpressionNode idExprNode) {
                        if (GetIdentifier(idExprNode.Identifier)[0] == id) inputFound = true;
                    }
                }
                
                if (!inputFound) errors.Add("Input not used");
            }

            // routing from all instances
            foreach (InstanceDeclaration instance in networkDeclarationNode.Instances)
            {
                string id = GetIdentifier(instance.ExistingInstance)[0];
                bool instanceFound = false;

                foreach (RouteDefinitionNode route in networkDeclarationNode.Routes.Cast<RouteDefinitionNode>())
                {
                    if (route.From is IdentifierExpressionNode idExprNode) {
                        if (GetIdentifier(idExprNode.Identifier)[0] == id) instanceFound = true;
                    }
                }
                
                if (!instanceFound) errors.Add("Instance not used");
            }

            // routing to all outputs
            foreach (SingleIdentifierNode output in networkDeclarationNode.Outputs)
            {
                string id = GetIdentifier(output)[0];
                bool outputFound = false;

                foreach (RouteDefinitionNode route in networkDeclarationNode.Routes.Cast<RouteDefinitionNode>())
                {
                    if (route.From is IdentifierExpressionNode idExprNode) {
                        if (GetIdentifier(idExprNode.Identifier)[0] == id) outputFound = true;
                    }
                }
                
                if (!outputFound) errors.Add("Instance not used");
            }
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
            if (environment.Lookup(GetIdentifier(queueDeclarationNode.Identifier)[0], out Node? _)) errors.Add("Identifier already declared");

            // service expression...
            if (FindExpressionType(queueDeclarationNode.Service, errors) is not IntTypeNode && FindExpressionType(queueDeclarationNode.Service, errors) is not DoubleTypeNode) errors.Add("Service expression must be int or double");

            // capacity expression...
            if (FindExpressionType(queueDeclarationNode.Capacity, errors) is not IntTypeNode) errors.Add("Capacity expression must be int");

            // servers expression...
            if (FindExpressionType(queueDeclarationNode.NumberOfServers, errors) is not IntTypeNode) errors.Add("Number of servers expression must be int");

            // capacity expression...
            TypeCheckQueueMetricList(queueDeclarationNode.Metrics, errors);

            // bind x to queue
            environment.TryBindIfNotExists(GetIdentifier(queueDeclarationNode.Identifier)[0], queueDeclarationNode.CustomType);
            
            // next definition is being checked in the end of this method
        }

        // check children
        if (networkDefinitionNode.NextDefinition is not null) TypeCheckDefinitionNode(networkDefinitionNode.NextDefinition, errors);
    }

    private void TypeCheckStatementNode(StatementNode statementNode, List<string> errors, TypeNode returnType, Table<Node>? localEnvironment)
    {
        if (localEnvironment is not null)
        {
            environment = localEnvironment;
        }
        
        if (statementNode is AssignNode assignNode){
            // E ⊢ x = e : ok   if 
            // E(x) = T 
            // E ⊢ e : T 
            // T is not const- int, doub, or bool
            if (environment.Lookup(assignNode.Identifier.Identifier, out Node? nodeType) == false)
            {
                throw new("This identifier is not found");
            }
            else if (FindExpressionType(assignNode.Expression, errors) != nodeType)
            {
                errors.Add("The expression type does not match the idetifier");
            }

            if (constEnvironment.Lookup(assignNode.Identifier.Identifier, out Node? _))
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
            if (FindExpressionType(ifElseNode.Condition, errors) is not BoolTypeNode)
            {
                errors.Add("The expression type is not bool");
            }

            TypeCheckStatementNode(ifElseNode.IfBody, errors, returnType, localEnvironment);
            TypeCheckStatementNode(ifElseNode.ElseBody, errors, returnType, localEnvironment);
        }

        else if (statementNode is ReturnNode returnNode)
        {
            Node? expressionNode = FindExpressionType(returnNode.Expression, errors);

            if (expressionNode is not TypeNode expressionTypeNode || expressionTypeNode.GetType() != returnType.GetType())
            {
                errors.Add($"Not a valid type on line {statementNode.LineNumber}");
            }

        }
        else if (statementNode is SkipNode skipNode)
        {
            // no operations for skip node
        }
        else if (statementNode is StatementCompositionNode statementCompositionNode)
        {
            // S_1 : OK 
            // S_2 : OK 
            StatementNode? current = statementCompositionNode;

            while (current is not null)
            {
                TypeCheckStatementNode(current, errors, returnType, localEnvironment);

                if (current is StatementCompositionNode stmCompositionNode)
                {
                    current = stmCompositionNode.NextStatement;
                }
                else
                {
                    break;
                }
            }
        }
        else if (statementNode is VariableDeclarationNode variableDeclarationNode)
        {
            // E ⊢ Tx = e; S 
            // x not in dom(E)      
            // E ⊢ e: T
            // E [x ⊢> T] ⊢ S : ok 
            environment.Lookup(variableDeclarationNode.Identifier.Identifier, out Node? nodeType);

            if (!environment.TryBindIfNotExists(variableDeclarationNode.Identifier.Identifier, variableDeclarationNode.Type))
            {
                errors.Add("Error: Identifier already exist");
            }

            if (FindExpressionType(variableDeclarationNode.Expression, errors) != nodeType)
            {
                errors.Add("The expression type does not match that of the idetifier");
            }

            TypeCheckStatementNode(statementNode, errors, returnType, localEnvironment);
        }

        else if (statementNode is WhileNode whileNode)
        {
            // while e do S : ok 
            // E ⊢ e : bool
            // E ⊢ S : ok 
            if (FindExpressionType(whileNode.Condition, errors) is not BoolTypeNode)
            {
                errors.Add("exression is not of bool type");
            }

            TypeCheckStatementNode(whileNode.Body, errors, returnType, localEnvironment);
        }


    }



    private Node? FindExpressionType(ExpressionNode topExpression, List<string> errors)
    {
        try
        {
            return FindExpressionTypeInner(topExpression);
        }
        catch (Exception e)
        {
            errors.Add(e.Message);
            return null;
        }

        Node FindExpressionTypeInner(ExpressionNode expressionNode)
        {
            return expressionNode switch
            {
                // Further expressions. Clearly not optimized
                AddNode node => (FindExpressionTypeInner(node.Left) == FindExpressionTypeInner(node.Right)
                    ? ((FindExpressionTypeInner(node.Left) is DoubleTypeNode || FindExpressionTypeInner(node.Right) is DoubleTypeNode)
                        ? new DoubleTypeNode(node.LineNumber)
                        : ((FindExpressionTypeInner(node.Left) is IntTypeNode || FindExpressionTypeInner(node.Right) is IntTypeNode)
                            ? new IntTypeNode(node.LineNumber)
                            : throw new("Expression must be int or double")))
                    : throw new("Error: Expression not found")),
                AndNode node => (FindExpressionTypeInner(node.Left) is BoolTypeNode) && (FindExpressionTypeInner(node.Right) is BoolTypeNode) ? new BoolTypeNode(node.LineNumber) : throw new("Expressions must evaluate to bool"),
                DivisionNode node => (FindExpressionTypeInner(node.Left) == FindExpressionTypeInner(node.Right)
                    ? ((FindExpressionTypeInner(node.Left) is DoubleTypeNode || FindExpressionTypeInner(node.Right) is DoubleTypeNode)
                        ? new DoubleTypeNode(node.LineNumber)
                        : ((FindExpressionTypeInner(node.Left) is IntTypeNode || FindExpressionTypeInner(node.Right) is IntTypeNode)
                            ? new IntTypeNode(node.LineNumber)
                            : throw new("Expression must be int or double")))
                    : throw new("Error: Expression not found")),
                EqualNode node => FindExpressionTypeInner(node.Left) == FindExpressionTypeInner(node.Right) ? new BoolTypeNode(node.LineNumber) : new BoolTypeNode(node.LineNumber),
                FunctionCallNode node => GetReturnTypeOfFunctionCall(node),
                IdentifierExpressionNode node => environment.Lookup(GetIdentifier(node.Identifier)[0], out Node? typeNode) 
                    ? typeNode 
                    : (constEnvironment.Lookup(GetIdentifier(node.Identifier)[0], out Node? constTypeNode) 
                        ? constTypeNode 
                        : throw new("Error: Expression not found")),
                LessThanNode node => FindExpressionTypeInner(node.Left) == FindExpressionTypeInner(node.Right) ? new BoolTypeNode(node.LineNumber) : new BoolTypeNode(node.LineNumber),
                MultiplyNode node => (FindExpressionTypeInner(node.Left) == FindExpressionTypeInner(node.Right)
                    ? ((FindExpressionTypeInner(node.Left) is DoubleTypeNode || FindExpressionTypeInner(node.Right) is DoubleTypeNode)
                        ? new DoubleTypeNode(node.LineNumber)
                        : ((FindExpressionTypeInner(node.Left) is IntTypeNode || FindExpressionTypeInner(node.Right) is IntTypeNode)
                            ? new IntTypeNode(node.LineNumber)
                            : throw new("Expression must be int or double")))
                    : throw new("Error: Expression not found")),
                NegativeNode node => (FindExpressionTypeInner(node.Inner) is IntTypeNode || FindExpressionTypeInner(node.Inner) is DoubleTypeNode) ? node.Inner : throw new("Expression not int or double"),
                NotNode node => (FindExpressionTypeInner(node.Inner) is BoolTypeNode ? node.Inner : throw new("Expression must evaluate to bool")),
                ParenthesesNode node => (FindExpressionTypeInner(node.Inner)),

                // Literals
                ArrayLiteralNode node => new ArrayTypeNode(node.LineNumber, (TypeNode)FindExpressionTypeInner(node.Elements[0])), //Make sure each element is same type
                BoolLiteralNode node => new BoolTypeNode(node.LineNumber),
                DoubleLiteralNode node => new DoubleTypeNode(node.LineNumber),
                IntLiteralNode node => new IntTypeNode(node.LineNumber),
                LiteralNode node => new StringTypeNode(node.LineNumber),
                _ => throw new("Error: Expression not found")
            };
            throw new NotImplementedException();
        }
    }

    private TypeNode GetReturnTypeOfFunctionCall(FunctionCallNode node) 
    {
        environment.Lookup(GetIdentifier(node.Identifier)[0], out Node? functionBody);
        if (functionBody is FunctionNode funcNode) {
            return funcNode.ReturnType;
        }
        else
        {
            throw new("Error: Unexpected node saved at identifier.");
        }
    }

    private string[] GetIdentifier(IdentifierNode idNode) 
    {
        if (idNode is SingleIdentifierNode node) {
            return new string[]{node.Identifier};
        }
        else if (idNode is QualifiedIdentifierNode qualifiedIdentifierNode)
        {
            return new string[]{qualifiedIdentifierNode.LeftIdentifier.Identifier, qualifiedIdentifierNode.RightIdentifier.Identifier};
        }
        else return [];
    }

    private bool CheckNodeMatchesLiteral(Node node, LiteralNode expectedLiteral)
    {
        if (node is FunctionCallNode funcNode)
        {
            environment.Lookup(GetIdentifier(funcNode.Identifier)[0], out Node? returnType);

            if (returnType is null) return false;

            return returnType.GetType() == expectedLiteral.GetType();
        }
        else if (node is IdentifierExpressionNode idNode)
        {
            if (idNode.Identifier is SingleIdentifierNode singleIdNode)
            {
                environment.Lookup(singleIdNode.Identifier, out Node? type);

                if (type is null) return false;

                return type.GetType() == expectedLiteral.GetType();
            }
            else
            {
                throw new NotImplementedException("We only look at single identifiers currently.");
            }
        }
        return false;
    }

    //These 2 methods could probably be combined, but it is not important
    private void TypeCheckInputs(IReadOnlyList<SingleIdentifierNode> io, Table<Node> localNetworkTable, List<string> errors)
    {
        foreach (SingleIdentifierNode node in io)
        {
            // not in dom(E) or dom(Sigma)
            if (environment.Lookup(node.Identifier, out Node? _) || localNetworkTable.Lookup(node.Identifier, out Node? _)) errors.Add("identifier already declared");

            // i != j => x_i != x_j
            localNetworkTable.TryBindIfNotExists(node.Identifier, new InputTypeNode(node.LineNumber));
            // should be checked above, since we do it one identifier at a time, it and makes sure it is not defined before
        }
    }

    private void TypeCheckOutputs(IReadOnlyList<SingleIdentifierNode> io, Table<Node> localNetworkTable, List<string> errors)
    {
        foreach (SingleIdentifierNode node in io)
        {
            // not in dom(E) or dom(Sigma)
            if (environment.Lookup(node.Identifier, out Node? _) || localNetworkTable.Lookup(node.Identifier, out Node? _)) errors.Add("identifier already declared");

            // i != j => x_i != x_j
            localNetworkTable.TryBindIfNotExists(node.Identifier, new OutputTypeNode(node.LineNumber)); //Should always work since we alread looked it up

        }
    }

    readonly static string[] queueMetricNames = [
        "mrt",
        "vrt",
        "awt",
        "expected_num_entities",
        "util",
        "throughput"
    ];

    private static void TypeCheckQueueMetricList(IReadOnlyList<NamedMetricNode> metricList, List<string> errors)
    {
        foreach (NamedMetricNode metric in metricList)
        {
            if (queueMetricNames.Contains(metric.Name)) errors.Add("metric list must only contain metrics");
        }
    }

    readonly static string[] networkMetricNames = [
        "avg",
        "throughput",
        "expected_num_entities"
    ];

    private static void TypeCheckNetworkMetricList(IReadOnlyList<NamedMetricNode> metricList, List<string> errors)
    {
        foreach (NamedMetricNode metric in metricList)
        {
            if (networkMetricNames.Contains(metric.Name)) errors.Add("metric list must only contain metrics");
        }
    }

    private void TypeCheckInstances(IReadOnlyList<InstanceDeclaration> instances, Table<Node> localNetwork, List<string> errors)
    {
        foreach (InstanceDeclaration instance in instances)
        {
            // check existing
            if (environment.Lookup(GetIdentifier(instance.ExistingInstance)[0], out Node? _) || localNetworkScopesEnvironment.Lookup(GetIdentifier(instance.ExistingInstance)[0], out Table<Node>? _)) {
                // bind new to same as existing
                localNetwork.TryBindIfNotExists(GetIdentifier(instance.NewInstance)[0], instance.ExistingInstance);
            }
            else errors.Add("Error: Instance identifier not found");
        }
    }

    private void TypeCheckRoutes(IReadOnlyList<RouteNode> routes, Table<Node> localNetwork, List<string> errors)
    {
        foreach (RouteNode route in routes)
        {
            if (route is RouteDefinitionNode routeDefinitionNode)
            {
                // Check destination
                TypeCheckRouteDestination(routeDefinitionNode.To, localNetwork, errors);

                // If it is identifier node, then it is case 2 or 3
                if (routeDefinitionNode.From is IdentifierExpressionNode identifierExpressionNode) {
                    // case 3
                    if (localNetwork.Lookup(GetIdentifier(identifierExpressionNode.Identifier)[0], out Node? type)) {
                        if (type is not QueueDeclarationNode and not InputTypeNode) errors.Add("Input is not a valid queue or input");
                    }
                    // case 2
                    else if (localNetworkScopesEnvironment.Lookup(GetIdentifier(identifierExpressionNode.Identifier)[0], out Table<Node>? network)) {
                        network.Lookup(GetIdentifier(identifierExpressionNode.Identifier)[1], out Node? outputNode);
                        if (outputNode is not OutputTypeNode) errors.Add("Second identifier must be an output node");
                    }
                    else errors.Add("Identifier not found");
                }
                // Case 1
                else {
                    Node? type = FindExpressionType(routeDefinitionNode.From, errors);
                    // not case 1
                    if (type is not IntTypeNode && type is not DoubleTypeNode) errors.Add("Expression vlaue must be of type int or double");
                }
            }
            else
            {
                errors.Add("Route is not valid");
            }
        }
    }

    private void TypeCheckRouteDestination(IReadOnlyList<RouteValuePairNode> destinations, Table<Node> localNetwork, List<string> errors)
    {
        foreach (RouteValuePairNode destination in destinations)
        {
            Node? type = FindExpressionType(destination.Probability, errors);
            
            if (type is not IntTypeNode && type is not DoubleTypeNode) errors.Add("Route destination weight must be double or int");
            
            if (localNetworkScopesEnvironment.Lookup(GetIdentifier(destination.RouteTo)[0], out Table<Node>? net)) {
                if (net.Lookup(GetIdentifier(destination.RouteTo)[1], out Node? inputNode)) {
                    if (inputNode is not InputTypeNode) errors.Add("Route to network identifier must be an input");
                }
                else errors.Add("Network identifier not found");
            }
            else if (localNetwork.Lookup(GetIdentifier(destination.RouteTo)[0], out Node? caseTwoType)) {
                if (caseTwoType is not QueueDeclarationNode && caseTwoType is not OutputTypeNode) errors.Add("Must point to queue or output");
            }
            else {
                errors.Add("Instance not found");
            }
        }
    }
}