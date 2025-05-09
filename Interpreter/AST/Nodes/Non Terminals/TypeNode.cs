



namespace Interpreter.AST.Nodes.NonTerminals;
public abstract class TypeNode(int lineNumber) : Node(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{GetTypeString()}";

    public abstract string GetTypeString();
}