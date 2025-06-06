


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Metrics;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Routes;
using Interpreter.AST.Nodes.Types;

namespace Interpreter.AST.Nodes.Networks;

public class NetworkDeclarationNode(int lineNumber, NetworkTypeNode customType, SingleIdentifierNode identifier, IEnumerable<SingleIdentifierNode> inputs, IEnumerable<SingleIdentifierNode> outputs, IEnumerable<InstanceDeclaration> instances, IEnumerable<RouteDefinitionNode> routes, IEnumerable<NamedMetricNode> metrics) : NetworkNode(lineNumber, customType, identifier, metrics)
{
    public IReadOnlyList<SingleIdentifierNode> Inputs { get; } = [.. inputs];
    public IReadOnlyList<SingleIdentifierNode> Outputs { get; } = [.. outputs];
    public IReadOnlyList<InstanceDeclaration> Instances { get; } = [.. instances];
    public IReadOnlyList<RouteDefinitionNode> Routes { get; } = [.. routes];

    public override string ToString() => $"NetworkDeclaration(({string.Join(',', Identifier)}), ({string.Join(',', Inputs)}), ({string.Join(',', Outputs)}), ({string.Join(',', Instances)}), ({string.Join(',', Routes)}), ({string.Join(',', Metrics)}))";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. Inputs,
            .. Outputs,
            .. Instances,
            .. Routes,
            .. base.GetChildren(),
        ];
    }
}