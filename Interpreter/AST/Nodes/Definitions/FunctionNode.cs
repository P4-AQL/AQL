


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Statements;
using Interpreter.AST.Nodes.Identifiers;

namespace Interpreter.AST.Nodes.Definitions;
public class FunctionNode(int lineNumber, DefinitionNode? nextDefinition, TypeNode returnType, SingleIdentifierNode identifier, IEnumerable<TypeAndIdentifier> parameters, StatementNode body) : DefinitionCompositionNode(lineNumber, nextDefinition)
{
    public TypeNode ReturnType { get; } = returnType;
    public SingleIdentifierNode Identifier { get; } = identifier;
    public IReadOnlyList<TypeAndIdentifier> Parameters { get; } = [.. parameters];
    public StatementNode Body { get; } = body;

    public override string ToString() => $"FunctionNode({ReturnType}, {Identifier}, ({string.Join(',', Parameters)}), {Body}, {NextDefinition})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            ReturnType,
            Identifier,
            .. Parameters,
            Body,
            .. base.GetChildren(),
        ];
    }

    public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Identifier.Identifier}";

}