


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class NegativeNode(ExpressionNode inner) : ExpressionNode
{
    ExpressionNode Inner { get; } = inner;

    public override string ToString() => $"NegativeNode({Inner})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Inner,
        ];
    }
}