


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Types;
public class NetworkType(IdentifierNode identifier) : TypeNode
{
    public IdentifierNode Identifier { get; } = identifier;

    public override string ToString() => $"NetworkType({Identifier})";
}