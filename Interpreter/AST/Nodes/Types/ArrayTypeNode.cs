


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Types;
public class ArrayTypeNode(TypeNode innerType) : TypeNode
{
    public TypeNode InnerType { get; } = innerType;
    public override string ToString() => $"ArrayTypeNode({InnerType})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            InnerType,
        ];
    }

}