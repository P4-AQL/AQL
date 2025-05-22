using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Expressions;

public class InterpreterExpressionTests
{
    [Fact]
    public void InterpretAddNode_ReturnsSumOfLiterals()
    {
        var node = new AddNode(0, new IntLiteralNode(0, 5), new IntLiteralNode(0, 3));
        var result = new InterpreterClass(new DummyProgramNode()).InterpretAddNode(node, null);
        Assert.IsType<double>(result);
        Assert.Equal(8.0, (double)result);
    }

    [Fact]
    public void InterpretMultiplyNode_ReturnsProduct()
    {
        var node = new MultiplyNode(0, new IntLiteralNode(0, 4), new IntLiteralNode(0, 2));
        var result = new InterpreterClass(new DummyProgramNode()).InterpretMultiplyNode(node, null);
        Assert.Equal(8.0, result);
    }

    [Fact]
    public void InterpretDivisionNode_ReturnsQuotient()
    {
        var node = new DivisionNode(0, new IntLiteralNode(0, 8), new IntLiteralNode(0, 2));
        var result = new InterpreterClass(new DummyProgramNode()).InterpretDivisionNode(node, null);
        Assert.Equal(4.0, result);
    }

    [Fact]
    public void InterpretNegativeNode_NegatesValue()
    {
        var node = new NegativeNode(0, new IntLiteralNode(0, 10));
        var result = new InterpreterClass(new DummyProgramNode()).InterpretNegativeNode(node, null);
        Assert.Equal(-10.0, result);
    }

    [Fact]
    public void InterpretNotNode_InvertsBoolean()
    {
        var node = new NotNode(0, new BoolLiteralNode(0, true));
        var result = new InterpreterClass(new DummyProgramNode()).InterpretNotNode(node, null);
        Assert.False(result);
    }

    [Fact]
    public void InterpretEqualNode_ComparesEquality()
    {
        var node = new EqualNode(0, new IntLiteralNode(0, 5), new IntLiteralNode(0, 5));
        var result = new InterpreterClass(new DummyProgramNode()).InterpretEqualNode(node, null);
        Assert.True(result);
    }

    [Fact]
    public void InterpretAndNode_EvaluatesLogicalAnd()
    {
        var node = new AndNode(0, new BoolLiteralNode(0, true), new BoolLiteralNode(0, false));
        var result = new InterpreterClass(new DummyProgramNode()).InterpretAndNode(node, null);
        Assert.False(result);
    }
}
