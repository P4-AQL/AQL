


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class WhileNode(ExpressionNode condition, StatementNode body) : StatementNode
{
    public ExpressionNode Condition { get; } = condition;
    public StatementNode Body { get; } = body;

    public override string ToString() => $"WhileNode({Condition}, {Body})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Condition,
            Body,
        ];
    }

}