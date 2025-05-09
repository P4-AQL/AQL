


namespace Interpreter.AST.Nodes.NonTerminals;
public class DefinitionNode(int lineNumber) : Node(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }
}