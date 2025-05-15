



using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;

namespace Interpreter.AST.Nodes.Networks;
public class NetworkInputOutputNode(IEnumerable<SingleIdentifierNode> inputs, IEnumerable<SingleIdentifierNode> outputs)
{
    public IReadOnlyList<SingleIdentifierNode> Inputs { get; } = [.. inputs];
    public IReadOnlyList<SingleIdentifierNode> Outputs { get; } = [.. outputs];

}