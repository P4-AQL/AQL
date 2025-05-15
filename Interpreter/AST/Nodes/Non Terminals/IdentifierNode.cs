



namespace Interpreter.AST.Nodes.NonTerminals;
public abstract class IdentifierNode(int lineNumber) : Node(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }
}