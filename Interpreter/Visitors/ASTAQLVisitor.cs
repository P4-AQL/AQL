


using Antlr4.Runtime.Misc;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Metrics;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Statements;
using Interpreter.AST.Nodes.Types;
using Interpreter.AST.NonNodes;

namespace Interpreter.Visitors;
class ASTAQLVisitor : AQLBaseVisitor<object>
{
    public override ProgramNode VisitProgram([NotNull] AQLParser.ProgramContext context)
    {
        ProgramNode programNode;
        if (context.importStatement() != null)
        {
            programNode = VisitImportStatement(context.importStatement());
        }
        else if (context.definition() != null)
        {
            DefinitionNode definitionNode = VisitDefinition(context.definition());
            programNode = new DefinitionProgramNode(
                definition: definitionNode
            );
        }
        else
        {
            programNode = new();
        }

        return programNode;
    }

    public override ImportNode VisitImportStatement([NotNull] AQLParser.ImportStatementContext context)
    {
        string moduleName = context.@string().GetText();
        moduleName = moduleName[1..^1]; // Remove quotes

        StringLiteralNode moduleNameNode = new(moduleName);
        ProgramNode programNode = VisitProgram(context.program());

        return new(moduleNameNode, programNode);
    }

    public override DefinitionNode VisitDefinition([NotNull] AQLParser.DefinitionContext context)
    {
        if (context.baseDefinition() != null)
        {
            return VisitBaseDefinition(context.baseDefinition());
        }
        else if (context.definitionComposition() != null)
        {
            return VisitDefinitionComposition(context.definitionComposition());
        }
        else
        {
            throw new("Not a valid definition.");
        }
    }

    public override DefinitionCompositionNode VisitDefinitionComposition([NotNull] AQLParser.DefinitionCompositionContext context)
    {
        DefinitionNode leftNode = VisitBaseDefinition(context.left);
        DefinitionNode rightNode = VisitDefinition(context.right);

        return new(
            left: leftNode,
            right: rightNode
        );
    }

    public override DefinitionNode VisitBaseDefinition([NotNull] AQLParser.BaseDefinitionContext context)
    {
        if (context.functionDefinition() != null)
        {
            return VisitFunctionDefinition(context.functionDefinition());
        }
        else if (context.constDefinition() != null)
        {
            return VisitConstDefinition(context.constDefinition());
        }
        else if (context.networks() != null)
        {
            return VisitNetworks(context.networks());
        }
        else if (context.simulateDefinition() != null)
        {
            return VisitSimulateDefinition(context.simulateDefinition());
        }
        else
        {
            throw new("Not a valid definition.");
        }
    }

    public override FunctionNode VisitFunctionDefinition([NotNull] AQLParser.FunctionDefinitionContext context)
    {
        TypeNode returnTypeNode = VisitType(context.returnType);
        IdentifierNode identifierNode = VisitIdentifier(context.identifier());
        IEnumerable<TypeAndIdentifier> parameterNodes = VisitFormalParameterList(context.formalParameterList());
        StatementNode statementNode = VisitStatement(context.statement());

        return new(
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

        return new(
            type: typeNode,
            identifier: assignNode.Identifier,
            expression: assignNode.Expression
        );
    }

    public override StatementNode VisitStatement([NotNull] AQLParser.StatementContext context)
    {
        if (context.statementComposition() != null)
        {
            return VisitStatementComposition(context.statementComposition());
        }
        else if (context.baseStatement() != null)
        {
            return VisitBaseStatement(context.baseStatement());
        }
        else
        {
            throw new("Not a valid statement.");
        }
    }

    public override StatementCompositionNode VisitStatementComposition([NotNull] AQLParser.StatementCompositionContext context)
    {
        StatementNode leftNode = VisitBaseStatement(context.left);
        StatementNode rightNode = VisitStatement(context.right);

        return new(
            left: leftNode,
            right: rightNode
        );
    }

    public override StatementNode VisitBaseStatement([NotNull] AQLParser.BaseStatementContext context)
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

        return new(
            condition: conditionNode,
            body: bodyNode
        );
    }

    public override StatementNode VisitBlock([NotNull] AQLParser.BlockContext context)
    {
        return VisitStatement(context.statement());
    }

