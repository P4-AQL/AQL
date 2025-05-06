


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class DoubleLiteralNode(double value) : ExpressionNode
{
    public double Value { get; set; } = value;
}