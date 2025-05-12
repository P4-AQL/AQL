


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class IndexingNode(int lineNumber, IdentifierNode target, ExpressionNode index) : ExpressionNode(lineNumber)
{
    public IdentifierNode Target { get; } = target;
    public ExpressionNode Index { get; } = index;

    public override string ToString() => $"IndexingNode({Target},{Index})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Target,
            Index,
            .. base.GetChildren(),
        ];
    }
}