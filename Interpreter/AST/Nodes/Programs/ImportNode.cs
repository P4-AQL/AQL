


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Programs
{
    public class ImportNode(int lineNumber, IdentifierNode @namespace, ProgramNode? nextProgram) : ProgramNode(lineNumber)
    {
        public IdentifierNode Namespace { get; } = @namespace;
        public ProgramNode? NextProgram { get; } = nextProgram;

        public override string ToString() => $"ImportNode({Namespace}, {NextProgram})";

        public override IEnumerable<Node> GetChildren()
        {
            return [
                .. base.GetChildren(),
                Namespace,
                NextProgram,
             ];
        }

        public override string GetNodeLabel() => $"{base.GetNodeLabel()}\n{Namespace}";

    }
}