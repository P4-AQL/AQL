


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Programs;
public class SkipNode(int lineNumber) : StatementNode(lineNumber)
{
    public override string ToString() => "SkipNode()";

    public override IEnumerable<Node> GetChildren()
    {
        return base.GetChildren();
    }

}