


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Networks;
public class NetworkDeclarationNode(IdentifierNode identifier, IEnumerable<IdentifierNode> inputs, IEnumerable<IdentifierNode> outputs, IEnumerable<ExpressionNode> instances, IEnumerable<RouteNode> routes, IEnumerable<MetricNode> metrics) : NetworkNode(identifier, metrics)
{
    public IReadOnlyList<IdentifierNode> Inputs { get; } = [.. inputs];
    public IReadOnlyList<IdentifierNode> Outputs { get; } = [.. outputs];
    public IReadOnlyList<ExpressionNode> Instances { get; } = [.. instances];
    public IReadOnlyList<RouteNode> Routes { get; } = [.. routes];

    public override string ToString() => $"NetworkDeclaration({Identifier}, {Inputs}, {Outputs}, {Instances}, {Routes}, {Metrics})";
}