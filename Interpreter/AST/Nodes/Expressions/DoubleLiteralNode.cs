


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class DoubleLiteralNode(double value) : ExpressionNode
{
    public double Value { get; set; } = value;

    public override IEnumerable<Node> GetChildren()
    {
        return base.GetChildren();
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Value}";
}