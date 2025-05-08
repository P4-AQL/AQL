


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Programs
{
    public class ImportNode(IdentifierNode @namespace) : ProgramNode
    {
        public IdentifierNode Namespace { get; } = @namespace;

        public override string ToString() => $"ImportNode({Namespace})";

        public override IEnumerable<Node> GetChildren()
        {
            return [
                .. base.GetChildren(),
                Namespace,
             ];
        }

        public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Namespace}";

    }
}