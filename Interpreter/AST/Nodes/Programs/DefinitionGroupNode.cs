


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Programs;
public class DefinitionGroupNode(IEnumerable<DefinitionNode> definitions) : ProgramNode
{
    public IEnumerable<DefinitionNode> Definitions { get; } = definitions;

    public override string ToString() => $"DefinitionGroupNode({string.Join(", ", Definitions)})";

}