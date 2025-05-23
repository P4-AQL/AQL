


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Definitions;
public class SimulateNode(int lineNumber, IdentifierNode networkIdentifier, ExpressionNode runs, ExpressionNode terminationCriteria) : DefinitionNode(lineNumber)
{
    public IdentifierNode NetworkIdentifier { get; } = networkIdentifier;
    public ExpressionNode Runs { get; } = runs;
    public ExpressionNode TerminationCriteria { get; } = terminationCriteria;

    public override string ToString() => $"SimulateNode({NetworkIdentifier}, {Runs}, {TerminationCriteria})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            NetworkIdentifier,
            Runs,
            TerminationCriteria,
            .. base.GetChildren(),
        ];
    }

}