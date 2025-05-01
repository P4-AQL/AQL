


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Expressions;
public class BoolLiteralNode(bool value) : ExpressionNode
{
    public bool Value { get; } = value;
}