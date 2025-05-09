


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Types;
public class FunctionTypeNode(int lineNumber, IEnumerable<TypeNode> formalParameterTypes, TypeNode returnType) : TypeNode(lineNumber)
{
    public IReadOnlyList<TypeNode> FormalParameterTypes { get; } = [.. formalParameterTypes];
    public TypeNode ReturnType { get; } = returnType;
    public override string ToString() => $"FunctionTypeNode(({string.Join(", ", FormalParameterTypes)}), {ReturnType})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. base.GetChildren(),
            .. FormalParameterTypes,
            ReturnType,
        ];
    }

    public override string GetTypeString() => $"Func<{string.Join(',', FormalParameterTypes.Select(parameter => parameter.GetTypeString()))}, {ReturnType.GetTypeString}>";

}