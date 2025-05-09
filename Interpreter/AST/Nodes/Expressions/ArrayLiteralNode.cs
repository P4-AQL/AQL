


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class ArrayLiteralNode(int lineNumber, IEnumerable<ExpressionNode> elements) : LiteralNode(lineNumber)
{
    public IReadOnlyList<ExpressionNode> Elements { get; } = [.. elements];

    public override string ToString() => $"ArrayTypeNode(Elements: [{string.Join(',', Elements)}])";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. Elements,
            .. base.GetChildren(),
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n[{Elements.Count}]";

}