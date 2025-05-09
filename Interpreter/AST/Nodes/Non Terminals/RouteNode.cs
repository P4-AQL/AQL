namespace Interpreter.AST.Nodes.NonTerminals;

public class RouteNode(int lineNumber) : Node(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return [];
    }
}