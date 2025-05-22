using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Statements;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Definitions;
using System.Collections.Generic;

public class InterpreterFunctionCallTests
{
    [Fact]
    public void InterpretFunctionCallNode_EvaluatesAndReturns()
    {
        var id = new SingleIdentifierNode(0, "f");
        var param = new SingleIdentifierNode(0, "x");
        var body = new ReturnNode(0, new IdentifierExpressionNode(0, param));

        var fn = new FunctionNode(
            0,
            null,
            DummyTypeNode.Instance,
            id,
            new[] { new TypeAndIdentifier(0, DummyTypeNode.Instance, param) },
            body
        );

        var funcState = new FunctionStateTuple(fn, new Table<object>());

        var interpreter = new InterpreterClass(new DummyProgramNode());
        interpreter.GlobalEnvironment.FunctionState.ForceBind("f", funcState);

        var call = new FunctionCallNode(0, id, new[] { new IntLiteralNode(0, 42) });
        var result = interpreter.InterpretFunctionCallNode(call, null);

        Assert.Equal(42, result);
    }
}
