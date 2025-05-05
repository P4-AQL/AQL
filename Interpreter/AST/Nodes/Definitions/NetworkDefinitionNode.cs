


using System.Text;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Definitions;
public class NetworkDefinitionNode(NetworkNode network) : DefinitionNode
{
    public NetworkNode Network { get; } = network;

    public override string ToString() => $"NetworkDefinitionNode({Network})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            Network
        ];
    }
}