


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class IndexingNode(ExpressionNode target, ExpressionNode index) : ExpressionNode
{
    public ExpressionNode Target { get; } = target;
    public ExpressionNode Index { get; } = index;

    public override string ToString() => $"IndexingNode({Target},{Index})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Target,
            Index,
        ];
    }
}