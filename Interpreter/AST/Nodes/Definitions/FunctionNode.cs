


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.NonNodes;

namespace Interpreter.AST.Nodes.Definitions;
public class FunctionNode(TypeNode returnType, IdentifierNode identifier, IEnumerable<TypeAndIdentifier> parameters, StatementNode body) : DefinitionNode
{
    public TypeNode ReturnType { get; } = returnType;
    public IdentifierNode Identifier { get; } = identifier;
    public IReadOnlyList<TypeAndIdentifier> Parameters { get; } = [.. parameters];
    public StatementNode Body { get; } = body;
}