


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Expressions;

namespace Interpreter.AST.Nodes;
public class TypedIdentifierNode(TypeNode type, IdentifierNode identifier) : Node
{
    public TypeNode Type { get; } = type;
    public IdentifierNode Identifier { get; } = identifier;

    public override string ToString() => $"TypedIdentifierNode({Type}, {Identifier})";
}
