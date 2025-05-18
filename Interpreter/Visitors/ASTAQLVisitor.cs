


using Antlr4.Runtime.Misc;
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
using Interpreter.AST.NonNodes;

namespace Interpreter.Visitors;

class ASTAQLVisitor : AQLBaseVisitor<object>
{
    public override ProgramNode VisitProgramEOF([NotNull] AQLParser.ProgramEOFContext context)
    {
        return VisitProgram(context.program());
    }

    public override ProgramNode VisitProgram([NotNull] AQLParser.ProgramContext context)
    {
        List<ImportNode> importNodes = [];
        List<DefinitionNode> definitionNodes = [];

        ProgramNode? programNode = null;

        if (context.importStatement() != null)
        {
            programNode = VisitImportStatement(context.importStatement());
        }

        else if (context.definition() != null)
        {
            DefinitionNode definitionNode = VisitDefinition(context.definition());
            programNode = new DefinitionProgramNode(
                lineNumber: context.Start.Line,
                definition: definitionNode
            );
        }

        if (programNode is null)
        {
            return new(
                lineNumber: context.Start.Line
            );
        }
        else
        {
            return programNode;
        }
    }

    public override DefinitionNode VisitDefinition([NotNull] AQLParser.DefinitionContext context)
    {
        DefinitionNode definitionNode;
        if (context.functionDefinition() != null)
        {
            definitionNode = VisitFunctionDefinition(context.functionDefinition());
        }
        else if (context.constDefinition() != null)
        {
            definitionNode = VisitConstDefinition(context.constDefinition());
        }
        else if (context.networks() != null)
        {
            definitionNode = VisitNetworks(context.networks());
        }
        else if (context.simulateDefinition() != null)
        {
            definitionNode = VisitSimulateDefinition(context.simulateDefinition());
        }
        else
        {
            throw new("Not a valid definition.");
        }

        return definitionNode;
    }

    public override ImportNode VisitImportStatement([NotNull] AQLParser.ImportStatementContext context)
    {
        SingleIdentifierNode identifierNode = VisitIdentifier(context.identifier());

        ProgramNode? nextProgram = null;

        if (context.program() != null)
        {
            nextProgram = VisitProgram(context.program());
        }

        return new(
            lineNumber: context.Start.Line,
            nextProgram: nextProgram,
            @namespace: identifierNode
        );
    }

    public override FunctionNode VisitFunctionDefinition([NotNull] AQLParser.FunctionDefinitionContext context)
    {
        TypeNode returnTypeNode = VisitType(context.returnType);
        SingleIdentifierNode identifierNode = VisitIdentifier(context.identifier());

        IEnumerable<TypeAndIdentifier> parameterNodes;
        if (context.formalParameterList() is null)
        {
            parameterNodes = [];
        }
        else
        {
            parameterNodes = VisitFormalParameterList(context.formalParameterList());
        }

        StatementNode statementNode = VisitBlock(context.block());

        DefinitionNode? nextDefinition = null;
        if (context.definition() != null)
        {
            nextDefinition = VisitDefinition(context.nextDefinition);
        }

        return new(
            lineNumber: context.Start.Line,
            nextDefinition: nextDefinition,
            returnType: returnTypeNode,
            identifier: identifierNode,
            parameters: parameterNodes,
            body: statementNode
        );
    }

    public override ConstDeclarationNode VisitConstDefinition([NotNull] AQLParser.ConstDefinitionContext context)
    {
        TypeNode typeNode = VisitType(context.type());
        AssignNode assignNode = VisitAssignStatement(context.assignStatement());

        DefinitionNode? nextDefinition = null;
        if (context.definition() != null)
        {
            nextDefinition = VisitDefinition(context.nextDefinition);
        }

        return new(
            lineNumber: context.Start.Line,
            nextDefinition: nextDefinition,
            type: typeNode,
            identifier: assignNode.Identifier,
            expression: assignNode.Expression
        );
    }

    public override StatementNode VisitStatement([NotNull] AQLParser.StatementContext context)
    {
        if (context.whileStatement() != null)
        {
            return VisitWhileStatement(context.whileStatement());
        }
        else if (context.variableDeclarationStatement() != null)
        {
            return VisitVariableDeclarationStatement(context.variableDeclarationStatement());
        }
        else if (context.assignStatement() != null)
        {
            return VisitAssignStatement(context.assignStatement());
        }
        else if (context.ifStatement() != null)
        {
            return VisitIfStatement(context.ifStatement());
        }
        else if (context.returnStatement() != null)
        {
            return VisitReturnStatement(context.returnStatement());
        }
        else
        {
            throw new("Not a valid statement.");
        }
    }

