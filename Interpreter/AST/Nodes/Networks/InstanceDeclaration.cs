


using Interpreter.AST.Nodes.Expressions;

namespace Interpreter.AST.Nodes.Networks;
public class InstanceDeclaration(QualifiedIdentifierNode existingInstance, IEnumerable<IdentifierNode> newInstances) : Node
{
    public QualifiedIdentifierNode ExistingInstance { get; } = existingInstance;
    public IEnumerable<IdentifierNode> NewInstances { get; } = newInstances;

    public override string ToString()
    {
        return $"InstanceDeclaration({ExistingInstance}, ({string.Join(", ", NewInstances)})";
    }

}