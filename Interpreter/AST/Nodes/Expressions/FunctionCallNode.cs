


using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;

public class FunctionCallNode(int lineNumber, IdentifierNode identifier, IEnumerable<ExpressionNode> actualParameters) : ExpressionNode(lineNumber)
{
    public IdentifierNode Identifier { get; } = identifier;
    public IReadOnlyList<ExpressionNode> ActualParameters { get; } = [.. actualParameters];
    public override string ToString() => $"FunctionCallNode({Identifier}, ({string.Join(", ", ActualParameters)}))";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Identifier,
            .. ActualParameters,
            .. base.GetChildren(),
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{ActualParameters.Count} parameters";
}