    public override WhileNode VisitWhileStatement([NotNull] AQLParser.WhileStatementContext context)
    {
        ExpressionNode conditionNode = VisitExpression(context.condition);
        StatementNode bodyNode = VisitBlock(context.body);

        StatementNode? nextStatement = null;
        if (context.statement() != null)
        {
            nextStatement = VisitStatement(context.nextStatement);
        }

        return new(
            lineNumber: context.Start.Line,
            nextStatement: nextStatement,
            condition: conditionNode,
            body: bodyNode
        );
    }

    public override StatementNode VisitBlock([NotNull] AQLParser.BlockContext context)
    {
        return VisitStatement(context.statement());
    }

    public override VariableDeclarationNode VisitVariableDeclarationStatement([NotNull] AQLParser.VariableDeclarationStatementContext context)
    {
        TypeNode typeNode = VisitType(context.type());
        AssignNode assignNode = VisitAssignStatement(context.assignStatement());

        return new(
                lineNumber: context.Start.Line,
                nextStatement: assignNode.NextStatement,
                type: typeNode,
                identifier: assignNode.Identifier,
                expression: assignNode.Expression
            );
    }

    public override IfElseNode VisitIfStatement([NotNull] AQLParser.IfStatementContext context)
    {
        StatementNode? nextStatement = null;
        if (context.statement() != null)
        {
            nextStatement = VisitStatement(context.nextStatement);
        }

        // We handle the 'if elseif* else?' statements in 
        // reverse.

        // Handling 'else?', if it isn't there semanticly 
        // means a 'skip' statement has been used.
        StatementNode elseBody;
        if (context.elseIfStatement() != null)
        {
            elseBody = VisitElseStatement(context.elseStatement());
        }
        else
        {
            elseBody = new SkipNode(lineNumber: context.Start.Line);
        }

        IfElseNode? currentNode = null;
        if (context._elseIfStatements != null && context._elseIfStatements.Count > 0)
        {
            // We handle the 'if elseif* else?' statements in 
            // reverse in order to create the immutable node 
            // classes.
            List<AQLParser.ElseIfStatementContext> elseIfStatementContexts = [.. context._elseIfStatements];
            elseIfStatementContexts.Reverse();

            ElseIfReturn firstElseIfReturn = VisitElseIfStatement(elseIfStatementContexts.First());
            currentNode = new(
                lineNumber: context.Start.Line,
                nextStatement: nextStatement,
                condition: firstElseIfReturn.Condition,
                ifBody: firstElseIfReturn.Body,
                elseBody: elseBody
            );
            foreach (AQLParser.ElseIfStatementContext elseIfStatementContext in elseIfStatementContexts.Skip(1))
            {
                ElseIfReturn elseIfReturn = VisitElseIfStatement(elseIfStatementContext);
                currentNode = new(
                    lineNumber: context.Start.Line,
                    nextStatement: nextStatement,
                    elseIfReturn: elseIfReturn,
                    elseBody: currentNode
                );
            }
        }

        ExpressionNode mainIfCondition = VisitExpression(context.ifCondition);
        StatementNode mainIfBody = VisitBlock(context.ifBody);

        // If no 'elseif's where present the 'else' should just 
        // be the else body, as it is not part of the 'elseif' 
        // chain.

        // Otherwise the chain is appended to the first if case.
        if (currentNode == null)
        {
            currentNode = new(
                lineNumber: context.Start.Line,
                nextStatement: nextStatement,
                condition: mainIfCondition,
                ifBody: mainIfBody,
                elseBody: elseBody
            );
        }

        else
        {
            currentNode = new(
                lineNumber: context.Start.Line,
                nextStatement: nextStatement,
                condition: mainIfCondition,
                ifBody: mainIfBody,
                elseBody: currentNode
            );
        }

        return currentNode;
    }

    public override ElseIfReturn VisitElseIfStatement([NotNull] AQLParser.ElseIfStatementContext context)
    {
        ExpressionNode conditionNode = VisitExpression(context.condition);
        StatementNode bodyNode = VisitBlock(context.body);

        return new ElseIfReturn(
            condition: conditionNode,
            body: bodyNode
        );
    }

    public override StatementNode VisitElseStatement([NotNull] AQLParser.ElseStatementContext context)
    {
        return VisitBlock(context.body);
    }

    public override ReturnNode VisitReturnStatement([NotNull] AQLParser.ReturnStatementContext context)
    {
        ExpressionNode expressionNode = VisitExpression(context.expression());

        return new(
            lineNumber: context.Start.Line,
            expression: expressionNode
        );
    }

