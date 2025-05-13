




using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Metrics;
using Interpreter.AST.Nodes.Types;

namespace Interpreter.AST.Nodes.NonTerminals;
public class NetworkNode(int lineNumber, NetworkTypeNode customType, SingleIdentifierNode identifier, IEnumerable<NamedMetricNode> metrics) : Node(lineNumber)
{
    public NetworkTypeNode CustomType { get; } = customType;
    public SingleIdentifierNode Identifier { get; } = identifier;
    public IReadOnlyList<NamedMetricNode> Metrics { get; } = [.. metrics];

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