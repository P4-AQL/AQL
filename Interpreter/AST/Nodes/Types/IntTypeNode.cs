


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Types;
public class IntTypeNode() : TypeNode
{
    public override IEnumerable<Node> GetChildren()
    {
        return base.GetChildren();
    }

    public override string GetTypeString() => $"int";

}