    public override NetworkDefinitionNode VisitNetworks([NotNull] AQLParser.NetworksContext context)
    {
        NetworkNode networkNode;
        if (context.networkDefinition() != null)
        {
            networkNode = VisitNetworkDefinition(context.networkDefinition());
        }
        else if (context.queueDefinition() != null)
        {
            networkNode = VisitQueueDefinition(context.queueDefinition());
        }
        else
        {
            throw new("Not a valid network.");
        }

        DefinitionNode? nextDefinition = null;
        if (context.definition() != null)
        {
            nextDefinition = VisitDefinition(context.definition());
        }

        return new(
            lineNumber: context.Start.Line,
            nextDefinition: nextDefinition,
            network: networkNode
        );
    }

    public override QueueDeclarationNode VisitQueueDefinition([NotNull] AQLParser.QueueDefinitionContext context)
    {
        SingleIdentifierNode identifierNode = VisitIdentifier(context.identifier());
        ExpressionNode serviceNode = VisitExpression(context.service);
        ExpressionNode capacityNode = VisitExpression(context.capacity);

        ExpressionNode numberOfServersNode;
        if (context.numberOfServers is null)
        {
            numberOfServersNode = new IntLiteralNode(
                lineNumber: context.Start.Line,
                value: 1
            );
        }
        else
        {
            numberOfServersNode = VisitExpression(context.numberOfServers);
        }

        IEnumerable<NamedMetricNode> metricNodes;
        if (context.metrics() is null)
        {
            metricNodes = [];
        }
        else
        {
            metricNodes = VisitMetrics(context.metrics());
        }

        return new(
            lineNumber: context.Start.Line,
            customType: new(
                lineNumber: context.Start.Line,
                identifier: identifierNode
            ),
            identifierNode,
            serviceNode,
            capacityNode,
            numberOfServersNode,
            metricNodes
        );
    }

    public override NetworkDeclarationNode VisitNetworkDefinition([NotNull] AQLParser.NetworkDefinitionContext context)
    {
        SingleIdentifierNode identifierNode = VisitIdentifier(context.identifier());

        List<SingleIdentifierNode> inputNodes = [];
        List<SingleIdentifierNode> outputNodes = [];
        List<InstanceDeclaration> instanceNodes = [];
        List<RouteDefinitionNode> routeNodes = [];
        List<NamedMetricNode> metricNodes = [];

        foreach (AQLParser.NetworkExpressionContext networkExpressionContext in context.networkExpression())
        {
            IEnumerable<object> networkExpressions = VisitNetworkExpression(networkExpressionContext);

            if (TryCast(networkExpressions, out IEnumerable<NetworkInputOutputNode>? inputOutputCast))
            {
                inputNodes.AddRange(inputOutputCast.First().Inputs);
                outputNodes.AddRange(inputOutputCast.First().Outputs);
            }
            else if (TryCast(networkExpressions, out IEnumerable<InstanceDeclaration>? instanceCast))
            {
                instanceNodes.AddRange(instanceCast);
            }
            else if (TryCast(networkExpressions, out IEnumerable<RouteDefinitionNode>? routeCast))
            {
                routeNodes.AddRange(routeCast);
            }
            else if (TryCast(networkExpressions, out IEnumerable<NamedMetricNode>? metricCast))
            {
                metricNodes.AddRange(metricCast);
            }
            else
            {
                throw new("Cast failed");
            }
        }

        return new(
            lineNumber: context.Start.Line,
            customType: new(
                lineNumber: context.Start.Line,
                identifier: identifierNode
            ),
            identifier: identifierNode,
            inputs: inputNodes,
            outputs: outputNodes,
            instances: instanceNodes,
            routes: routeNodes,
            metrics: metricNodes
        );
    }

    public override IEnumerable<object> VisitNetworkExpression([NotNull] AQLParser.NetworkExpressionContext context)
    {
        if (context.inputOutputNetworkExpression() != null)
        {
            return VisitInputOutputNetworkExpression(context.inputOutputNetworkExpression());
        }
        else if (context.instanceNetworkExpression() != null)
        {
            return VisitInstanceNetworkExpression(context.instanceNetworkExpression());
        }
        else if (context.routes() != null)
        {
            return VisitRoutes(context.routes()).Cast<Node>();
        }
        else if (context.metrics() != null)
        {
            return VisitMetrics(context.metrics());
        }
        else
        {
            throw new("Not a valid network expression.");
        }
    }

    public override IEnumerable<NetworkInputOutputNode> VisitInputOutputNetworkExpression([NotNull] AQLParser.InputOutputNetworkExpressionContext context)
    {
        IEnumerable<SingleIdentifierNode> inputNodes = VisitIdList(context.inputs);
        IEnumerable<SingleIdentifierNode> outputNodes = VisitIdList(context.outputs);

        return [
            new(
                inputs: inputNodes,
                outputs: outputNodes
            ),
        ];
    }

