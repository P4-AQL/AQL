


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Types;
public class FunctionTypeNode(IEnumerable<TypeNode> formalParameterTypes, TypeNode returnType) : TypeNode
{
    public IEnumerable<TypeNode> FormalParameterTypes { get; } = formalParameterTypes.ToList();
    public TypeNode ReturnType { get; } = returnType;
    public override string ToString() => $"FunctionTypeNode(({string.Join(", ", FormalParameterTypes)}), {ReturnType})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            .. FormalParameterTypes,
            ReturnType,
        ];
    }

}