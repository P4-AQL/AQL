


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Programs;
public class DefinitionProgramNode(DefinitionNode definition) : ProgramNode
{
    public DefinitionNode Definition { get; } = definition;

    public override string ToString() => $"DefinitionProgramNode({Definition})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            .. base.GetChildren(),
            Definition,
        ];
    }
}