    public override IEnumerable<InstanceDeclaration> VisitInstanceNetworkExpression([NotNull] AQLParser.InstanceNetworkExpressionContext context)
    {
        IdentifierNode exisitingInstance = VisitAnyIdentifier(context.existing);
        IEnumerable<SingleIdentifierNode> newInstances = VisitIdList(context.@new);

        List<InstanceDeclaration> instanceDeclarations = [];
        foreach (SingleIdentifierNode newInstance in newInstances)
        {
            InstanceDeclaration instanceDeclaration = new(
                lineNumber: context.Start.Line,
                existingInstance: exisitingInstance,
                newInstance: newInstance
            );
            instanceDeclarations.Add(instanceDeclaration);
        }

        return instanceDeclarations;
    }

    private static bool TryCast<T>(IEnumerable<object> nodes, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out IEnumerable<T> cast)
    {
        if (nodes.Any() == false || nodes.First() is not T)
        {
            cast = null;
            return false;
        }

        try
        {
            cast = nodes.Cast<T>();
            return true;
        }
        catch (InvalidCastException)
        {
            cast = null;
            return false;
        }
    }

    public override AssignNode VisitAssignStatement([NotNull] AQLParser.AssignStatementContext context)
    {
        SingleIdentifierNode identifierNode = VisitIdentifier(context.identifier());
        ExpressionNode expressionNode = VisitExpression(context.expression());

        StatementNode? nextStatement = null;
        if (context.nextStatement != null)
        {
            nextStatement = VisitStatement(context.nextStatement);
        }

        return new(
            lineNumber: context.Start.Line,
            nextStatement: nextStatement,
            identifier: identifierNode,
            expression: expressionNode
        );
    }

    public override IEnumerable<TypeAndIdentifier> VisitFormalParameterList([NotNull] AQLParser.FormalParameterListContext context)
    {
        List<TypeAndIdentifier> parameters = [];

        AQLParser.IdentifierContext[] identifierContexts = context.identifier();
        int index = 0;
        foreach (AQLParser.TypeContext typeContext in context.type())
        {
            TypeNode typeNode = VisitType(typeContext);
            SingleIdentifierNode identifierNode = VisitIdentifier(identifierContexts[index]);

            TypeAndIdentifier typeAndIdentifier = new(
                lineNumber: context.Start.Line,
                type: typeNode,
                identifier: identifierNode
            );

            parameters.Add(typeAndIdentifier);
            index++;
        }

        return parameters;
    }

    public override TypeNode VisitType([NotNull] AQLParser.TypeContext context)
    {
        if (context.typeKeyword() != null)
        {
            return VisitTypeKeyword(context.typeKeyword());
        }
        else if (context.arrayType() != null)
        {
            return VisitArrayType(context.arrayType());
        }
        else
        {
            throw new("Not a valid type.");
        }
    }

    public override TypeNode VisitTypeKeyword([NotNull] AQLParser.TypeKeywordContext context)
    {
        if (context.boolKeyword() != null)
        {
            return VisitBoolKeyword(context.boolKeyword());
        }
        else if (context.intKeyword() != null)
        {
            return VisitIntKeyword(context.intKeyword());
        }
        else if (context.doubleKeyword() != null)
        {
            return VisitDoubleKeyword(context.doubleKeyword());
        }
        else if (context.stringKeyword() != null)
        {
            return VisitStringKeyword(context.stringKeyword());
        }
        else
        {
            throw new("Not a valid type.");
        }
    }

    #region Types
    public override BoolTypeNode VisitBoolKeyword([NotNull] AQLParser.BoolKeywordContext context)
    {
        return new(lineNumber: context.Start.Line);
    }

    public override IntTypeNode VisitIntKeyword([NotNull] AQLParser.IntKeywordContext context)
    {
        return new(lineNumber: context.Start.Line);
    }

    public override DoubleTypeNode VisitDoubleKeyword([NotNull] AQLParser.DoubleKeywordContext context)
    {
        return new(lineNumber: context.Start.Line);
    }

    public override StringTypeNode VisitStringKeyword([NotNull] AQLParser.StringKeywordContext context)
    {
        return new(lineNumber: context.Start.Line);
    }

    public override ArrayTypeNode VisitArrayType([NotNull] AQLParser.ArrayTypeContext context)
    {
        return new(
            lineNumber: context.Start.Line,
            innerType: VisitType(context.type())
        );
    }
    #endregion

    public override IEnumerable<SingleIdentifierNode> VisitIdList([NotNull] AQLParser.IdListContext context)
    {
        List<SingleIdentifierNode> identifiers = [];

        foreach (AQLParser.IdentifierContext identifierContext in context.identifier())
        {
            SingleIdentifierNode identifier = VisitIdentifier(identifierContext);
            identifiers.Add(identifier);
        }

        return identifiers;
    }

