


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class StringLiteralNode(string value) : ExpressionNode
{
    public string Value { get; } = value;

    public override IEnumerable<Node> Children()
    {
        return base.Children();
    }
}