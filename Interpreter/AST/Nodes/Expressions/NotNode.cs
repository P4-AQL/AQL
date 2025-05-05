


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class NotNode(ExpressionNode inner) : ExpressionNode
{
    public ExpressionNode Inner { get; } = inner;

    public override string ToString() => $"NotNode({Inner})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Inner,
        ];
    }
}