    public override List<RouteDefinitionNode> VisitRoutes([NotNull] AQLParser.RoutesContext context)
    {
        if (context.routesId() != null)
        {
            return VisitRoutesId(context.routesId());
        }
        else if (context.routesValue() != null)
        {
            return VisitRoutesValue(context.routesValue());
        }

        throw new($"Not a valid route definition! (Line {context.Start.Line})");
    }

    public override List<RouteDefinitionNode> VisitRoutesId([NotNull] AQLParser.RoutesIdContext context)
    {
        // Rule: routesId:
        //          qualifiedId '->' (routesId | qualifiedId | probabilityIdList);

        // We get all qualified Id's there are at most two.
        AQLParser.AnyIdentifierContext[] anyIdentifierContexts = context.anyIdentifier();
        IdentifierNode fromIdentifierNode = VisitAnyIdentifier(anyIdentifierContexts.First());
        IdentifierExpressionNode fromExpressionNode = new(fromIdentifierNode.LineNumber, fromIdentifierNode);

        // In the case that routesId is defined, we know the chain is longer than where we currently are.
        if (context.routesId() != null)
        {
            List<RouteDefinitionNode> routeNodes = VisitRoutesId(context.routesId());
            // We always add to the list, meaning the newest element will be last in the list.
            ExpressionNode routeTo = routeNodes.Last().From;

            if (routeTo is not IdentifierExpressionNode identifierExpressionNode)
            {
                throw new($"Not a valid route definition (Line: {routeTo.LineNumber})");
            }

            RouteDefinitionNode routeNode = MakeRouteDefinition(lineNumber: context.Start.Line, from: fromExpressionNode, to: identifierExpressionNode.Identifier);

            routeNodes.Add(routeNode);

            return routeNodes;
        }
        // There is always one identifier rule present, but at most two.
        // If there is two we know the rule is:
        //          qualifiedId '->' qualifiedId
        else if (anyIdentifierContexts.Length > 1)
        {
            IdentifierNode toIdentifierNode = VisitAnyIdentifier(anyIdentifierContexts[1]);
            return [
                MakeRouteDefinition(lineNumber: context.Start.Line, from: fromExpressionNode, to: toIdentifierNode)
            ];
        }
        // If the probabilityIdList exists we know the rule is:
        //          qualifiedId '->' probabilityIdList
        else if (context.probabilityIdList() != null)
        {
            IEnumerable<RouteValuePairNode> routeValuePairNodes = VisitProbabilityIdList(context.probabilityIdList());
            return [
                new(
                    lineNumber: context.Start.Line,
                    from: fromExpressionNode,
                    to: routeValuePairNodes
                )
            ];
        }
        else
        {
            throw new("Not a valid route.");
        }
    }

    public override List<RouteDefinitionNode> VisitRoutesValue([NotNull] AQLParser.RoutesValueContext context)
    {
        // Rule: routesValue:
        //          value '->' (routesId | qualifiedId | probabilityIdList);

        ExpressionNode value = VisitValue(context.value());

        // In the case that routesId is defined, we know the chain is longer than where we currently are.
        if (context.routesId() != null)
        {
            List<RouteDefinitionNode> routeNodes = VisitRoutesId(context.routesId());
            // We always add to the list, meaning the newest element will be last in the list.
            ExpressionNode routeTo = routeNodes.Last().From;

            if (routeTo is not IdentifierExpressionNode identifierExpressionNode)
            {
                throw new($"Not a valid route definition (Line: {routeTo.LineNumber})");
            }

            RouteDefinitionNode routeNode = MakeRouteDefinition(lineNumber: context.Start.Line, from: value, to: identifierExpressionNode.Identifier);

            routeNodes.Add(routeNode);

            return routeNodes;
        }
        // If there is two we know the rule is:
        //          value '->' qualifiedId
        else if (context.anyIdentifier() != null)
        {
            IdentifierNode toIdentifierNode = VisitAnyIdentifier(context.anyIdentifier());
            return [
                MakeRouteDefinition(lineNumber: context.Start.Line, from: value, to: toIdentifierNode)
            ];
        }
        // If the probabilityIdList exists we know the rule is:
        //          value '->' probabilityIdList
        else if (context.probabilityIdList() != null)
        {
            IEnumerable<RouteValuePairNode> routeValuePairNodes = VisitProbabilityIdList(context.probabilityIdList());
            return [
                new(
                    lineNumber: context.Start.Line,
                    from: value,
                    to: routeValuePairNodes
                )
            ];
        }
        else
        {
            throw new("Not a valid route.");
        }
    }

    private RouteDefinitionNode MakeRouteDefinition(int lineNumber, ExpressionNode from, IdentifierNode to)
    {
        return new(
            lineNumber: lineNumber,
            from: from,
            to: [
                new(
                    lineNumber: lineNumber,
                    probability: new IntLiteralNode(
                        lineNumber: lineNumber,
                        value: 1
                    ),
                    routeTo: to
                )
            ]
        );
    }


