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
using SimEngine.Nodes;
using Node = Interpreter.AST.Nodes.Node;

namespace Interpreter.SemanticAnalysis;

public class TypeChecker
{
    TypeCheckerEnvironment globalEnvironment = new();
    Table<Node> ConstEnvironment => globalEnvironment.ConstEnvironment;
    Table<Node> Environment => globalEnvironment.Environment;
    Table<TypeCheckerNetworkState> LocalNetworkScopesEnvironment => globalEnvironment.LocalNetworkScopesEnvironment;


    // env for definitions and localEnv for statements? Return localEnv as new so it is not referenced
    public TypeCheckerEnvironment TypeCheckNode(Node node, List<string> errors)
    {
        if (node is ProgramNode programNode)
        {
            TypeCheckProgramNode(programNode, errors);
        }
        else errors.Add("Unexpected definition");

        // Return errors to parent node type check
        return globalEnvironment;
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
            string library = importNode.Namespace.Identifier;
            ProgramNode importedASTRoot = ModuleLoader.LoadModuleByName(library);

            List<string> importedErrors = [];
            TypeCheckerEnvironment importedEnvironment = new TypeChecker().TypeCheckNode(importedASTRoot, importedErrors);

            if (globalEnvironment.Dependencies.TryBindIfNotExists(library, importedEnvironment) == false)
                errors.Add($"Import name '{library}' already used (Line {importNode.LineNumber})");

            errors.AddRange(importedErrors.Select(error => $"{library}: {error}"));
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
            object? expressionType = FindExpressionType(cdNode.Expression, errors, localEnvironment: null);
            if (expressionType is not null && expressionType.GetType() != cdNode.Type.GetType()) errors.Add($"Error: Expression type must match declaration type. (Line {cdNode.LineNumber})");

            // Try binding and error if fail
            if (!ConstEnvironment.TryBindIfNotExists(cdNode.Identifier.Identifier, cdNode.Type)) errors.Add($"Error: Identifier already declared. (Line {cdNode.LineNumber})");
        }
        else if (defNode is FunctionNode funcNode)
        {
            // x not in dom(E)
            // bind function identifier to E1
            // bind function parameters to new E and pass them to S
            // pass new E to next definition (also happens later automatically)

            // bind function identifier to E
            if (Environment.TryBindIfNotExists(funcNode.Identifier.Identifier, funcNode))
            {
                // E1
                Table<Node> newEnvironment = new(Environment);

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
            object? found = GetTypeFromIdentifier(simNode.NetworkIdentifier, globalEnvironment, null, errors);
            if (found is null)
                errors.Add($"Network identifier not found (Line {simNode.NetworkIdentifier.LineNumber})");
            else if (found.GetType() != typeof(TypeCheckerNetworkState))
                errors.Add($"Identifier is not of type network! (Line {simNode.NetworkIdentifier.LineNumber})");

            // Expression is int
            if (FindExpressionType(simNode.Runs, errors, localEnvironment: null) is not IntTypeNode)
                errors.Add($"Expression for runs must be of type int (Line {simNode.Runs.LineNumber})");
            if (FindExpressionType(simNode.TerminationCriteria, errors, localEnvironment: null) is not IntTypeNode)
                errors.Add($"Expression for termination criteria must be of type int (Line {simNode.TerminationCriteria.LineNumber})");
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
            object? @object = GetTypeFromIdentifier(networkDeclarationNode.Identifier, globalEnvironment, null, errors);
            if (@object is not null)
                errors.Add($"Identifier already declared (Line {networkDeclarationNode.LineNumber})");

            // Make Sigma
            TypeCheckerNetworkState typeCheckerNetworkState = new(networkDeclarationNode, Environment);

            // input
            TypeCheckInputs(typeCheckerNetworkState, errors);

            // output
            TypeCheckOutputs(typeCheckerNetworkState, errors);

            // instances
            TypeCheckInstances(typeCheckerNetworkState, errors);

            // routes
            TypeCheckRoutes(typeCheckerNetworkState, errors);

            // metrics
            TypeCheckNetworkMetricList(typeCheckerNetworkState, errors);

            // bind to network env
            LocalNetworkScopesEnvironment.TryBindIfNotExists(networkDeclarationNode.Identifier.Identifier, typeCheckerNetworkState);

            /// isvalid? Check that there is routing from all inputs, routing from all instances and routing to all outputs
            NetworkIsValid(errors, networkDeclarationNode, typeCheckerNetworkState.localScope);
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
            object? @object = GetTypeFromIdentifier(queueDeclarationNode.Identifier, globalEnvironment, null, errors);
            if (@object is not null)
                errors.Add($"Identifier already declared (Line {queueDeclarationNode.LineNumber})");

            // service expression...
            object? serviceType = FindExpressionType(queueDeclarationNode.Service, errors, localEnvironment: null);
            if (serviceType is not null && serviceType is not IntTypeNode && serviceType is not DoubleTypeNode)
                errors.Add("Service expression must be int or double");

            // capacity expression...
            object? capcityType = FindExpressionType(queueDeclarationNode.Capacity, errors, localEnvironment: null);
            if (capcityType is not null && capcityType is not IntTypeNode)
                errors.Add($"Capacity expression must be int (Line {queueDeclarationNode.Capacity.LineNumber})");

            // servers expression...
            object? serversType = FindExpressionType(queueDeclarationNode.Servers, errors, localEnvironment: null);
            if (serversType is not null && serversType is not IntTypeNode)
                errors.Add($"Number of servers expression must be int (Line {queueDeclarationNode.Servers.LineNumber})");

            // capacity expression...
            TypeCheckQueueMetricList(queueDeclarationNode.Metrics, errors);

            // bind x to queue
            if (Environment.TryBindIfNotExists(queueDeclarationNode.Identifier.Identifier, queueDeclarationNode) == false)
            {
                errors.Add($"Identifier '{queueDeclarationNode.Identifier.Identifier}' already exists! (Line {queueDeclarationNode.LineNumber})");
            }

        }

    }

