


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Identifiers;

public class SingleIdentifierNode(int lineNumber, string identifier) : IdentifierNode(lineNumber)
{
    public string Identifier { get; } = identifier;

    public override string ToString()
    {
        return $"Identifier({Identifier})";
    }

    public override IEnumerable<Node> GetChildren()
    {
        return base.GetChildren();
    }

    public override string FullIdentifier => Identifier;

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier}";
}
