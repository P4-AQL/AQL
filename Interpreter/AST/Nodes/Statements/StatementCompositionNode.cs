


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class StatementCompositionNode(StatementNode left, StatementNode right) : StatementNode
{
    public StatementNode Left { get; } = left;
    public StatementNode Right { get; } = right;

    public override string ToString() => $"StatementCompositionNode({Left}; {Right})";
}