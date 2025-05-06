


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Expressions;

namespace Interpreter.AST.Nodes.Definitions;
public class FunctionNode(TypeNode returnType, IdentifierNode identifier, IEnumerable<TypedIdentifierNode> parameters, StatementNode body) : DefinitionNode
{
    public TypeNode ReturnType { get; } = returnType;
    public IdentifierNode Identifier { get; } = identifier;
    public IReadOnlyList<TypedIdentifierNode> Parameters { get; } = [.. parameters];
    public StatementNode Body { get; } = body;
}