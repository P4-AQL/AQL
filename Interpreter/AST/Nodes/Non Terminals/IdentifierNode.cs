



namespace Interpreter.AST.Nodes.NonTerminals;

public abstract class IdentifierNode(int lineNumber) : Node(lineNumber)
{
    public abstract string FirstIdentifier { get; }
    public abstract string LastIdentifier { get; }
    public abstract string FullIdentifier { get; }

    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }
}