    public override IEnumerable<RouteValuePairNode> VisitProbabilityIdList([NotNull] AQLParser.ProbabilityIdListContext context)
    {
        AQLParser.ExpressionContext[] expressionContexts = context.expression();
        AQLParser.AnyIdentifierContext[] anyIdentifierContexts = context.anyIdentifier();

        List<RouteValuePairNode> routeValuePairNodes = [];

        int index = 0;
        foreach (AQLParser.ExpressionContext expressionContext in expressionContexts)
        {
            ExpressionNode probabilityNode = VisitExpression(expressionContext);
            IdentifierNode identifierNode = VisitAnyIdentifier(anyIdentifierContexts[index]);

            RouteValuePairNode routeValuePairNode = new(
                lineNumber: context.Start.Line,
                probability: probabilityNode,
                routeTo: identifierNode
            );

            routeValuePairNodes.Add(routeValuePairNode);

            index++;
        }

        return routeValuePairNodes;
    }

    public override IEnumerable<NamedMetricNode> VisitMetrics([NotNull] AQLParser.MetricsContext context)
    {
        List<NamedMetricNode> metrics = [];

        if (context.metric() != null)
        {
            foreach (AQLParser.MetricContext metricContext in context.metric())
            {
                NamedMetricNode metric = VisitMetric(metricContext);
                metrics.Add(metric);
            }
        }

        return metrics;
    }

    public override NamedMetricNode VisitMetric([NotNull] AQLParser.MetricContext context)
    {
        if (context.namedMetric() != null)
        {
            return VisitNamedMetric(context.namedMetric());
        }
        else
        {
            throw new("Not a valid metric.");
        }
    }

    public override NamedMetricNode VisitNamedMetric([NotNull] AQLParser.NamedMetricContext context)
    {
        return new(
            lineNumber: context.Start.Line,
            name: context.GetText()
        );
    }

    public override SimulateNode VisitSimulateDefinition([NotNull] AQLParser.SimulateDefinitionContext context)
    {
        IdentifierNode networkNode = VisitAnyIdentifier(context.network);
        ExpressionNode runsNode = VisitExpression(context.runs);
        ExpressionNode terminationCriteriaNode = VisitExpression(context.terminationCriteria);

        return new(
            lineNumber: context.Start.Line,
            networkIdentifier: networkNode,
            runs: runsNode,
            terminationCriteria: terminationCriteriaNode
        );
    }

    public override ExpressionNode VisitValue([NotNull] AQLParser.ValueContext context)
    {
        if (context.functionCall() != null)
        {
            return VisitFunctionCall(context.functionCall());
        }
        else if (context.anyIdentifier() != null)
        {
            return new IdentifierExpressionNode(
                lineNumber: context.Start.Line,
                identifier: VisitAnyIdentifier(context.anyIdentifier())
            );
        }
        else if (context.@string() != null)
        {
            return VisitString(context.@string());
        }
        else if (context.@int() != null)
        {
            return VisitInt(context.@int());
        }
        else if (context.@double() != null)
        {
            return VisitDouble(context.@double());
        }
        else if (context.@bool() != null)
        {
            return VisitBool(context.@bool());
        }
        else if (context.arrayInitialization() != null)
        {
            return VisitArrayInitialization(context.arrayInitialization());
        }
        else if (context.arrayIndexing() != null)
        {
            return VisitArrayIndexing(context.arrayIndexing());
        }
        else
        {
            throw new("Not a valid value.");
        }
    }

    public override FunctionCallNode VisitFunctionCall(AQLParser.FunctionCallContext context)
    {
        IdentifierNode functionIdentifierNode = VisitAnyIdentifier(context.functionIdentifier);

        IEnumerable<ExpressionNode>? parameters = null;
        if (context.parameters != null)
        {
            parameters = VisitExpressionList(context.parameters);
        }

        return new(
            lineNumber: context.Start.Line,
            identifier: functionIdentifierNode,
            actualParameters: parameters ?? []
        );
    }

    public override IdentifierNode VisitAnyIdentifier([NotNull] AQLParser.AnyIdentifierContext context)
    {
        if (context.identifier() != null)
        {
            return VisitIdentifier(context.identifier());
        }
        else if (context.qualifiedId() != null)
        {
            return VisitQualifiedId(context.qualifiedId());
        }

        throw new($"Not a valid identifier! (Line: {context.Start.Line})");
    }

    public override IEnumerable<ExpressionNode> VisitExpressionList([NotNull] AQLParser.ExpressionListContext context)
    {
        List<ExpressionNode> expressionNodes = [];
        foreach (AQLParser.ExpressionContext expressionContext in context.expression())
        {
            ExpressionNode expressionNode = VisitExpression(expressionContext);
            expressionNodes.Add(expressionNode);
        }

        return expressionNodes;
    }

