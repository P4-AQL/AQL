


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class IntLiteralNode(int value) : ExpressionNode
{
    public int Value { get; } = value;

    public override IEnumerable<Node> Children()
    {
        return base.Children();
    }
}