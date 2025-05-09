


namespace Interpreter.AST.Nodes;
public class CollectionNode(int lineNumber, IEnumerable<Node> children) : Node(lineNumber)
{
    public IReadOnlyList<Node> Children { get; } = [.. children];

    public override IEnumerable<Node> GetChildren()
    {
        return Children;
    }
}