


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Types;
public class NetworkTypeNode(int lineNumber, SingleIdentifierNode identifier) : TypeNode(lineNumber)
{
    public SingleIdentifierNode Identifier { get; } = identifier;

    public override string ToString() => $"NetworkType({Identifier})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Identifier,
            .. base.GetChildren(),
        ];
    }

    public override string GetTypeString() => $"{Identifier}";

}