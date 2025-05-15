


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Networks;
public class InstanceDeclaration(int lineNumber, IdentifierNode existingInstance, SingleIdentifierNode newInstances) : Node(lineNumber)
{
    public IdentifierNode ExistingInstance { get; } = existingInstance;
    public SingleIdentifierNode NewInstances { get; } = newInstances;

    public override string ToString()
    {
        return $"InstanceDeclaration({ExistingInstance}, ({string.Join(", ", NewInstances)})";
    }

    public override IEnumerable<Node> GetChildren()
    {
        return [
            ExistingInstance,
            NewInstances,
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{NewInstances}";

}