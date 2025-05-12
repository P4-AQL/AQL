




using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;

namespace Interpreter.AST.Nodes.NonTerminals;
public class NetworkNode(int lineNumber, SingleIdentifierNode identifier, IEnumerable<MetricNode> metrics) : Node(lineNumber)
{
    public SingleIdentifierNode Identifier { get; } = identifier;
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