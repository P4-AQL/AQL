


namespace Interpreter.AST.Nodes;
public abstract class Node
{
    public abstract IEnumerable<Node> Children();

    public virtual string GetNodeLabel() => GetType().Name;
}