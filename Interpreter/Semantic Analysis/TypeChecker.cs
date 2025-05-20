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
using Interpreter.Utilities.Modules;

namespace Interpreter.SemanticAnalysis;

public class TypeChecker
{
    TypeCheckerEnvironment globalEnvironment = new();


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

    private void TypeCheckProgramNode(ProgramNode programNode, List<string> errors)
    {
        if (programNode is ImportNode importNode)
        {
            TypeCheckImportNode(errors, importNode);
        }
        else if (programNode is DefinitionProgramNode definitionProgramNode)
        {
            TypeCheckDefinitionNode(definitionProgramNode.Definition, errors);
        }
        else
        {
            errors.Add($"Unexpected definition (Line {programNode.LineNumber})");
        }
    }

    private void TypeCheckImportNode(List<string> errors, ImportNode importNode)
    {
        try
        {
            string library = GetIdentifiers(importNode.Namespace);
            InterpretationEnvironment interpretationEnvironment = ModuleLoader.LoadModuleByName(library);
            //Add all environment variables
            //global scope
            foreach (string key in interpretationEnvironment.typeChecker.environment.Dictionary.Keys)
            {
                interpretationEnvironment.typeChecker.environment.Lookup(key, out Node? node);
                if (node is not null) environment.TryBindIfNotExists(library + "." + key, node);
            }

            //consts
            foreach (string key in interpretationEnvironment.typeChecker.constEnvironment.Dictionary.Keys)
            {
                interpretationEnvironment.typeChecker.constEnvironment.Lookup(key, out Node? node);
                if (node is not null) constEnvironment.TryBindIfNotExists(library + "." + key, node);
            }

            //network scope
            foreach (string key in interpretationEnvironment.typeChecker.localNetworkScopesEnvironment.Dictionary.Keys)
            {
                interpretationEnvironment.typeChecker.localNetworkScopesEnvironment.Lookup(key, out Table<Node>? env);
                if (env is not null) localNetworkScopesEnvironment.TryBindIfNotExists(library + "." + key, env);
            }
        }
        catch (Exception)
        {
            errors.Add($"Import error. (Line {importNode.LineNumber})");
        }

        if (importNode.NextProgram is not null)
            TypeCheckProgramNode(importNode.NextProgram, errors);
    }

    private void TypeCheckDefinitionNode(DefinitionNode defNode, List<string> errors)
    {
        if (defNode is ConstDeclarationNode cdNode)
        {
            // x not in domain E
            // e : T
            // E[x -> T]

            // Check if expression is correct type else add error
            TypeNode? expressionType = FindExpressionType(cdNode.Expression, errors, localEnvironment: null);
            if (expressionType is not null && expressionType.GetType() != cdNode.Type.GetType()) errors.Add($"Error: Expression type must match declaration type. (Line {cdNode.LineNumber})");

            // Try binding and error if fail
            if (!constEnvironment.TryBindIfNotExists(cdNode.Identifier.Identifier, cdNode.Type)) errors.Add($"Error: Identifier already declared. (Line {cdNode.LineNumber})");
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
                Table<Node> newEnvironment = new(environment);

                // bind function parameters
                foreach (TypeAndIdentifier parameter in funcNode.Parameters)
                {
                    if (!newEnvironment.TryBindIfNotExists(parameter.Identifier.Identifier, parameter.Type)) errors.Add($"Error: Parameter identifier already exist (Line {funcNode.LineNumber})");
                }

                /// type check children

                // pass E1 to S
                TypeCheckStatementNode(funcNode.Body, errors, funcNode.ReturnType, newEnvironment);
                // pass E to D

            }
            else
            {
                errors.Add($"Error: Identifier already declared. (Line {funcNode.LineNumber})");
            }

        }
        else if (defNode is NetworkDefinitionNode netNode)
        {
            TypeCheckNetwork(netNode, errors);
        }
        else if (defNode is SimulateNode simNode)
        {
            // x in domain Gamma
            if (!localNetworkScopesEnvironment.Lookup(GetIdentifiers(simNode.NetworkIdentifier), out Table<Node>? _)) errors.Add($"Network identifier not found (Line {simNode.LineNumber})");


            // Expression is int
            if (FindExpressionType(simNode.Runs, errors, localEnvironment: null) is not IntTypeNode) errors.Add($"Expression for runs must be of type int (Line {simNode.LineNumber})");
        }

