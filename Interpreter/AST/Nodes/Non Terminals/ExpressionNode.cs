



namespace Interpreter.AST.Nodes.NonTerminals;
public class ExpressionNode(int lineNumber) : Node(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }
}
