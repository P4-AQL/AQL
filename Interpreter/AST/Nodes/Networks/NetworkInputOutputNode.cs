



using Interpreter.AST.Nodes.Expressions;

namespace Interpreter.AST.Nodes.Networks;
public class NetworkInputOutputNode(IEnumerable<IdentifierNode> inputs, IEnumerable<IdentifierNode> outputs)
{
    public IReadOnlyList<IdentifierNode> Inputs { get; } = [.. inputs];
    public IReadOnlyList<IdentifierNode> Outputs { get; } = [.. outputs];

}