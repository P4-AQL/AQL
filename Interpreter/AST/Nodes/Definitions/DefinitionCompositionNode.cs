


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Definitions;
public abstract class DefinitionCompositionNode(int lineNumber, DefinitionNode? nextDefinition) : DefinitionNode(lineNumber)
{
    public DefinitionNode? NextDefinition { get; } = nextDefinition;

    public override IEnumerable<Node> GetChildren()
    {
        return NextDefinition is null
        ? [
            .. base.GetChildren(),
        ]
        : [
            ..base.GetChildren(),
            NextDefinition,
        ];
    }
}