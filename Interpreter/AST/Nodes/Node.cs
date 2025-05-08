


namespace Interpreter.AST.Nodes;
public abstract class Node
{
    public abstract IEnumerable<Node> GetChildren();

    public virtual string GetNodeLabel() => GetType().Name;
}