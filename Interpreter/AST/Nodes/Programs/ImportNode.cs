


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.AST.Nodes.Programs
{
    public class ImportNode(StringLiteralNode @namespace, ProgramNode program) : ProgramNode
    {
        public StringLiteralNode Namespace { get; } = @namespace;
        public ProgramNode Program { get; } = program;

        public override string ToString() => $"ImportNode({Namespace}, {Program})";

    }
}