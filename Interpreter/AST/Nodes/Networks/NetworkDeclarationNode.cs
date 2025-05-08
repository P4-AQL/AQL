


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Networks;
public class NetworkDeclarationNode(IdentifierNode identifier, IEnumerable<IdentifierNode> inputs, IEnumerable<IdentifierNode> outputs, IEnumerable<InstanceDeclaration> instances, IEnumerable<RouteNode> routes, IEnumerable<MetricNode> metrics) : NetworkNode(identifier, metrics)
{
    public IReadOnlyList<IdentifierNode> Inputs { get; } = [.. inputs];
    public IReadOnlyList<IdentifierNode> Outputs { get; } = [.. outputs];
    public IReadOnlyList<InstanceDeclaration> Instances { get; } = [.. instances];
    public IReadOnlyList<RouteNode> Routes { get; } = [.. routes];

    public override string ToString() => $"NetworkDeclaration(({string.Join(',', Identifier)}), ({string.Join(',', Inputs)}), ({string.Join(',', Outputs)}), ({string.Join(',', Instances)}), ({string.Join(',', Routes)}), ({string.Join(',', Metrics)}))";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. base.GetChildren(),
            .. Inputs,
            .. Outputs,
            .. Instances,
            .. Routes,
        ];
    }
}