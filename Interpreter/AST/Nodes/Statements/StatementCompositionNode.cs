


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Statements;
public abstract class StatementCompositionNode(int lineNumber, StatementNode? nextStatement) : StatementNode(lineNumber)
{
    public StatementNode? NextStatement { get; } = nextStatement;

    public override IEnumerable<Node> GetChildren()
    {
        return NextStatement is null
        ? [
            .. base.GetChildren()
        ]
        : [
            NextStatement,
            .. base.GetChildren(),
        ];
    }
}