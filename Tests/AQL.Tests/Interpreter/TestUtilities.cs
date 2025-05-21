using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.SemanticAnalysis;

public class DummyProgramNode : ProgramNode
{
    public DummyProgramNode() : base(0) { }
}

public class DummyTypeNode : TypeNode
{
    public static DummyTypeNode Instance { get; } = new DummyTypeNode();
    private DummyTypeNode() : base(0) { }

    public override string GetNodeLabel() => "DummyType";
    public override string GetTypeString() => "DummyType";
}

public class DummyQueueable : Queueable
{
    public DummyQueueable(string name) : base(name) { }
    public override Queueable FindQueueable(string name) => this;
}