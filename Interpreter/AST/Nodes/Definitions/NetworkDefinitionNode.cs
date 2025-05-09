


using System.Text;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Definitions;
public class NetworkDefinitionNode(int lineNumber, DefinitionNode? nextDefinition, NetworkNode network) : DefinitionCompositionNode(lineNumber, nextDefinition)
{
    public NetworkNode Network { get; } = network;

    public override string ToString() => $"NetworkDefinitionNode({Network}, {NextDefinition})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Network,
            .. base.GetChildren(),
        ];
    }
}