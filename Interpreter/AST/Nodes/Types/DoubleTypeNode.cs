


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Types;
public class DoubleTypeNode(int lineNumber) : TypeNode(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return base.GetChildren();
    }

    public override string GetTypeString() => $"double";

}