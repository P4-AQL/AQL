


namespace Interpreter.AST.Nodes.NonTerminals;
public class StatementNode(int lineNumber) : Node(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }
}