    private void NetworkIsValid(List<string> errors, NetworkDeclarationNode networkDeclarationNode, Table<Node> localNetwork)
    {
        // routing from all inputs
        foreach (SingleIdentifierNode input in networkDeclarationNode.Inputs)
        {
            bool inputFound = false;

            foreach (RouteDefinitionNode route in networkDeclarationNode.Routes.Cast<RouteDefinitionNode>())
            {
                if (route.From is IdentifierExpressionNode idExprNode)
                {
                    if (idExprNode.Identifier.FirstIdentifier == input.Identifier)
                        inputFound = true;
                }
            }

            if (!inputFound)
                errors.Add($"Input '{input.FullIdentifier}' not used (Line {input.LineNumber})");
        }

        // routing from all instances
        foreach (InstanceDeclaration instance in networkDeclarationNode.Instances.Skip(1))
        {
            bool instanceFound = false;

            foreach (RouteDefinitionNode route in networkDeclarationNode.Routes.Cast<RouteDefinitionNode>())
            {
                if (route.From is IdentifierExpressionNode idExprNode)
                {
                    if (idExprNode.Identifier.FirstIdentifier == instance.NewInstance.Identifier)
                    {
                        instanceFound = true;
                        break;
                    }
                }
            }

            if (!instanceFound)
                errors.Add($"instance '{instance.ExistingInstance.FirstIdentifier}' not used (Line {instance.LineNumber})");
        }

        // routing to all outputs
        foreach (SingleIdentifierNode output in networkDeclarationNode.Outputs)
        {
            bool outputFound = false;

            foreach (RouteDefinitionNode route in networkDeclarationNode.Routes.Cast<RouteDefinitionNode>())
            {
                foreach (RouteValuePairNode toNode in route.To)
                {
                    if (toNode.RouteTo.FirstIdentifier == output.Identifier) outputFound = true;
                }
            }

            if (!outputFound) errors.Add($"Output '{output.FullIdentifier}' not used (Line {output.LineNumber})");
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
                bool isConst = ConstEnvironment.Lookup(assignNode.Identifier.Identifier, out Node? _);
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
            object? expressionNode = FindExpressionType(returnNode.Expression, errors, localEnvironment);

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
            object? expressionType = FindExpressionType(variableDeclarationNode.Expression, errors, localEnvironment);

            if (expressionType is not TypeNode || expressionType.GetType() != variableType.GetType())
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

    private object? FindExpressionType(ExpressionNode topExpression, List<string> errors, Table<Node>? localEnvironment)
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

        object? FindExpressionTypeInner(ExpressionNode expressionNode, Table<Node>? localEnvironment)
        {
            return expressionNode switch
            {
                // Further expressions. Clearly not optimized
                AddNode node => TypeCheckAddNode(node),
                AndNode node => TypeCheckAndNode(node),
                DivisionNode node => TypeCheckDivisionNode(node),
                EqualNode node => TypeCheckEqualNode(node),
                FunctionCallNode node => TypeCheckFunctionCall(node),
                IdentifierExpressionNode node => TypeCheckIdentifierNode(node),
                LessThanNode node => TypeCheckLessThanNode(node),
                MultiplyNode node => TypeCheckMultiplyNode(node),
                NegativeNode node => TypeCheckNegativeNode(node),
                NotNode node => TypeCheckNotNode(node),
                ParenthesesNode node => FindRecursive(node.Inner),

                // Literals
                ArrayLiteralNode node => TypeCheckArrayLiteralNode(node), //TODO: Make sure each element is same type
                BoolLiteralNode node => new BoolTypeNode(node.LineNumber),
                DoubleLiteralNode node => new DoubleTypeNode(node.LineNumber),
                IntLiteralNode node => new IntTypeNode(node.LineNumber),
                LiteralNode node => new StringTypeNode(node.LineNumber),
                _ => throw new($"Error: Not a valid expression! (Line {expressionNode.LineNumber})")
            };

            #region Local Functions

            object TypeCheckAddNode(AddNode node)
            {
                object? leftType = FindRecursive(node.Left);
                object? rightType = FindRecursive(node.Right);

                return (leftType, rightType) switch
                {
                    (DoubleTypeNode, DoubleTypeNode) => leftType,
                    (IntTypeNode, IntTypeNode) => leftType,
                    (DoubleTypeNode, IntTypeNode) => leftType,
                    (IntTypeNode, DoubleTypeNode) => rightType,
                    _ => throw new("Expression must be int or double")
                };
            }

            object TypeCheckAndNode(AndNode node)
            {
                object? leftType = FindRecursive(node.Left);
                object? rightType = FindRecursive(node.Right);

                return (leftType, rightType) switch
                {
                    (BoolTypeNode, BoolTypeNode) => leftType,
                    _ => throw new("Expressions must evaluate to bool")
                };
            }

            object TypeCheckDivisionNode(DivisionNode node)
            {
                object? leftType = FindRecursive(node.Left);
                object? rightType = FindRecursive(node.Right);

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
                object? leftType = FindRecursive(node.Left);
                object? rightType = FindRecursive(node.Right);

                if (leftType is null)
                    throw new($"Expression must evaluate to a value (Line {node.Left.LineNumber})");
                if (rightType is null)
                    throw new($"Expression must evaluate to a value (Line {node.Right.LineNumber})");

                if (leftType.GetType() == rightType.GetType())
                    return new BoolTypeNode(node.LineNumber);
                else
                    throw new($"Cannot compare expressions of different types! (Line {node.LineNumber})");
            }

            TypeNode TypeCheckFunctionCall(FunctionCallNode node)
            {
                object? @object = GetTypeFromIdentifier(node.Identifier, globalEnvironment, localEnvironment, errors);

                if (@object is FunctionNode funcNode)
                {
                    return funcNode.ReturnType;
                }
                else
                {
                    throw new($"Error: Identifier is not a function. (Line {node.LineNumber})");
                }
            }

            object? TypeCheckIdentifierNode(IdentifierExpressionNode node)
            {
                object? typeNode = GetTypeFromIdentifier(node.Identifier, globalEnvironment, localEnvironment, errors);

                if (typeNode is not null)
                {
                    return typeNode;
                }
                else
                {
                    throw new($"Identifier not found! (Line {node.LineNumber})");
                }
            }

            object TypeCheckLessThanNode(LessThanNode node)
            {
                object? leftType = FindRecursive(node.Left);
                object? rightType = FindRecursive(node.Right);

                BoolTypeNode expressionType = new(node.LineNumber);

                if (IsTypeIntOrDouble(leftType) && IsTypeIntOrDouble(rightType))
                    return expressionType;
                else
                    throw new("Expression must be int or double");
            }

            object TypeCheckMultiplyNode(MultiplyNode node)
            {
                object? leftType = FindRecursive(node.Left);
                object? rightType = FindRecursive(node.Right);

                return (leftType, rightType) switch
                {
                    (DoubleTypeNode, DoubleTypeNode) => leftType,
                    (IntTypeNode, IntTypeNode) => leftType,
                    (DoubleTypeNode, IntTypeNode) => leftType,
                    (IntTypeNode, DoubleTypeNode) => rightType,
                    _ => throw new("Expression must be int or double")
                };
            }

            object TypeCheckNegativeNode(NegativeNode node)
            {
                object? innerType = FindRecursive(node.Inner);
                if (IsTypeIntOrDouble(innerType))
                    return innerType!;
                else
                    throw new("Expression not int or double");
            }

            object TypeCheckNotNode(NotNode node)
            {
                object? innerType = FindRecursive(node.Inner);
                if (innerType is not BoolTypeNode)
                    throw new($"Expression must evaluate to a bool (Line {node.LineNumber})");
                return innerType;
            }

            ArrayTypeNode TypeCheckArrayLiteralNode(ArrayLiteralNode node)
            {
                object? @object = FindRecursive(node.Elements[0]);
                if (@object is not TypeNode oldTypeNode)
                {
                    throw new($"Expression must evaluate to a type! (Line {node.Elements[0].LineNumber})");
                }

                foreach (ExpressionNode element in node.Elements.Skip(1))
                {

                    object? elementType = FindRecursive(element);
                    if (@object is not TypeNode typeNode)
                    {
                        throw new($"Expression must evaluate to a type! (Line {element.LineNumber})");
                    }
                    if (oldTypeNode.GetType() != typeNode.GetType())
                    {
                        throw new($"Types must match! (Line {typeNode.LineNumber})");
                    }
                    oldTypeNode = typeNode;
                }

                return new ArrayTypeNode(node.LineNumber, oldTypeNode);
            }

            object? FindRecursive(ExpressionNode expressionNode) => FindExpressionTypeInner(expressionNode, localEnvironment);

            #endregion
        }
    }

    static bool IsTypeIntOrDouble(object? typeNode) => typeNode is IntTypeNode || typeNode is DoubleTypeNode;

    private object? GetTypeFromIdentifier(IdentifierNode idNode, TypeCheckerEnvironment environmentToCheck, Table<Node>? localScope, List<string> errors)
    {
        string firstIdentifier = idNode.FirstIdentifier;
        object? returnValue = null;
        if (localScope is not null)
        {
            localScope.Lookup(firstIdentifier, out Node? @out);
            if (@out is NetworkDeclarationNode networkDeclarationNode && idNode is QualifiedIdentifierNode qualifiedIdentifierNode)
            {
                returnValue = GetTypeFromIdentifier(new QualifiedIdentifierNode(@out.LineNumber, networkDeclarationNode.Identifier, qualifiedIdentifierNode.RightIdentifier), environmentToCheck, localScope, errors);
            }
            else
            {
                returnValue = @out;
            }
        }
        if (returnValue is null)
        {
            environmentToCheck.ConstEnvironment.Lookup(firstIdentifier, out Node? @out);
            returnValue = @out;
        }
        if (returnValue is null)
        {
            environmentToCheck.Environment.Lookup(firstIdentifier, out Node? @out);
            returnValue = @out;
        }
        if (returnValue is null)
        {
            environmentToCheck.LocalNetworkScopesEnvironment.Lookup(firstIdentifier, out TypeCheckerNetworkState? outNetwork);
            if (outNetwork is not null && idNode is QualifiedIdentifierNode qualifiedIdentifierNode)
            {
                outNetwork.localScope.Lookup(qualifiedIdentifierNode.RightIdentifier.Identifier, out Node? @out);
                returnValue = @out;
            }
            else
            {
                returnValue = outNetwork;
            }
        }
        if (returnValue is null)
        {
            environmentToCheck.Dependencies.Lookup(firstIdentifier, out TypeCheckerEnvironment? dependency);
            if (dependency is not null && idNode is QualifiedIdentifierNode qualifiedIdentifierNode)
            {
                returnValue = GetTypeFromIdentifier(qualifiedIdentifierNode.RightIdentifier, dependency, localScope, errors);
            }/*
            else
            {
                errors.Add($"Identifier not found! (Line {idNode.LineNumber})");
            }*/
        }

        return returnValue;

    }

    //These 2 methods could probably be combined, but it is not important
    private void TypeCheckInputs(TypeCheckerNetworkState localNetworkTable, List<string> errors)
    {
        foreach (SingleIdentifierNode node in localNetworkTable.NetworkNode.Inputs)
        {
            // not in dom(E) or dom(Sigma)
            if (Environment.Lookup(node.Identifier, out Node? _))
                errors.Add($"Identifier already declared (Line {node.LineNumber})");

            // i != j => x_i != x_j
            if (localNetworkTable.localScope.TryBindIfNotExists(node.Identifier, new InputTypeNode(node.LineNumber)) == false)
            {
                errors.Add($"Duplicate identifier '{node.Identifier}' in network! (Line {node.LineNumber})");
            }
            // should be checked above, since we do it one identifier at a time, it and makes sure it is not defined before
        }
    }

    private void TypeCheckOutputs(TypeCheckerNetworkState typeCheckerNetworkState, List<string> errors)
    {
        foreach (SingleIdentifierNode node in typeCheckerNetworkState.NetworkNode.Outputs)
        {
            // not in dom(E) or dom(Sigma)
            if (Environment.Lookup(node.Identifier, out Node? _))
                errors.Add($"Identifier already declared (Line {node.LineNumber})");

            // i != j => x_i != x_j
            if (typeCheckerNetworkState.localScope.TryBindIfNotExists(node.Identifier, new OutputTypeNode(node.LineNumber)) == false)
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

    private static void TypeCheckNetworkMetricList(TypeCheckerNetworkState typeCheckerNetworkState, List<string> errors)
    {
        foreach (NamedMetricNode metric in typeCheckerNetworkState.NetworkNode.Metrics)
        {
            if (networkMetricNames.Contains(metric.Name) == false)
                errors.Add($"'{metric.Name}' is not a valid metric for networks! (Line {metric.LineNumber})");
        }
    }

    private void TypeCheckInstances(TypeCheckerNetworkState typeCheckerNetworkState, List<string> errors)
    {
        object? Get(IdentifierNode identifierNode) => GetTypeFromIdentifier(identifierNode, globalEnvironment, typeCheckerNetworkState.localScope, errors);

        foreach (InstanceDeclaration instance in typeCheckerNetworkState.NetworkNode.Instances)
        {
            //Start by checking if existing instance is proper
            object? network = Get(instance.ExistingInstance);

            // Check if new instance identifier is available
            object? newInstance = Get(instance.NewInstance);
            if (newInstance is not null)
            {
                errors.Add($"Identifier of instance is already used. (Line {instance.LineNumber})");
            }

            // If the instance exists and is a network or a queue, then it is all good
            if (network is TypeCheckerNetworkState networkState)
            {
                typeCheckerNetworkState.localScope.TryBindIfNotExists(instance.NewInstance.FirstIdentifier, networkState.NetworkNode);
            }
            else if (network is NetworkNode networkNode)
            {
                typeCheckerNetworkState.localScope.TryBindIfNotExists(instance.NewInstance.FirstIdentifier, networkNode);
            }
            else
            {
                errors.Add($"Instance identifier is not queue or network. (Line {instance.LineNumber})");
            }
        }
    }

    private void TypeCheckRoutes(TypeCheckerNetworkState typeCheckerNetworkState, List<string> errors)
    {
        foreach (RouteDefinitionNode routeDefinitionNode in typeCheckerNetworkState.NetworkNode.Routes)
        {
            ExpressionNode from = routeDefinitionNode.From;
            IReadOnlyList<RouteValuePairNode> to = routeDefinitionNode.To;
            // Check destination
            TypeCheckRouteDestination(to, typeCheckerNetworkState.localScope, errors);

            object? type = FindExpressionType(routeDefinitionNode.From, errors, typeCheckerNetworkState.localScope);
            if (type is not (IntTypeNode or DoubleTypeNode or OutputTypeNode or QueueDeclarationNode or InputTypeNode))
            {
                errors.Add($"From routing is neither int, double or instance! (Line {routeDefinitionNode.LineNumber})");
            }
        }
    }

    private void TypeCheckRouteDestination(IReadOnlyList<RouteValuePairNode> destinations, Table<Node> localNetwork, List<string> errors)
    {
        foreach (RouteValuePairNode destination in destinations)
        {
            object? type = FindExpressionType(destination.Probability, errors, localNetwork);

            if (type is not null && IsTypeIntOrDouble(type) == false)
                errors.Add($"Route destination weight must be double or int (Line {destination.LineNumber})");

            if (destination.RouteTo is not IdentifierNode identifierNode)
            {
                errors.Add($"Route destination must be an identifier (Line {destination.LineNumber})");
                continue;
            }

            object? @object = GetTypeFromIdentifier(destination.RouteTo, globalEnvironment, localNetwork, errors);

            if (@object is null)
            {
                errors.Add($"Route destination identifier '{destination.RouteTo}' not found (Line {destination.RouteTo.LineNumber})");
                continue;
            }

            if (@object is QueueDeclarationNode or InputTypeNode or OutputTypeNode == false)
            {
                errors.Add($"Route destination must be a queue or output (Line {destination.LineNumber})");
            }
        }
    }
}