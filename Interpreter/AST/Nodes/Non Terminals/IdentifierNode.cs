



namespace Interpreter.AST.Nodes.NonTerminals;
public class IdentifierNode(int lineNumber) : Node(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }
}