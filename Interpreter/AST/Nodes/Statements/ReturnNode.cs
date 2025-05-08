


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public class ReturnNode(ExpressionNode expression) : StatementNode
{
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"ReturnNpde({Expression})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. base.GetChildren(),
            Expression,
        ];
    }

}