


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Programs;
public class SkipNode : StatementNode
{
    public override string ToString() => "SkipNode()";

    public override IEnumerable<Node> Children()
    {
        return base.Children();
    }

}