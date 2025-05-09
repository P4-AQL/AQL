


using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Programs;

namespace Interpreter.AST.Nodes.NonTerminals;
public class ProgramNode(int lineNumber) : Node(lineNumber)
{
    public override IEnumerable<Node> GetChildren()
    {
        return [

        ];
    }
}