        // Check next definition
        if (defNode is DefinitionCompositionNode definitionCompositionNode)
        {
            if (definitionCompositionNode.NextDefinition is not null)
            {
                TypeCheckDefinitionNode(definitionCompositionNode.NextDefinition, errors);
            }
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
            string identifiers = GetIdentifiers(networkDeclarationNode.Identifier);
            if (environment.Lookup(identifiers, out Node? _))
                errors.Add($"Identifier already declared (Line {networkDeclarationNode.LineNumber})");

            // Make Sigma
            Table<Node> localNetwork = new(environment);

            // input
            TypeCheckInputs(networkDeclarationNode.Inputs, localNetwork, errors);

            // output
            TypeCheckOutputs(networkDeclarationNode.Outputs, localNetwork, errors);

            // instances
            TypeCheckInstances(networkDeclarationNode.Instances, localNetwork, errors);

            // routes
            TypeCheckRoutes(networkDeclarationNode.Routes, localNetwork, errors);

            // metrics
            TypeCheckNetworkMetricList(networkDeclarationNode.Metrics, errors);

            // bind to network env
            localNetworkScopesEnvironment.TryBindIfNotExists(GetIdentifiers(networkDeclarationNode.Identifier), localNetwork);

            /// isvalid? Check that there is routing from all inputs, routing from all instances and routing to all outputs
            // routing from all inputs
            foreach (SingleIdentifierNode input in networkDeclarationNode.Inputs)
            {
                string id = GetIdentifiers(input);
                bool inputFound = false;

                foreach (RouteDefinitionNode route in networkDeclarationNode.Routes.Cast<RouteDefinitionNode>())
                {
                    if (route.From is IdentifierExpressionNode idExprNode)
                    {
                        if (GetIdentifiers(idExprNode.Identifier) == id) inputFound = true;
                    }
                }

                if (!inputFound) errors.Add($"Input '{id}' not used (Line {input.LineNumber})");
            }

            bool firstInstance = true;
            // routing from all instances
            foreach (InstanceDeclaration instance in networkDeclarationNode.Instances)
            {
                string id = GetIdentifiers(instance.NewInstance);

                // The first instance does not need to be used
                if (firstInstance)
                {
                    firstInstance = false;
                    continue;
                }

                bool instanceFound = false;

                foreach (RouteDefinitionNode route in networkDeclarationNode.Routes.Cast<RouteDefinitionNode>())
                {
                    if (route.From is IdentifierExpressionNode idExprNode)
                    {
                        if (GetIdentifiers(idExprNode.Identifier) == id) instanceFound = true;
                    }
                }

                if (!instanceFound) errors.Add($"instance '{id}' not used (Line {instance.LineNumber})");
            }

            // routing to all outputs
            foreach (SingleIdentifierNode output in networkDeclarationNode.Outputs)
            {
                string id = GetIdentifiers(output);
                bool outputFound = false;

                foreach (RouteDefinitionNode route in networkDeclarationNode.Routes.Cast<RouteDefinitionNode>())
                {
                    if (route.To is IReadOnlyList<RouteValuePairNode> toNodes)
                    {
                        foreach (RouteValuePairNode toNode in toNodes)
                        {
                            if (GetIdentifiers(toNode.RouteTo) == id) outputFound = true;
                        }
                    }
                }

                if (!outputFound) errors.Add($"Output '{id}' not used (Line {output.LineNumber})");
            }
        }
        else if (networkDefinitionNode.Network is QueueDeclarationNode queueDeclarationNode)
        {
            /// x not in dom(E)
            /// service expression must give a value? This must mean that it has to either be int or double
            /// capacity expression must be integer value
            /// number of servers expression must be integer value
            /// metrics must be metric array type
            /// next definition should be type checked with identifier binded to the queue in E

            // x not in dom(E)
            string identifier = GetIdentifiers(queueDeclarationNode.Identifier);
            if (environment.Lookup(identifier, out Node? _))
                errors.Add($"Identifier already declared (Line {queueDeclarationNode.LineNumber})");

            // service expression...
            TypeNode? serviceType = FindExpressionType(queueDeclarationNode.Service, errors, localEnvironment: null);
            if (serviceType is not null && serviceType is not IntTypeNode && serviceType is not DoubleTypeNode)
                errors.Add("Service expression must be int or double");

            // capacity expression...
            TypeNode? capcityType = FindExpressionType(queueDeclarationNode.Capacity, errors, localEnvironment: null);
            if (capcityType is not null && capcityType is not IntTypeNode)
                errors.Add($"Capacity expression must be int (Line {queueDeclarationNode.Capacity.LineNumber})");

            // servers expression...
            TypeNode? serversType = FindExpressionType(queueDeclarationNode.Servers, errors, localEnvironment: null);
            if (serversType is not null && serversType is not IntTypeNode)
                errors.Add($"Number of servers expression must be int (Line {queueDeclarationNode.Servers.LineNumber})");

            // capacity expression...
            TypeCheckQueueMetricList(queueDeclarationNode.Metrics, errors);

            // bind x to queue
            if (environment.TryBindIfNotExists(identifier, queueDeclarationNode.CustomType) == false)
            {
                errors.Add($"Identifier '{identifier}' already exists! (Line {queueDeclarationNode.LineNumber})");
            }

        }

    }

    private void TypeCheckStatementNode(StatementNode statementNode, List<string> errors, TypeNode returnType, Table<Node> localEnvironment)
    {
        if (statementNode is AssignNode assignNode)
        {
            // E ⊢ x = e : ok   if 
            // E(x) = T 
            // E ⊢ e : T 
            // T is not const- int, doub, or bool
            if (localEnvironment.Lookup(assignNode.Identifier.Identifier, out Node? nodeType) == false)
            {
                bool isConst = constEnvironment.Lookup(assignNode.Identifier.Identifier, out Node? _);
                if (isConst)
                    errors.Add($"You cannot assign to a constant! (Line {assignNode.LineNumber})");
                else
                    errors.Add($"Variable not found (Line {assignNode.LineNumber})");
            }
            else if (FindExpressionType(assignNode.Expression, errors, localEnvironment) != nodeType)
            {
                errors.Add($"The expression type does not match the idetifier (Line {assignNode.LineNumber})");
            }
        }

        else if (statementNode is IfElseNode ifElseNode)
        {
            // E ⊢ if e then S_1 else S_2 : ok 
            // E ⊢ e : bool 
            // E ⊢ S_1 : ok 
            // E ⊢ S_2 : ok
            if (FindExpressionType(ifElseNode.Condition, errors, localEnvironment) is not BoolTypeNode)
            {
                errors.Add($"The condition expression must evaluate to type bool! (Line {ifElseNode.LineNumber})");
            }

            TypeCheckStatementNode(ifElseNode.IfBody, errors, returnType, localEnvironment);
            TypeCheckStatementNode(ifElseNode.ElseBody, errors, returnType, localEnvironment);
        }

        else if (statementNode is ReturnNode returnNode)
        {
            Node? expressionNode = FindExpressionType(returnNode.Expression, errors, localEnvironment);

            if (expressionNode is not TypeNode expressionTypeNode || expressionTypeNode.GetType() != returnType.GetType())
            {
                errors.Add($"Type must match the return type of the function! {statementNode.LineNumber}");
            }

        }
        else if (statementNode is SkipNode)
        {
            // no operations for skip node
        }
        else if (statementNode is VariableDeclarationNode variableDeclarationNode)
        {
            // E ⊢ Tx = e; S 
            // x not in dom(E)      
            // E [x ⊢> T] ⊢ S : ok 
            TypeNode variableType = variableDeclarationNode.Type;

            if (localEnvironment.TryBindIfNotExists(variableDeclarationNode.Identifier.Identifier, variableType) == false)
            {
                errors.Add($"Error: Identifier already exist (Line {variableDeclarationNode.LineNumber})");
            }

            // E ⊢ e: T
            TypeNode? expressionType = FindExpressionType(variableDeclarationNode.Expression, errors, localEnvironment);

            if (expressionType is not null && expressionType.GetType() != variableType.GetType())
            {
                errors.Add($"Expression must evaluate to the same type as the declared variable ({variableDeclarationNode.LineNumber})");
            }
        }

        else if (statementNode is WhileNode whileNode)
        {
            // while e do S : ok 
            // E ⊢ e : bool
            // E ⊢ S : ok 
            if (FindExpressionType(whileNode.Condition, errors, localEnvironment) is not BoolTypeNode)
            {
                errors.Add($"While condition expression must evaluate to type bool! (Line {whileNode.LineNumber})");
            }

            TypeCheckStatementNode(whileNode.Body, errors, returnType, localEnvironment);
        }

        if (statementNode is StatementCompositionNode statementCompositionNode)
        {
            // S_1 : OK 
            // S_2 : OK 
            StatementNode? next = statementCompositionNode.NextStatement;

            if (next is not null)
            {
                TypeCheckStatementNode(next, errors, returnType, localEnvironment);
            }
        }


    }

    private TypeNode? FindExpressionType(ExpressionNode topExpression, List<string> errors, Table<Node>? localEnvironment)
    {
        try
        {
            return FindExpressionTypeInner(topExpression, localEnvironment);
        }
        catch (Exception e)
        {
            errors.Add(e.Message);
            return null;
        }

        TypeNode FindExpressionTypeInner(ExpressionNode expressionNode, Table<Node>? localEnvironment)
        {
            return expressionNode switch
            {
                // Further expressions. Clearly not optimized
                AddNode node => TypeCheckAddNode(node),
                AndNode node => TypeCheckAndNode(node),
                DivisionNode node => TypeCheckDivisionNode(node),
                EqualNode node => TypeCheckEqualNode(node),
                FunctionCallNode node => TypeCheckFunctionCall(node),
                IdentifierExpressionNode node => TypeCheckIdentifierNode(node, localEnvironment),
                LessThanNode node => TypeCheckLessThanNode(node),
                MultiplyNode node => TypeCheckMultiplyNode(node),
                NegativeNode node => TypeCheckNegativeNode(node),
                NotNode node => TypeCheckNotNode(node),
                ParenthesesNode node => FindRecursive(node.Inner),

                // Literals
                ArrayLiteralNode node => new ArrayTypeNode(node.LineNumber, FindRecursive(node.Elements[0])), //TODO: Make sure each element is same type
                BoolLiteralNode node => new BoolTypeNode(node.LineNumber),
                DoubleLiteralNode node => new DoubleTypeNode(node.LineNumber),
                IntLiteralNode node => new IntTypeNode(node.LineNumber),
                LiteralNode node => new StringTypeNode(node.LineNumber),
                _ => throw new($"Error: Not a valid expression! (Line {expressionNode.LineNumber})")
            };

            #region Local Functions

            TypeNode TypeCheckAddNode(AddNode node)
            {
                TypeNode leftType = FindRecursive(node.Left);
                TypeNode rightType = FindRecursive(node.Right);

                return (leftType, rightType) switch
                {
                    (DoubleTypeNode, DoubleTypeNode) => leftType,
                    (IntTypeNode, IntTypeNode) => leftType,
                    (DoubleTypeNode, IntTypeNode) => leftType,
                    (IntTypeNode, DoubleTypeNode) => rightType,
                    _ => throw new("Expression must be int or double")
                };
            }

            TypeNode TypeCheckAndNode(AndNode node)
            {
                TypeNode leftType = FindRecursive(node.Left);
                TypeNode rightType = FindRecursive(node.Right);

                return (leftType, rightType) switch
                {
                    (BoolTypeNode, BoolTypeNode) => leftType,
                    _ => throw new("Expressions must evaluate to bool")
                };
            }

            TypeNode TypeCheckDivisionNode(DivisionNode node)
            {
                TypeNode leftType = FindRecursive(node.Left);
                TypeNode rightType = FindRecursive(node.Right);

                return (leftType, rightType) switch
                {
                    (DoubleTypeNode, DoubleTypeNode) => leftType,
                    (IntTypeNode, IntTypeNode) => leftType,
                    (DoubleTypeNode, IntTypeNode) => leftType,
                    (IntTypeNode, DoubleTypeNode) => rightType,
                    _ => throw new("Expression must be int or double")
                };
            }

            TypeNode TypeCheckEqualNode(EqualNode node)
            {
                TypeNode leftType = FindRecursive(node.Left);
                TypeNode rightType = FindRecursive(node.Right);

                if (leftType.GetType() == rightType.GetType())
                    return new BoolTypeNode(node.LineNumber);
                else
                    throw new($"Cannot compare expressions of different types! (Line {node.LineNumber})");
            }

            TypeNode TypeCheckFunctionCall(FunctionCallNode node)
            {
                // TODO: PATRICK FIXER DET HER FORDI HVAD SKER DER LIGE?
                string identifier = GetIdentifiers(node.Identifier);
                if (environment.Lookup(identifier, out Node? functionBody))
                {
                    if (functionBody is FunctionNode funcNode)
                    {
                        return funcNode.ReturnType;
                    }
                    else
                    {
                        throw new($"Error: Identifier is not a function. (Line {node.LineNumber})");
                    }
                }
                else
                {
                    throw new($"Identifier '{identifier}' not found! (Line {node.LineNumber})");
                }
            }

            TypeNode TypeCheckIdentifierNode(IdentifierExpressionNode node, Table<Node>? localEnvironment)
            {
                Node? typeNode = null;
                localEnvironment?.Lookup(GetIdentifiers(node.Identifier), out typeNode);
                if (typeNode is null)
                {
                    environment.Lookup(GetIdentifiers(node.Identifier), out typeNode);
                    if (typeNode is null)
                    {
                        constEnvironment.Lookup(GetIdentifiers(node.Identifier), out typeNode);
                    }
                }

                if (typeNode is not null)
                {
                    if (typeNode is not TypeNode typeNodeCast)
                    {
                        throw new($"Not valid in this context! (Line {node.LineNumber})");
                    }
                    return typeNodeCast;
                }
                else
                {
                    throw new($"Identifier not found! (Line {node.LineNumber})");
                }
            }

            TypeNode TypeCheckLessThanNode(LessThanNode node)
            {
                TypeNode leftType = FindRecursive(node.Left);
                TypeNode rightType = FindRecursive(node.Right);

                BoolTypeNode expressionType = new(node.LineNumber);

                if (IsTypeIntOrDouble(leftType) && IsTypeIntOrDouble(rightType))
                    return expressionType;
                else
                    throw new("Expression must be int or double");
            }

            TypeNode TypeCheckMultiplyNode(MultiplyNode node)
            {
                TypeNode leftType = FindRecursive(node.Left);
                TypeNode rightType = FindRecursive(node.Right);

                return (leftType, rightType) switch
                {
                    (DoubleTypeNode, DoubleTypeNode) => leftType,
                    (IntTypeNode, IntTypeNode) => leftType,
                    (DoubleTypeNode, IntTypeNode) => leftType,
                    (IntTypeNode, DoubleTypeNode) => rightType,
                    _ => throw new("Expression must be int or double")
                };
            }

            TypeNode TypeCheckNegativeNode(NegativeNode node)
            {
                TypeNode innerType = FindRecursive(node.Inner);
                if (IsTypeIntOrDouble(innerType))
                    return innerType;
                else
                    throw new("Expression not int or double");
            }

            TypeNode TypeCheckNotNode(NotNode node)
            {
                TypeNode innerType = FindRecursive(node.Inner);
                if (innerType is not BoolTypeNode)
                    throw new($"Expression must evaluate to a bool (Line {node.LineNumber})");
                return innerType;
            }

            TypeNode FindRecursive(ExpressionNode expressionNode) => FindExpressionTypeInner(expressionNode, localEnvironment);
            #endregion
        }
    }

    static bool IsTypeIntOrDouble(TypeNode typeNode) => typeNode is IntTypeNode || typeNode is DoubleTypeNode;

    private string GetIdentifiers(IdentifierNode idNode)
    {
        if (idNode is SingleIdentifierNode node)
        {
            return node.Identifier;
        }
        else if (idNode is QualifiedIdentifierNode qualifiedIdentifierNode)
        {
            if (localNetworkScopesEnvironment.Lookup(qualifiedIdentifierNode.LeftIdentifier.Identifier, out Table<Node>? table))
            {
                return qualifiedIdentifierNode.RightIdentifier.Identifier;
            }
            return qualifiedIdentifierNode.LeftIdentifier.Identifier + "." + qualifiedIdentifierNode.RightIdentifier.Identifier;
        }
        else return "";
    }

    //These 2 methods could probably be combined, but it is not important
    private void TypeCheckInputs(IReadOnlyList<SingleIdentifierNode> io, Table<Node> localNetworkTable, List<string> errors)
    {
        foreach (SingleIdentifierNode node in io)
        {
            // not in dom(E) or dom(Sigma)
            if (environment.Lookup(node.Identifier, out Node? _))
                errors.Add($"Identifier already declared (Line {node.LineNumber})");

            // i != j => x_i != x_j
            if (localNetworkTable.TryBindIfNotExists(node.Identifier, new InputTypeNode(node.LineNumber)) == false)
            {
                errors.Add($"Duplicate identifier '{node.Identifier}' in network! (Line {node.LineNumber})");
            }
            // should be checked above, since we do it one identifier at a time, it and makes sure it is not defined before
        }
    }

    private void TypeCheckOutputs(IReadOnlyList<SingleIdentifierNode> io, Table<Node> localNetworkTable, List<string> errors)
    {
        foreach (SingleIdentifierNode node in io)
        {
            // not in dom(E) or dom(Sigma)
            if (environment.Lookup(node.Identifier, out Node? _))
                errors.Add($"Identifier already declared (Line {node.LineNumber})");

            // i != j => x_i != x_j
            if (localNetworkTable.TryBindIfNotExists(node.Identifier, new OutputTypeNode(node.LineNumber)) == false)
            {
                errors.Add($"Duplicate identifier '{node.Identifier}' in network! (Line {node.LineNumber})");
            }
            //Should always work since we alread looked it up

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
            if (queueMetricNames.Contains(metric.Name) == false)
                errors.Add($"'{metric.Name}' is not a valid metric for queues! (Line {metric.LineNumber})");
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
            if (networkMetricNames.Contains(metric.Name) == false)
                errors.Add($"'{metric.Name}' is not a valid metric for networks! (Line {metric.LineNumber})");
        }
    }

    private void TypeCheckInstances(IReadOnlyList<InstanceDeclaration> instances, Table<Node> localNetwork, List<string> errors)
    {
        foreach (InstanceDeclaration instance in instances)
        {
            //Start by checking if existing instance is proper
            string id = GetIdentifiers(instance.ExistingInstance);
            Node? instanceType = null;
            Table<Node>? instanceEnv = null;

            // Look through if the instance exists
            localNetwork.Lookup(id, out instanceType);
            localNetworkScopesEnvironment.Lookup(id, out instanceEnv);

            // If the instance exists and is a network or a queue, then it is all good
            if (instanceType is not null)
            {
                //Instance should only be able to be queue
                if (instanceType is not (InstanceDeclaration or NetworkTypeNode)) errors.Add($"Instance identifier is not queue or network. (Line {instance.LineNumber})");
            }
            else if (instanceEnv is not null)
            {
                // Happy days
            }
            else errors.Add($"Instance identifier '{id}' not found. (Line {instance.LineNumber})");

            // Check if new instance identifier is available
            id = GetIdentifiers(instance.NewInstance);
            instanceType = null;
            instanceEnv = null;

            localNetwork.Lookup(id, out instanceType);
            localNetworkScopesEnvironment.Lookup(id, out instanceEnv);
            constEnvironment.Lookup(id, out instanceType);

            if (instanceType is not null && instanceEnv is not null)
            {
                errors.Add($"Identifier of instance is already used. (Line {instance.LineNumber})");
            }
            else if (instanceType is null)
            {
                localNetwork.TryBindIfNotExists(id, instance);
            }

            /*// check existing
            string[] existingIdentifiers = GetIdentifiers(instance.ExistingInstance);
            if (environment.Lookup(existingIdentifiers[0], out Node? _)
            || localNetworkScopesEnvironment.Lookup(existingIdentifiers[0], out Table<Node>? _))
            {
                // bind new to same as existing
                string[] newIdentifiers = GetIdentifiers(instance.NewInstance);
                localNetwork.TryBindIfNotExists(newIdentifiers[0], instance.ExistingInstance);
            }
            else
                errors.Add($"Error: Instance identifier '{existingIdentifiers[0]}' not found (Line {instance.LineNumber})");*/
        }
    }

    private void TypeCheckRoutes(IReadOnlyList<RouteDefinitionNode> routes, Table<Node> localNetwork, List<string> errors)
    {
        foreach (RouteDefinitionNode routeDefinitionNode in routes)
        {
            ExpressionNode from = routeDefinitionNode.From;
            IReadOnlyList<RouteValuePairNode> to = routeDefinitionNode.To;
            // Check destination
            TypeCheckRouteDestination(to, localNetwork, errors);

            // If it is identifier node, then it is case 2 or 3
            if (routeDefinitionNode.From is IdentifierExpressionNode identifierExpressionNode)
            {
                // case 3
                if (localNetwork.Lookup(GetIdentifiers(identifierExpressionNode.Identifier), out Node? type))
                {
                    // If instance, then the check of whether it is queue or network will be checked at instance check
                    if (type is not (InstanceDeclaration or InputTypeNode or NetworkTypeNode)) errors.Add($"Input is not a valid queue or input (Line {identifierExpressionNode.LineNumber})");
                }
                // case 2
                else if (localNetworkScopesEnvironment.Lookup(GetIdentifiers(identifierExpressionNode.Identifier), out Table<Node>? network))
                {
                    network.Lookup(GetIdentifiers(identifierExpressionNode.Identifier), out Node? outputNode);
                    if (outputNode is not OutputTypeNode) errors.Add($"Second identifier must be an output node (Line {identifierExpressionNode.LineNumber})");
                }
                else errors.Add($"Identifier not found (Line {identifierExpressionNode.LineNumber})");
            }
            // Case 1
            else
            {
                Node? type = FindExpressionType(from, errors, localNetwork);
                // not case 1
                if (type is not IntTypeNode && type is not DoubleTypeNode) errors.Add($"Expression vlaue must be of type int or double (Line {from.LineNumber})");
            }
        }
    }

    private void TypeCheckRouteDestination(IReadOnlyList<RouteValuePairNode> destinations, Table<Node> localNetwork, List<string> errors)
    {
        foreach (RouteValuePairNode destination in destinations)
        {
            TypeNode? type = FindExpressionType(destination.Probability, errors, localNetwork);

            if (type is not null && IsTypeIntOrDouble(type) == false)
                errors.Add($"Route destination weight must be double or int (Line {destination.LineNumber})");

            QualifiedIdentifierNode? identifierNode = destination.RouteTo as QualifiedIdentifierNode;
            string? routeToIdentifier = identifierNode?.RightIdentifier.Identifier;

            if (routeToIdentifier is not null && localNetworkScopesEnvironment.Lookup(routeToIdentifier, out Table<Node>? net))
            {
                if (net.Lookup(routeToIdentifier, out Node? inputNode))
                {
                    if (inputNode is not InputTypeNode)
                        errors.Add($"Route to network identifier must be an input (Line {destination.LineNumber})");
                }
                else
                    errors.Add($"Network identifier not found (Line {destination.LineNumber})");
            }
            else if (routeToIdentifier is not null && localNetwork.Lookup(routeToIdentifier, out Node? caseTwoType))
            {
                // If it is bound as instance, then it is confirmed 5as a network or queue
                if (caseTwoType is not (InstanceDeclaration or OutputTypeNode or NetworkTypeNode)) errors.Add($"Instance not found. (Line {destination.LineNumber})");
            }
            else
            {
                errors.Add($"Instance not found. (Line {destination.LineNumber})");
            }
        }
    }
}