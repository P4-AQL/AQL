


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Definitions;
public class SimulateNode(ExpressionNode identifier, ExpressionNode runs, ExpressionNode terminationCriteria) : DefinitionNode
{
    public ExpressionNode IdentifierNode { get; } = identifier;
    public ExpressionNode Runs { get; } = runs;
    public ExpressionNode TerminationCriteria { get; } = terminationCriteria;

}