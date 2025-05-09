


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class NotNode(int lineNumber, ExpressionNode inner) : ExpressionNode(lineNumber)
{
    public ExpressionNode Inner { get; } = inner;

    public override string ToString() => $"NotNode({Inner})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Inner,
            .. base.GetChildren(),
        ];
    }
}