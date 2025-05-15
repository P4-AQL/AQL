


using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public struct Queue
{
    public ExpressionNode Service { get; }
    public int NumberOfServers { get; }
    public int Capacity { get; }
    public IEnumerable<string> Metrics { get; }
}