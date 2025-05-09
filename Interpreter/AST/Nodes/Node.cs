


namespace Interpreter.AST.Nodes;
public abstract class Node(int lineNumber)
{
    public int LineNumber { get; } = lineNumber;
    public abstract IEnumerable<Node> GetChildren();

    public virtual string GetNodeLabel() => GetType().Name;
}