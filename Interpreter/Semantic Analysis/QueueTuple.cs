


namespace Interpreter.SemanticAnalysis;

public struct QueueTuple
{
    public int Servers;
    public int Capacity;
    public Func<double> Service;
    public IEnumerable<string> Metrics;
}