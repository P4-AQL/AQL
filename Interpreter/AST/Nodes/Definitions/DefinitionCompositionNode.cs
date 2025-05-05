


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Definitions;
public class DefinitionCompositionNode(DefinitionNode left, DefinitionNode right) : DefinitionNode
{
    public DefinitionNode Left { get; } = left;
    public DefinitionNode Right { get; } = right;

    public override string ToString() => $"DefinitionCompositionNode({Left}, {Right})";
}