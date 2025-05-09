


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Programs;
public class DefinitionProgramNode(int lineNumber, DefinitionNode definition) : ProgramNode(lineNumber)
{
    public DefinitionNode Definition { get; } = definition;

    public override string ToString() => $"DefinitionProgramNode({Definition})";

    public override IEnumerable<Node> GetChildren()
    {
        return [
            Definition,
            .. base.GetChildren(),
        ];
    }
}