    public override StatementCompositionNode VisitVariableDeclarationStatement([NotNull] AQLParser.VariableDeclarationStatementContext context)
    {
        TypeNode typeNode = VisitType(context.type());
        AssignNode assignNode = VisitAssignStatement(context.assignStatement());

        return new(
            left: new VariableDeclarationNode(
                type: typeNode,
                identifier: assignNode.Identifier
            ),
            right: assignNode
        );
    }

    public override IfElseNode VisitIfStatement([NotNull] AQLParser.IfStatementContext context)
    {
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
            elseBody = new SkipNode();
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
                condition: firstElseIfReturn.Condition,
                ifBody: firstElseIfReturn.Body,
                elseBody: elseBody
            );
            foreach (AQLParser.ElseIfStatementContext elseIfStatementContext in elseIfStatementContexts.Skip(1))
            {
                ElseIfReturn elseIfReturn = VisitElseIfStatement(elseIfStatementContext);
                currentNode = new(
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
                condition: mainIfCondition,
                ifBody: mainIfBody,
                elseBody: elseBody
            );
        }
        else
        {
            currentNode = new(
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

        return new(
            network: networkNode
        );
    }

    public override QueueDeclarationNode VisitQueueDefinition([NotNull] AQLParser.QueueDefinitionContext context)
    {
        IdentifierNode identifierNode = VisitIdentifier(context.identifier());
        ExpressionNode serviceNode = VisitExpression(context.service);
        ExpressionNode capacityNode = VisitExpression(context.capacity);
        ExpressionNode numberOfServersNode = VisitExpression(context.numberOfServers);
        IEnumerable<MetricNode> metricNodes = VisitMetrics(context.metrics());

        return new(
            identifierNode,
            serviceNode,
            capacityNode,
            numberOfServersNode,
            metricNodes
        );
    }

    public override NetworkDeclarationNode VisitNetworkDefinition([NotNull] AQLParser.NetworkDefinitionContext context)
    {
        IdentifierNode identifierNode = VisitIdentifier(context.identifier());
        IEnumerable<IdentifierNode> inputNodes = VisitIdList(context.inputs);
        IEnumerable<IdentifierNode> outputNodes = VisitIdList(context.outputs);
        IEnumerable<InstanceDeclaration> instanceNodes = VisitInstances(context.instances());
        IEnumerable<RouteNode> routeNodes = VisitRoutes(context.routes());
        IEnumerable<MetricNode> metricNodes = VisitMetrics(context.metrics());

        return new(
            identifier: identifierNode,
            inputs: inputNodes,
            outputs: outputNodes,
            instances: instanceNodes,
            routes: routeNodes,
            metrics: metricNodes
        );
    }

    public override AssignNode VisitAssignStatement([NotNull] AQLParser.AssignStatementContext context)
    {
        IdentifierNode identifierNode = VisitIdentifier(context.identifier());
        ExpressionNode expressionNode = VisitExpression(context.expression());

        return new(
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
            IdentifierNode identifierNode = VisitIdentifier(identifierContexts[index]);

            TypeAndIdentifier typeAndIdentifier = new(
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
        return new();
    }

    public override IntTypeNode VisitIntKeyword([NotNull] AQLParser.IntKeywordContext context)
    {
        return new();
    }

    public override DoubleTypeNode VisitDoubleKeyword([NotNull] AQLParser.DoubleKeywordContext context)
    {
        return new();
    }

    public override StringTypeNode VisitStringKeyword([NotNull] AQLParser.StringKeywordContext context)
    {
        return new();
    }

    public override ArrayTypeNode VisitArrayType([NotNull] AQLParser.ArrayTypeContext context)
    {
        return new(
            innerType: VisitType(context.type())
        );
    }
    #endregion

    public override IEnumerable<IdentifierNode> VisitIdList([NotNull] AQLParser.IdListContext context)
    {
        List<IdentifierNode> identifiers = [];

        foreach (AQLParser.IdentifierContext identifierContext in context.identifier())
        {
            IdentifierNode identifier = VisitIdentifier(identifierContext);
            identifiers.Add(identifier);
        }

        return identifiers;
    }

    public override IEnumerable<InstanceDeclaration> VisitInstances([NotNull] AQLParser.InstancesContext context)
    {
        List<InstanceDeclaration> instanceDeclarations = [];
        foreach (AQLParser.InstanceContext instanceContext in context.instance())
        {
            InstanceDeclaration instanceDeclaration = VisitInstance(instanceContext);
            instanceDeclarations.Add(instanceDeclaration);
        }

        return instanceDeclarations;
    }

    public override InstanceDeclaration VisitInstance([NotNull] AQLParser.InstanceContext context)
    {
        QualifiedIdentifierNode existingInstanceNode = VisitQualifiedId(context.existing);
        IEnumerable<IdentifierNode> newInstanceNodes = VisitIdList(context.@new);

        return new(
            existingInstance: existingInstanceNode,
            newInstances: newInstanceNodes
        );
    }

    public override IEnumerable<RouteNode> VisitRoutes([NotNull] AQLParser.RoutesContext context)
    {
        AQLParser.IdentifierContext[] routeContexts = context.identifier();

        List<RouteNode> routes = [];
        for (int i = 0; i < routeContexts.Length - 2; i += 1)
        {
            IdentifierNode from = VisitIdentifier(routeContexts[i]);
            IdentifierNode to = VisitIdentifier(routeContexts[i + 1]);

            RouteNode routeNode = new(from, to);
            routes.Add(routeNode);
        }

        return routes;
    }

    public override IEnumerable<MetricNode> VisitMetrics([NotNull] AQLParser.MetricsContext context)
    {
        List<MetricNode> metrics = [];

        foreach (AQLParser.MetricContext metricContext in context.metric())
        {
            MetricNode metric = VisitMetric(metricContext);
            metrics.Add(metric);
        }

        return metrics;
    }

    public override MetricNode VisitMetric([NotNull] AQLParser.MetricContext context)
    {
        if (context.namedMetric() != null)
        {
            return VisitNamedMetric(context.namedMetric());
        }
        else if (context.functionMetric != null)
        {
            return new FunctionMetricNode(
                functionCall: VisitFunctionCall(context.functionMetric)
            );
        }
        else
        {
            throw new("Not a valid metric.");
        }
    }

    public override NamedMetricNode VisitNamedMetric([NotNull] AQLParser.NamedMetricContext context)
    {
        return new(
            name: context.GetText()
        );
    }

    public override SimulateNode VisitSimulateDefinition([NotNull] AQLParser.SimulateDefinitionContext context)
    {
        QualifiedIdentifierNode networkNode = VisitQualifiedId(context.network);
        ExpressionNode runsNode = VisitExpression(context.runs);
        ExpressionNode terminationCriteriaNode = VisitExpression(context.terminationCriteria);

        return new(
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
        else if (context.qualifiedId() != null)
        {
            return VisitQualifiedId(context.qualifiedId());
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
        ExpressionNode functionIdentifierNode = VisitQualifiedId(context.functionIdentifier);

        IEnumerable<ExpressionNode>? parameters = null;
        if (context.parameters != null)
        {
            parameters = VisitExpressionList(context.parameters);
        }

        return new(
            identifier: functionIdentifierNode,
            actualParameters: parameters ?? []
        );
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

    public override IEnumerable<QualifiedIdentifierNode> VisitQualifiedIdList([NotNull] AQLParser.QualifiedIdListContext context)
    {
        List<QualifiedIdentifierNode> qualifiedIdentifiers = [];

        foreach (AQLParser.QualifiedIdContext qualifiedIdContext in context.qualifiedId())
        {
            QualifiedIdentifierNode qualifiedIdentifier = VisitQualifiedId(qualifiedIdContext);
            qualifiedIdentifiers.Add(qualifiedIdentifier);
        }

        return qualifiedIdentifiers;
    }

    public override QualifiedIdentifierNode VisitQualifiedId([NotNull] AQLParser.QualifiedIdContext context)
    {
        List<AQLParser.IdentifierContext> identifiers = [.. context.identifier()];
        identifiers.Reverse();

        ExpressionNode current = VisitIdentifier(identifiers.First());
        foreach (AQLParser.IdentifierContext identifierContext in identifiers.Skip(1))
        {
            IdentifierNode newIdentifierNode = VisitIdentifier(identifierContext);
            current = new QualifiedIdentifierNode(
                identifier: newIdentifierNode,
                expression: current
            );
        }

        if (current is not QualifiedIdentifierNode qualifiedIdentifierNode)
        {
            throw new("Not a valid identifier.");
        }
        return qualifiedIdentifierNode;
    }

    public override IndexingNode VisitArrayIndexing([NotNull] AQLParser.ArrayIndexingContext context)
    {
        ExpressionNode targetNode = VisitQualifiedId(context.target);
        ExpressionNode indexNode = VisitExpression(context.index);

        return new(
            target: targetNode,
            index: indexNode
        );
    }

    #region Expressions
    public override ExpressionNode VisitExpression([NotNull] AQLParser.ExpressionContext context)
    {
        return VisitLogicalOrExpression(context.logicalOrExpression());
    }

    public override ExpressionNode VisitLogicalOrExpression([NotNull] AQLParser.LogicalOrExpressionContext context)
    {
        List<AQLParser.LogicalAndExpressionContext> logicalAndExpressionContexts = [.. context.logicalAndExpression()];
        if (logicalAndExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitLogicalAndExpression(logicalAndExpressionContexts[0]);
            if (logicalAndExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.LogicalAndExpressionContext logicalAndExpressionContext in logicalAndExpressionContexts.Skip(1))
                {
                    ExpressionNode logicalAndNode = VisitLogicalAndExpression(logicalAndExpressionContext);
                    current = MakeOrUsingDeMorganSubstitution(current, logicalAndNode);
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    private static NotNode MakeOrUsingDeMorganSubstitution(ExpressionNode left, ExpressionNode right)
    {
        return new NotNode(
            inner: new AndNode(
                left: new NotNode(
                    inner: left
                ),
                right: new NotNode(
                    inner: right
                )
            )
        );
    }

    public override ExpressionNode VisitLogicalAndExpression([NotNull] AQLParser.LogicalAndExpressionContext context)
    {
        List<AQLParser.EqualityExpressionContext> equalityExpressionContexts = [.. context.equalityExpression()];
        if (equalityExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitEqualityExpression(equalityExpressionContexts[0]);
            if (equalityExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.EqualityExpressionContext equalExpressionContext in equalityExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitEqualityExpression(equalExpressionContext);
                    current = new AndNode(
                        left: current,
                        right: equalityNode
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitEqualityExpression([NotNull] AQLParser.EqualityExpressionContext context)
    {
        if (context.equalExpression() != null)
        {
            return VisitEqualExpression(context.equalExpression());
        }
        else if (context.inEqualExpression() != null)
        {
            return VisitInEqualExpression(context.inEqualExpression());
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitEqualExpression([NotNull] AQLParser.EqualExpressionContext context)
    {
        List<AQLParser.RelationalExpressionContext> relationalExpressionContexts = [.. context.relationalExpression()];
        if (relationalExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitRelationalExpression(relationalExpressionContexts[0]);
            if (relationalExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.RelationalExpressionContext relationalContext in relationalExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitRelationalExpression(relationalContext);
                    current = new EqualNode(
                        left: current,
                        right: equalityNode
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitInEqualExpression([NotNull] AQLParser.InEqualExpressionContext context)
    {
        List<AQLParser.RelationalExpressionContext> relationalExpressionContexts = [.. context.relationalExpression()];
        if (relationalExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitRelationalExpression(relationalExpressionContexts[0]);
            if (relationalExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.RelationalExpressionContext relationalExpressionContext in relationalExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitRelationalExpression(relationalExpressionContext);
                    current = new NotNode(
                        inner: new EqualNode(
                            left: current,
                            right: equalityNode
                        )
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitRelationalExpression([NotNull] AQLParser.RelationalExpressionContext context)
    {
        if (context.lessThanExpression() != null)
        {
            return VisitLessThanExpression(context.lessThanExpression());
        }
        else if (context.lessThanOrEqualExpression() != null)
        {
            return VisitLessThanOrEqualExpression(context.lessThanOrEqualExpression());
        }
        else if (context.greaterThanExpression() != null)
        {
            return VisitGreaterThanExpression(context.greaterThanExpression());
        }
        else if (context.greaterThanOrEqualExpression() != null)
        {
            return VisitGreaterThanOrEqualExpression(context.greaterThanOrEqualExpression());
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitLessThanExpression([NotNull] AQLParser.LessThanExpressionContext context)
    {
        List<AQLParser.AdditiveExpressionContext> additiveExpressionContexts = [.. context.additiveExpression()];
        if (additiveExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitAdditiveExpression(additiveExpressionContexts[0]);
            if (additiveExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.AdditiveExpressionContext additiveExpressionContext in additiveExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitAdditiveExpression(additiveExpressionContext);
                    current = new LessThanNode(
                        left: current,
                        right: equalityNode
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitLessThanOrEqualExpression([NotNull] AQLParser.LessThanOrEqualExpressionContext context)
    {
        List<AQLParser.AdditiveExpressionContext> additiveExpressionContexts = [.. context.additiveExpression()];
        if (additiveExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitAdditiveExpression(additiveExpressionContexts[0]);
            if (additiveExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.AdditiveExpressionContext additiveExpressionContext in additiveExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitAdditiveExpression(additiveExpressionContext);
                    current = MakeLessThanOrEqualNode(current, equalityNode);
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    private static LessThanNode MakeLessThanOrEqualNode(ExpressionNode left, ExpressionNode right)
    {
        return new LessThanNode(
            left: new AddNode(
                left: left,
                right: new NegativeNode(
                    inner: new IntLiteralNode(
                        value: 1
                    )
                )
            ),
            right: right
        );
    }

    public override ExpressionNode VisitGreaterThanExpression([NotNull] AQLParser.GreaterThanExpressionContext context)
    {
        List<AQLParser.AdditiveExpressionContext> additiveExpressionContexts = [.. context.additiveExpression()];
        if (additiveExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitAdditiveExpression(additiveExpressionContexts[0]);
            if (additiveExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.AdditiveExpressionContext additiveExpressionContext in additiveExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitAdditiveExpression(additiveExpressionContext);
                    LessThanNode lessThanOrEqualNode = MakeLessThanOrEqualNode(current, equalityNode);
                    current = new NotNode(
                        inner: lessThanOrEqualNode
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitGreaterThanOrEqualExpression([NotNull] AQLParser.GreaterThanOrEqualExpressionContext context)
    {
        List<AQLParser.AdditiveExpressionContext> additiveExpressionContexts = [.. context.additiveExpression()];
        if (additiveExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitAdditiveExpression(additiveExpressionContexts[0]);
            if (additiveExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.AdditiveExpressionContext additiveExpressionContext in additiveExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitAdditiveExpression(additiveExpressionContext);
                    current = new NotNode(
                        inner: new LessThanNode(
                            left: current,
                            right: equalityNode
                        )
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitAdditiveExpression([NotNull] AQLParser.AdditiveExpressionContext context)
    {
        if (context.addExpression() != null)
        {
            return VisitAddExpression(context.addExpression());
        }
        else if (context.subtractExpression() != null)
        {
            return VisitSubtractExpression(context.subtractExpression());
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitAddExpression([NotNull] AQLParser.AddExpressionContext context)
    {
        List<AQLParser.MultiplicativeExpressionContext> multiplicativeExpressionContexts = [.. context.multiplicativeExpression()];
        if (multiplicativeExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitMultiplicativeExpression(multiplicativeExpressionContexts[0]);
            if (multiplicativeExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.MultiplicativeExpressionContext multiplicativeExpressionContext in multiplicativeExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitMultiplicativeExpression(multiplicativeExpressionContext);
                    current = new AddNode(
                        left: current,
                        right: equalityNode
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitSubtractExpression([NotNull] AQLParser.SubtractExpressionContext context)
    {
        List<AQLParser.MultiplicativeExpressionContext> multiplicativeExpressionContexts = [.. context.multiplicativeExpression()];
        if (multiplicativeExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitMultiplicativeExpression(multiplicativeExpressionContexts[0]);
            if (multiplicativeExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.MultiplicativeExpressionContext multiplicativeExpressionContext in multiplicativeExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitMultiplicativeExpression(multiplicativeExpressionContext);
                    current = new AddNode(
                        left: current,
                        right: new NegativeNode(
                            inner: equalityNode
                        )
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitMultiplicativeExpression([NotNull] AQLParser.MultiplicativeExpressionContext context)
    {
        if (context.multiplyExpression() != null)
        {
            return VisitMultiplyExpression(context.multiplyExpression());
        }
        else if (context.divisionExpression() != null)
        {
            return VisitDivisionExpression(context.divisionExpression());
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitMultiplyExpression([NotNull] AQLParser.MultiplyExpressionContext context)
    {
        List<AQLParser.UnaryExpressionContext> unaryExpressionContexts = [.. context.unaryExpression()];
        if (unaryExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitUnaryExpression(unaryExpressionContexts[0]);
            if (unaryExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.UnaryExpressionContext unaryExpressionContext in unaryExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitUnaryExpression(unaryExpressionContext);
                    current = new MultiplyNode(
                        left: current,
                        right: equalityNode
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
    }

    public override ExpressionNode VisitDivisionExpression([NotNull] AQLParser.DivisionExpressionContext context)
    {
        List<AQLParser.UnaryExpressionContext> unaryExpressionContexts = [.. context.unaryExpression()];
        if (unaryExpressionContexts.Count > 0)
        {
            ExpressionNode current = VisitUnaryExpression(unaryExpressionContexts[0]);
            if (unaryExpressionContexts.Count == 1)
            {
                return current;
            }
            else
            {
                foreach (AQLParser.UnaryExpressionContext unaryExpressionContext in unaryExpressionContexts.Skip(1))
                {
                    ExpressionNode equalityNode = VisitUnaryExpression(unaryExpressionContext);
                    current = new DivisionNode(
                        left: current,
                        right: equalityNode
                    );
                }

                return current;
            }
        }
        else
        {
            throw new("Not a valid expression.");
        }
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
            throw new("Not a valid expression.");
        }
    }

    public override NotNode VisitNegationExpression([NotNull] AQLParser.NegationExpressionContext context)
    {
        ExpressionNode innerNode = VisitExpression(context.expression());

        return new(
            inner: innerNode
        );
    }

    public override NegativeNode VisitNegativeExpression([NotNull] AQLParser.NegativeExpressionContext context)
    {
        ExpressionNode innerNode = VisitExpression(context.expression());

        return new(
            inner: innerNode
        );
    }

    public override ParenthesesNode VisitParenthesesExpression([NotNull] AQLParser.ParenthesesExpressionContext context)
    {
        ExpressionNode innerNode = VisitExpression(context.expression());

        return new(
            inner: innerNode
        );
    }

    #endregion

    #region Literals
    public override IdentifierNode VisitIdentifier([NotNull] AQLParser.IdentifierContext context)
    {
        return new(
            identifier: context.IDENTIFIER().GetText()
        );
    }

    public override BoolLiteralNode VisitBool([NotNull] AQLParser.BoolContext context)
    {
        return new(
            value: context.BOOL().GetText() == "true"
        );
    }

    public override IntLiteralNode VisitInt([NotNull] AQLParser.IntContext context)
    {
        return new(
            value: int.Parse(context.INT().GetText())
        );
    }

    public override DoubleLiteralNode VisitDouble([NotNull] AQLParser.DoubleContext context)
    {
        return new(
            value: double.Parse(context.DOUBLE().GetText())
        );
    }

    public override StringLiteralNode VisitString([NotNull] AQLParser.StringContext context)
    {
        return new(
            value: context.STRING().GetText()[1..^1] // Remove quotes
        );
    }

    public override ArrayLiteralNode VisitArrayInitialization([NotNull] AQLParser.ArrayInitializationContext context)
    {
        List<ExpressionNode> elements = [];
        foreach (AQLParser.ValueContext valueContext in context.value())
        {
            ExpressionNode element = VisitValue(valueContext);
            elements.Add(element);
        }

        return new(
            elements: elements
        );
    }
    #endregion


}