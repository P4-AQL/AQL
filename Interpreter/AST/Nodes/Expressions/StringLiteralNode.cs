


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class StringLiteralNode(string value) : ExpressionNode
{
    public string Value { get; } = value;

    public override IEnumerable<Node> GetChildren()
    {
        return base.GetChildren();
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Value}";

}