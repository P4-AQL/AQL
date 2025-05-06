


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Definitions;
public class ConstDeclarationNode(TypeNode type, IdentifierNode identifier, ExpressionNode expression) : DefinitionNode
{
    public TypeNode Type { get; } = type;
    public IdentifierNode Identifier { get; } = identifier;
    public ExpressionNode Expression { get; } = expression;

    public override string ToString() => $"ConstDeclaration({Type} {Identifier} = {Expression})";
}