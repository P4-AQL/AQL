


namespace Interpreter.AST.Nodes;
public class RootNode(IEnumerable<Node> children) : Node
{
    public IReadOnlyList<Node> Children { get; } = [.. children];

    public override IEnumerable<Node> GetChildren()
    {
        return Children;
    }
}