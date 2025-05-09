


namespace Interpreter.AST.Nodes.NonTerminals;
public class MetricNode(int lineNumber) : Node(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }
}