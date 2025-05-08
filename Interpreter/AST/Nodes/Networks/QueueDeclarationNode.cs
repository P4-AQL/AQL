


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Networks;
public class QueueDeclarationNode(IdentifierNode identifier, ExpressionNode service, ExpressionNode capacity, ExpressionNode numberOfServers, IEnumerable<MetricNode> metrics) : NetworkNode(identifier, metrics)
{
    public ExpressionNode Service { get; } = service;
    public ExpressionNode Capacity { get; } = capacity;
    public ExpressionNode NumberOfServers { get; } = numberOfServers;

    public override string ToString() => $"QueueDeclarationNode({Service}, {Capacity}, {NumberOfServers})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. base.GetChildren(),
            Service,
            Capacity,
            NumberOfServers,
        ];
    }

}