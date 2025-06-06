


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Networks;
public class InstanceDeclaration(int lineNumber, IdentifierNode existingInstance, SingleIdentifierNode newInstance) : Node(lineNumber)
{
    public IdentifierNode ExistingInstance { get; } = existingInstance;
    public SingleIdentifierNode NewInstance { get; } = newInstance;

    public override string ToString()
    {
        return $"InstanceDeclaration({ExistingInstance}, ({string.Join(", ", NewInstance)})";
    }

    public override IEnumerable<Node> GetChildren()
    {
        return [
            ExistingInstance,
            NewInstance,
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{NewInstance}";

}