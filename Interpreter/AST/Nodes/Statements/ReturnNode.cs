


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class ReturnNode(int lineNumber, ExpressionNode expression) : StatementNode(lineNumber)
{
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"ReturnNode({Expression})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Expression,
            .. base.GetChildren(),
        ];
    }

}