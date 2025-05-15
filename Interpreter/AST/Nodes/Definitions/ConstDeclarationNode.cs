


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Definitions;
public class ConstDeclarationNode(int lineNumber, DefinitionNode? nextDefinition, TypeNode type, SingleIdentifierNode identifier, ExpressionNode expression) : DefinitionCompositionNode(lineNumber, nextDefinition)
{
    public TypeNode Type { get; } = type;
    public SingleIdentifierNode Identifier { get; } = identifier;
    public ExpressionNode Expression { get; } = expression;
    public override string ToString() => $"ConstDeclaration({Type}, {Identifier}, {Expression}, {NextDefinition})";

    public override IEnumerable<Node> GetChildren()
    {
        return
        [
            Type,
            Identifier,
            Expression,
            .. base.GetChildren(),
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier.Identifier}";

}