


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Types;
public class NetworkType(int lineNumber, IdentifierNode identifier) : TypeNode(lineNumber)
{
    public IdentifierNode Identifier { get; } = identifier;

    public override string ToString() => $"NetworkType({Identifier})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. base.GetChildren(),
            Identifier,
        ];
    }

    public override string GetTypeString() => $"{Identifier}";

}