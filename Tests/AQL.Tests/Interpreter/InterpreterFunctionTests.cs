using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Statements;
using Interpreter.AST.Nodes.Identifiers;

public class InterpreterFunctionTests
{
    [Fact]
    public void InterpretFunctionDeclaration_BindsFunctionToState()
    {
        var identifier = new SingleIdentifierNode(0, "TestFunc");
        var body = new SkipNode(0);
        var funcNode = new FunctionNode(
            lineNumber: 0,
            nextDefinition: null,
            returnType: DummyTypeNode.Instance,
            identifier: identifier,
            parameters: [],
            body: body
        );

        var interpreter = new InterpreterClass(new DummyProgramNode());
        interpreter.InterpretFunctionDeclaration(funcNode);

        Assert.True(interpreter.GlobalEnvironment.FunctionState.Lookup("TestFunc", out _));
    }
}