    public override QualifiedIdentifierNode VisitQualifiedId([NotNull] AQLParser.QualifiedIdContext context)
    {

        SingleIdentifierNode leftNode = VisitIdentifier(context.left);
        SingleIdentifierNode rightNode = VisitIdentifier(context.right);

        return new(
            lineNumber: context.Start.Line,
            leftIdentifier: leftNode,
            rightIdentifier: rightNode
        );

    }

    public override IndexingNode VisitArrayIndexing([NotNull] AQLParser.ArrayIndexingContext context)
    {
        SingleIdentifierNode targetNode = VisitIdentifier(context.target);
        ExpressionNode indexNode = VisitExpression(context.index);

        return new(
            lineNumber: context.Start.Line,
            target: targetNode,
            index: indexNode
        );
    }

    #region Expressions

    public class BinaryOperatorReturn(BinaryOperator binaryOperator)
    {
        public BinaryOperator BinaryOperator { get; } = binaryOperator;
    }

    public enum BinaryOperator
    {
        and = 0,
        or = 1,
        add = 2,
        subtract = 3,
        division = 4,
        equal = 5,
        inEqual = 6,
        greaterThan = 7,
        greaterThanOrEqual = 8,
        lessThan = 9,
        lessThanOrEqual = 10,
        multiplication = 11,
    }

