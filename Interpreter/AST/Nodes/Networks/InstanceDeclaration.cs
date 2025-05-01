


using Interpreter.AST.Nodes.Expressions;

namespace Interpreter.AST.Nodes.Networks;
public class InstanceDeclaration(IdentifierNode existingInstance, IEnumerable<IdentifierNode> newInstances) : Node
{
    public IdentifierNode ExistingInstance { get; } = existingInstance;
    public IEnumerable<IdentifierNode> NewInstances { get; } = newInstances;

    public override string ToString()
    {
        return $"InstanceDeclaration({ExistingInstance}, ({string.Join(", ", NewInstances)})";
    }

}