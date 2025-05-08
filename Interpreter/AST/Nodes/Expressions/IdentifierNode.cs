


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;

public class IdentifierNode(string identifier) : ExpressionNode
{
    public string Identifier { get; } = identifier;

    public override string ToString()
    {
        return $"Identifier({Identifier})";
    }

    public override IEnumerable<Node> GetChildren()
    {
        return base.GetChildren();
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier}";
}
