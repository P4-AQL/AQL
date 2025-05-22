using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Statements;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;

public class InterpreterControlFlowTests
{
    [Fact]
    public void InterpretIfElseNode_EvaluatesIfBranch_WhenConditionTrue()
    {
        var cond = new BoolLiteralNode(0, true);
        var ifBody = new ReturnNode(0, new IntLiteralNode(0, 1));
        var elseBody = new ReturnNode(0, new IntLiteralNode(0, 2));

        var node = new IfElseNode(0, null, cond, ifBody, elseBody);
        var result = new InterpreterClass(new DummyProgramNode()).InterpretIfElseNode(node, new Table<object>());

        Assert.Equal(1, result);
    }

    [Fact]
    public void InterpretIfElseNode_EvaluatesElseBranch_WhenConditionFalse()
    {
        var cond = new BoolLiteralNode(0, false);
        var ifBody = new ReturnNode(0, new IntLiteralNode(0, 1));
        var elseBody = new ReturnNode(0, new IntLiteralNode(0, 2));

        var node = new IfElseNode(0, null, cond, ifBody, elseBody);
        var result = new InterpreterClass(new DummyProgramNode()).InterpretIfElseNode(node, new Table<object>());

        Assert.Equal(2, result);
    }

    [Fact]
    public void InterpretWhileNode_ExecutesBodyUntilFalse()
    {
        var counterId = new SingleIdentifierNode(0, "counter");

        var cond = new LessThanNode(0,
            new IdentifierExpressionNode(0, counterId),
            new IntLiteralNode(0, 3)
        );

        var increment = new AssignNode(
            lineNumber: 0,
            nextStatement: null,
            identifier: counterId,
            expression: new AddNode(0,
                new IdentifierExpressionNode(0, counterId),
                new IntLiteralNode(0, 1)
            )
        );

        var shadow = new Table<object>();
        shadow.ForceBind("counter", 0);

        var node = new WhileNode(0, null, cond, increment);
        var interpreter = new InterpreterClass(new DummyProgramNode());
        interpreter.InterpretWhileNode(node, shadow);

        Assert.True(shadow.Lookup("counter", out var finalValue));
        Assert.IsType<double>(finalValue);
        Assert.Equal(3.0, (double)finalValue);
    }
}
