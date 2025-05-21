using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Statements;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Definitions;

public class InterpreterVariableTests
{
    [Fact]
    public void InterpretConstDeclaration_BindsValueToVariableState()
    {
        var identifier = new SingleIdentifierNode(0, "x");
        var expression = new IntLiteralNode(0, 42);
        var constNode = new ConstDeclarationNode(0, null, DummyTypeNode.Instance, identifier, expression);

        var interpreter = new InterpreterClass(new DummyProgramNode());
        interpreter.InterpretConstDeclaration(constNode);

        Assert.True(interpreter.GlobalEnvironment.VariableState.Lookup("x", out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void InterpretVariableDeclaration_BindsValueToShadowState()
    {
        var id = new SingleIdentifierNode(0, "foo");
        var expr = new IntLiteralNode(0, 123);
        var node = new VariableDeclarationNode(0, null, DummyTypeNode.Instance, id, expr);

        var interpreter = new InterpreterClass(new DummyProgramNode());
        var shadow = new Table<object>();

        interpreter.InterpretVariableDeclaration(node, shadow);

        Assert.True(shadow.Lookup("foo", out var result));
        Assert.Equal(123, result);
    }

    [Fact]
    public void InterpretAssignment_UpdatesShadowState()
    {
        var id = new SingleIdentifierNode(0, "bar");
        var expr = new IntLiteralNode(0, 321);
        var node = new AssignNode(0, null, id, expr);

        var shadow = new Table<object>();
        shadow.ForceBind("bar", 0);

        var interpreter = new InterpreterClass(new DummyProgramNode());
        interpreter.InterpretAssignment(node, shadow);

        Assert.True(shadow.Lookup("bar", out var result));
        Assert.Equal(321, result);
    }

    [Fact]
    public void LookupVariableHelper_FindsValueInShadowState()
    {
        var interpreter = new InterpreterClass(new DummyProgramNode());
        var shadow = new Table<object>();
        shadow.ForceBind("y", 99);

        bool found = interpreter.LookupVariableHelper("y", shadow, out var result);

        Assert.True(found);
        Assert.Equal(99, result);
    }
}
