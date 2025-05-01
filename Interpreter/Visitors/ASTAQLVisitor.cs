


using Antlr4.Runtime.Misc;
using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Statements;

namespace Interpreter.Visitors;
class ASTAQLVisitor : AQLBaseVisitor<object>
{
    public override ProgramNode VisitProgram([NotNull] AQLParser.ProgramContext context)
    {
        var importStatementContext = context.importStatement();

        if (importStatementContext != null)
        {
            return VisitImportStatement(importStatementContext);
        }

        else if (context.definition().Length > 0)
        {
            List<DefinitionNode> definitions = [];
            foreach (AQLParser.DefinitionContext definitionContext in context.definition())
            {
                object? definition = Visit(definitionContext);
                if (definition is not DefinitionNode definitionNode)
                {
                    throw new Exception("Expected DefinitionNode.");
                }
                definitions.Add(definitionNode);
            }
            return new DefinitionGroupNode(definitions);
        }

        else
        {
            return new();
        }
    }

    public override ImportNode VisitImportStatement([NotNull] AQLParser.ImportStatementContext context)
    {
        string moduleName = context.@string().GetText();
        moduleName = moduleName[1..^1]; // Remove quotes

        StringLiteralNode moduleNameNode = new(moduleName);
        ProgramNode programNode = VisitProgram(context.program());

        return new(moduleNameNode, programNode);
    }

    public override ConstDeclarationNode VisitConstDefinition([NotNull] AQLParser.ConstDefinitionContext context)
    {
        object type = Visit(context.type());
        object assign = Visit(context.assign());

        if (type is not TypeNode typeNode)
        {
            throw new Exception("Expected TypeNode.");
        }
        if (assign is not AssignNode assignNode)
        {
            throw new Exception("Expected AssignmentNode.");
        }

        return new(
            type: typeNode,
            identifier: assignNode.Identifier,
            expression: assignNode.Expression
        );
    }

    public override FunctionNode VisitFunctionDefinition([NotNull] AQLParser.FunctionDefinitionContext context)
    {
        object returnType = Visit(context.returnType);
        IdentifierNode identifier = VisitIdentifier(context.identifier());
        object formalParameters = VisitFormalParameterList(context.formalParameterList());
        object statement = Visit(context.statement());

        if (returnType is not TypeNode returnTypeNode)
        {
            throw new Exception("Expected TypeNode.");
        }
        if (identifier is not IdentifierNode identifierNode)
        {
            throw new Exception("Expected IdentifierNode.");
        }
        if (formalParameters is not List<TypedIdentifierNode> parameters)
        {
            throw new Exception("Expected List<TypedIdentifierNode>.");
        }
        if (statement is not StatementNode statementNode)
        {
            throw new Exception("Expected StatementNode.");
        }

        return new(
            returnType: returnTypeNode,
            identifierNode,
            parameters,
            statementNode
        );
    }

    public override IEnumerable<TypedIdentifierNode> VisitFormalParameterList([NotNull] AQLParser.FormalParameterListContext context)
    {
        List<TypedIdentifierNode> parameters = [];
        AQLParser.TypeContext[] typeContexts = context.type();
        AQLParser.IdentifierContext[] identifierContexts = context.identifier();
        for (int i = 0; i < typeContexts.Length; i++)
        {
            //TypeNode typeNode = VisitType(typeContexts[i]);
            IdentifierNode identifierNode = VisitIdentifier(identifierContexts[i]);

            //TypedIdentifierNode typedIdentifierNode = new(typeNode, identifierNode);
            //parameters.Add(typedIdentifierNode);
        }

        return parameters;
    }

    public override NetworkNode VisitNetworks([NotNull] AQLParser.NetworksContext context)
    {
        if (context.networkDefinition() != null)
        {
            return VisitNetworkDefinition(context.networkDefinition());
        }
        else if (context.queueDefinition() != null)
        {
            return VisitQueueDefinition(context.queueDefinition());
        }
        else
        {
            throw new NotImplementedException("Network definition not implemented.");
        }
    }

    public override QueueDeclarationNode VisitQueueDefinition([NotNull] AQLParser.QueueDefinitionContext context)
    {
        IdentifierNode identifierNode = VisitIdentifier(context.identifier());
        ExpressionNode serviceNode = VisitExpression(context.service);
        ExpressionNode capacityNode = VisitExpression(context.capacity);
        ExpressionNode numberOfServersNode = VisitExpression(context.numberOfServers);
        object metrics = Visit(context.metrics());

        if (metrics is not List<MetricNode> metricsNode)
        {
            throw new Exception("Expected List<MetricNode>.");
        }

        return new(
            identifierNode,
            serviceNode,
            capacityNode,
            numberOfServersNode,
            metricsNode
        );
    }

    public override NetworkDeclarationNode VisitNetworkDefinition([NotNull] AQLParser.NetworkDefinitionContext context)
    {
        IdentifierNode identifierNode = VisitIdentifier(context.identifier());
        IEnumerable<IdentifierNode> inputNodes = VisitIdList(context.inputs);
        IEnumerable<IdentifierNode> outputNodes = VisitIdList(context.outputs);
        IEnumerable<ExpressionNode> instanceNodes = VisitInstanceList(context.instances);
        IEnumerable<RouteNode> routeNodes = VisitRoutes(context.routes());
        IEnumerable<MetricNode> metrics = VisitMetrics(context.metrics());

        if (metrics is not List<MetricNode> metricsNodes)
        {
            throw new Exception("Expected List<MetricNode>.");
        }

        return new(
            identifier: identifierNode,
            inputs: inputNodes,
            outputs: outputNodes,
            instances: instanceNodes,
            routes: routeNodes,
            metrics: metricsNodes
        );
    }

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

    public override IEnumerable<ExpressionNode> VisitInstanceList([NotNull] AQLParser.InstanceListContext context)
    {
        throw new NotImplementedException("Instance list not implemented.");
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
        return VisitMetricList(context.metricList());
    }

    public override IEnumerable<MetricNode> VisitMetricList([NotNull] AQLParser.MetricListContext context)
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
        return new(
            metricName: context.GetText()
        );
    }

    public override SimulateNode VisitSimulateDefinition([NotNull] AQLParser.SimulateDefinitionContext context)
    {
        QualifiedIdentifierNode networkNode = VisitQualifiedId(context.network);
        ExpressionNode runsNode = VisitExpression(context.runs);
        ExpressionNode terminationCriteriaNode = VisitExpression(context.terminationCriteria);

        return new(
            identifier: networkNode,
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
            throw new NotImplementedException("Value not implemented.");
        }
    }

    public override FunctionCallNode VisitFunctionCall(AQLParser.FunctionCallContext context)
    {
        ExpressionNode functionIdentifierNode = VisitQualifiedId(context.functionIdentifier);

        if (context.parameters == null)
        {
            throw new Exception("Expected parameters.");

        }

        IEnumerable<ExpressionNode> parameters = VisitQualifiedIdList(context.parameters);

        return new(
            identifier: functionIdentifierNode,
            actualParameters: parameters
        );
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
            throw new Exception("Expected QualifiedIdentifierNode.");
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

    public override ExpressionNode VisitExpression([NotNull] AQLParser.ExpressionContext context)
    {
        throw new NotImplementedException("Expression not implemented.");
    }

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