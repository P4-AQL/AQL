


namespace Interpreter.SemanticAnalysis;
public struct QueueTuple
{
    public Func<object> Service;
    public int Capacity;
    public int Servers;
    public IEnumerable<string> Metrics;
}