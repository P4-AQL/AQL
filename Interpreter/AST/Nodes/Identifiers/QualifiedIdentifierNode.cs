


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Identifiers;
public class QualifiedIdentifierNode(int lineNumber, SingleIdentifierNode leftIdentifier, SingleIdentifierNode rightIdentifier) : IdentifierNode(lineNumber)
{
    public SingleIdentifierNode Identifier { get; } = leftIdentifier;
    public SingleIdentifierNode Expression { get; } = rightIdentifier;

    public override string ToString() => $"QualifiedIdentifierNode({Identifier}, {Expression})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Identifier,
            Expression,
            .. base.GetChildren(),
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier.Identifier}";
}