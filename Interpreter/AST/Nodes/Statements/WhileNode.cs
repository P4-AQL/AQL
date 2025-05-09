


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class WhileNode(int lineNumber, StatementNode? nextStatement, ExpressionNode condition, StatementNode body) : StatementCompositionNode(lineNumber, nextStatement)
{
    public ExpressionNode Condition { get; } = condition;
    public StatementNode Body { get; } = body;

    public override string ToString() => $"WhileNode({Condition}, {Body}, {NextStatement})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Condition,
            Body,
            .. base.GetChildren(),
        ];
    }

}