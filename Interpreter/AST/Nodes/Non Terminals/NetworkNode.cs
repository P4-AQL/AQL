




using Interpreter.AST.Nodes.Expressions;

namespace Interpreter.AST.Nodes.NonTerminals;
public class NetworkNode(IdentifierNode identifier, IEnumerable<MetricNode> metrics) : Node
{
    public IdentifierNode Identifier { get; } = identifier;
    public IReadOnlyList<MetricNode> Metrics { get; } = [.. metrics];

    public override string ToString() => $"NetworkNode({Identifier}, ({string.Join(',', Metrics)}))";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Identifier,
            .. Metrics,
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier.Identifier}";
}