    public override ExpressionNode VisitExpression([NotNull] AQLParser.ExpressionContext context)
    {
        int ln = context.Start.Line;
        if (context.unaryExpression() != null)
        {
            return VisitUnaryExpression(context.unaryExpression());
        }
        else if (context.binaryOperator() != null)
        {
            ExpressionNode leftExpressionNode = VisitExpression(context.left);
            ExpressionNode rightExpressionNode = VisitExpression(context.right);
            BinaryOperatorReturn binaryOperator = VisitBinaryOperator(context.binaryOperator());
            return binaryOperator switch
            {
                { BinaryOperator: BinaryOperator.and } => new AndNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode),
                // This is done by using De Morgans Law      P OR Q = NOT( (NOT P) AND (NOT Q) )
                { BinaryOperator: BinaryOperator.or } => new NotNode(
                    lineNumber: ln,
                    inner: new AndNode(
                        lineNumber: ln,
                        left: new NotNode(lineNumber: ln, inner: leftExpressionNode),
                        right: new NotNode(lineNumber: ln, inner: rightExpressionNode)
                    )
                ),
                { BinaryOperator: BinaryOperator.add } => new AddNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode),
                // This is done by adding the negative value of the right expression
                { BinaryOperator: BinaryOperator.subtract } => new AddNode(
                    lineNumber: ln,
                    left: leftExpressionNode,
                    right: new NegativeNode(lineNumber: ln, inner: rightExpressionNode)
                ),
                { BinaryOperator: BinaryOperator.multiplication } => new MultiplyNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode),
                { BinaryOperator: BinaryOperator.division } => new DivisionNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode),
                { BinaryOperator: BinaryOperator.equal } => new EqualNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode),
                // This is done by doing:       P IN-EQUAL Q = NOT(P EQUAL Q)
                { BinaryOperator: BinaryOperator.inEqual } => new NotNode(
                    lineNumber: ln,
                    inner: new EqualNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode)
                ),
                // This is done by doing  NOT( (L LESSTHAN R) OR (L EQUAL R) )
                // Less thanOrEqual is made bellow so just taking the not of that leaves us with:
                // NOT(L LESSTHAN R) AND NOT(L EQUAL R)         
                { BinaryOperator: BinaryOperator.greaterThan } => new AndNode(
                    lineNumber: ln,
                    left: new NotNode(
                        lineNumber: ln,
                        inner: new LessThanNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode)
                    ),
                    right: new NotNode(
                        lineNumber: ln,
                        inner: new EqualNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode)
                    )
                ),
                // This is done by doing NOT(L LESSTHAN R)
                { BinaryOperator: BinaryOperator.greaterThanOrEqual } => new NotNode(
                    lineNumber: ln,
                    inner: new LessThanNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode)
                ),
                { BinaryOperator: BinaryOperator.lessThan } => new LessThanNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode),
                // This is done by doing
                // P = L LESSTHAN R
                // Q = L EQUAL R
                // Then Then by using De Morgans Law      P OR Q = NOT( (NOT P) AND (NOT Q) ) 
                // NOT( NOT(L LESSTHAN R) AND NOT(L EQUAL R) )
                { BinaryOperator: BinaryOperator.lessThanOrEqual } => new NotNode(
                    lineNumber: ln,
                    inner: new AndNode(
                        lineNumber: ln,
                        left: new NotNode(
                            lineNumber: ln,
                            inner: new LessThanNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode)
                        ),
                        right: new NotNode(
                            lineNumber: ln,
                            inner: new EqualNode(lineNumber: ln, left: leftExpressionNode, right: rightExpressionNode)
                        )
                    )
                ),
                _ => throw new($"Not a valid operator! (Line: {ln})"),
            };
        }

        throw new($"Not a valid operator! (Line: {ln})");
    }

    public override ExpressionNode VisitUnaryExpression([NotNull] AQLParser.UnaryExpressionContext context)
    {
        if (context.negationExpression() != null)
        {
            return VisitNegationExpression(context.negationExpression());
        }
        else if (context.negativeExpression() != null)
        {
            return VisitNegativeExpression(context.negativeExpression());
        }
        else if (context.parenthesesExpression() != null)
        {
            return VisitParenthesesExpression(context.parenthesesExpression());
        }
        else if (context.value() != null)
        {
            return VisitValue(context.value());
        }
        else
        {
            throw new($"Not a valid expression. (Line: {context.Start.Line})");
        }
    }

    public override NotNode VisitNegationExpression([NotNull] AQLParser.NegationExpressionContext context)
    {
        ExpressionNode innerNode = VisitExpression(context.expression());

        return new(
            lineNumber: context.Start.Line,
            inner: innerNode
        );
    }

    public override NegativeNode VisitNegativeExpression([NotNull] AQLParser.NegativeExpressionContext context)
    {
        ExpressionNode innerNode = VisitExpression(context.expression());

        return new(
            lineNumber: context.Start.Line,
            inner: innerNode
        );
    }

    public override ParenthesesNode VisitParenthesesExpression([NotNull] AQLParser.ParenthesesExpressionContext context)
    {
        ExpressionNode innerNode = VisitExpression(context.expression());

        return new(
            lineNumber: context.Start.Line,
            inner: innerNode
        );
    }

    public override BinaryOperatorReturn VisitBinaryOperator([NotNull] AQLParser.BinaryOperatorContext context)
    {
        if (context.andOperator() != null) return new(BinaryOperator.and);
        else if (context.orOperator() != null) return new(BinaryOperator.or);
        else if (context.addOperator() != null) return new(BinaryOperator.add);
        else if (context.subtractOperator() != null) return new(BinaryOperator.subtract);
        else if (context.multiplicationOperator() != null) return new(BinaryOperator.multiplication);
        else if (context.divisionOperator() != null) return new(BinaryOperator.division);
        else if (context.equalOperator() != null) return new(BinaryOperator.equal);
        else if (context.inEqualOperator() != null) return new(BinaryOperator.inEqual);
        else if (context.greaterThanOperator() != null) return new(BinaryOperator.greaterThan);
        else if (context.greaterThanOrEqualOperator() != null) return new(BinaryOperator.greaterThanOrEqual);
        else if (context.lessThanOperator() != null) return new(BinaryOperator.lessThan);
        else if (context.lessThanOrEqualOperator() != null) return new(BinaryOperator.lessThanOrEqual);

        throw new($"Not a valid operator. (Line: {context.Start.Line})");
    }

    #endregion

    #region Literals
    public override SingleIdentifierNode VisitIdentifier([NotNull] AQLParser.IdentifierContext context)
    {
        return new(
            lineNumber: context.Start.Line,
            identifier: context.IDENTIFIER().GetText()
        );
    }

    public override BoolLiteralNode VisitBool([NotNull] AQLParser.BoolContext context)
    {
        return new(
            lineNumber: context.Start.Line,
            value: context.BOOL().GetText() == "true"
        );
    }

    public override IntLiteralNode VisitInt([NotNull] AQLParser.IntContext context)
    {
        return new(
            lineNumber: context.Start.Line,
            value: int.Parse(context.INT().GetText())
        );
    }

    public override DoubleLiteralNode VisitDouble([NotNull] AQLParser.DoubleContext context)
    {
        return new(
            lineNumber: context.Start.Line,
            value: double.Parse(context.DOUBLE().GetText())
        );
    }

    public override StringLiteralNode VisitString([NotNull] AQLParser.StringContext context)
    {
        return new(
            lineNumber: context.Start.Line,
            value: context.STRING().GetText()[1..^1] // Remove quotes
        );
    }

    public override ArrayLiteralNode VisitArrayInitialization([NotNull] AQLParser.ArrayInitializationContext context)
    {
        List<ExpressionNode> elements = [];
        foreach (AQLParser.ExpressionContext expressionContext in context.expression())
        {
            ExpressionNode element = VisitExpression(expressionContext);
            elements.Add(element);
        }

        return new(
            lineNumber: context.Start.Line,
            elements: elements
        );
    }
    #endregion


}