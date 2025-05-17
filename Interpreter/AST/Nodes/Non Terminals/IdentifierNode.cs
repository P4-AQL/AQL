



namespace Interpreter.AST.Nodes.NonTerminals;

public abstract class IdentifierNode(int lineNumber) : Node(lineNumber)
{
    public abstract string FullIdentifier { get; }

    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }
}