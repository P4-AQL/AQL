


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Statements;

namespace Interpreter.AST.Nodes.Definitions;
public class FunctionNode(TypeNode returnType, IdentifierNode identifier, IEnumerable<TypeAndIdentifier> parameters, StatementNode body) : DefinitionNode
{
    public TypeNode ReturnType { get; } = returnType;
    public IdentifierNode Identifier { get; } = identifier;
    public IReadOnlyList<TypeAndIdentifier> Parameters { get; } = [.. parameters];
    public StatementNode Body { get; } = body;

    public override string ToString() => $"FunctionNode({ReturnType}, {Identifier}, ({string.Join(',', Parameters)}), {Body})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. base.GetChildren(),
            ReturnType,
            Identifier,
            .. Parameters,
            Body,
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier.Identifier}";

}