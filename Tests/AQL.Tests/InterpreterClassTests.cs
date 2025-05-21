using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Statements;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.Utilities.Modules;
using System.Collections.Generic;

public class InterpreterClassTests
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
    public void InterpretAddNode_ReturnsSumOfLiterals()
    {
        var left = new IntLiteralNode(0, 5);
        var right = new IntLiteralNode(0, 3);
        var addNode = new AddNode(0, left, right);

        var interpreter = new InterpreterClass(new DummyProgramNode());
        var result = interpreter.InterpretAddNode(addNode, null);

        Assert.IsType<double>(result);
        Assert.Equal(8.0, (double)result);

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

    [Fact]
    public void InterpretImport_ShouldSetError_WhenImportFails()
    {
        const string importNamespace = "ThisDoesNotExist";
        var root = new ImportNode(
            0,
            new SingleIdentifierNode(0, importNamespace),
            null
        );

        var interpreter = new InterpreterClass(root);
        interpreter.StartInterpretation();
        var environment = interpreter.GlobalEnvironment;

        Assert.True(environment.EncounteredError);
        Assert.False(string.IsNullOrWhiteSpace(environment.ErrorMessage));
    }
}

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
