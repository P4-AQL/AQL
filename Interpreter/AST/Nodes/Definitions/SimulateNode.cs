


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Definitions;
public class SimulateNode(ExpressionNode networkIdentifier, ExpressionNode runs, ExpressionNode terminationCriteria) : DefinitionNode
{
    public ExpressionNode NetworkIdentifier { get; } = networkIdentifier;
    public ExpressionNode Runs { get; } = runs;
    public ExpressionNode TerminationCriteria { get; } = terminationCriteria;

    public override string ToString() => $"SimulateNode({NetworkIdentifier}, {Runs}, {TerminationCriteria})";

    public override IEnumerable<Node> Children()
    {
        return [
            .. base.Children(),
            NetworkIdentifier,
            Runs,
            TerminationCriteria,
        ];
    }

}