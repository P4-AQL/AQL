


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Networks;
public class InstanceDeclaration(ExpressionNode existingInstance, IEnumerable<IdentifierNode> newInstances) : Node
{
    public ExpressionNode ExistingInstance { get; } = existingInstance;
    public IEnumerable<IdentifierNode> NewInstances { get; } = newInstances;

    public override string ToString()
    {
        return $"InstanceDeclaration({ExistingInstance}, ({string.Join(", ", NewInstances)})";
    }

    public override IEnumerable<Node> Children()
    {
        return [
            ExistingInstance,
            .. NewInstances,
        ];
    }

}