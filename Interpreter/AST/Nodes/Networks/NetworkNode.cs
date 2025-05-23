


using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Routes;
using Interpreter.AST.Nodes.Metrics;

namespace Interpreter.AST.Nodes.Networks;

public class Network
{
    public string Name { get; set; }
    public IReadOnlyList<SingleIdentifierNode> Inputs { get; set; }
    public IReadOnlyList<SingleIdentifierNode> Outputs { get; set; }
    public IReadOnlyList<InstanceDeclaration> NewInstances { get; set; }
    public IReadOnlyList<RouteDefinitionNode> Routes { get; set; }
    public IReadOnlyList<NamedMetricNode> Metrics { get; set; }

    public Network(string alias, NetworkDeclarationNode source)
    {
        Name = alias;
        Inputs = source.Inputs;
        Outputs = source.Outputs;
        NewInstances = source.Instances;
        Routes = source.Routes;
        Metrics = source.Metrics;
    }
}
