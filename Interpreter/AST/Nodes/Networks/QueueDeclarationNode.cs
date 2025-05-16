


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Metrics;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Types;

namespace Interpreter.AST.Nodes.Networks;

public class QueueDeclarationNode(int lineNumber, NetworkTypeNode customType, SingleIdentifierNode identifier, ExpressionNode service, ExpressionNode capacity, ExpressionNode numberOfServers, IEnumerable<NamedMetricNode> metrics) : NetworkNode(lineNumber, customType, identifier, metrics)
{
    public ExpressionNode Service { get; } = service;
    public ExpressionNode Capacity { get; } = capacity;
    public ExpressionNode Servers { get; } = numberOfServers;

    public override string ToString() => $"QueueDeclarationNode({Service}, {Capacity}, {Servers})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Service,
            Capacity,
            Servers,
            .. base.GetChildren(),
        ];
    }

}