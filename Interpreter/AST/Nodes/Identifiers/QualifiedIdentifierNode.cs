


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Identifiers;

public class QualifiedIdentifierNode(int lineNumber, SingleIdentifierNode leftIdentifier, SingleIdentifierNode rightIdentifier) : IdentifierNode(lineNumber)
{
    public SingleIdentifierNode LeftIdentifier { get; } = leftIdentifier;
    public SingleIdentifierNode RightIdentifier { get; } = rightIdentifier;

    public override string ToString() => $"QualifiedIdentifierNode({LeftIdentifier}, {RightIdentifier})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            LeftIdentifier,
            RightIdentifier,
            .. base.GetChildren(),
        ];
    }

    public override string FirstIdentifier => LeftIdentifier.Identifier;
    public override string LastIdentifier => RightIdentifier.Identifier;
    public override string FullIdentifier => string.Join('.', LeftIdentifier.Identifier, RightIdentifier.Identifier);

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{LeftIdentifier.Identifier}";
}