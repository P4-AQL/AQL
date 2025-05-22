using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using System.Collections.Generic;
using Interpreter.AST.Nodes.NonTerminals;


public class InterpreterArrayAndIndexingTests
{
    [Fact]
    public void InterpretArrayNode_EvaluatesElements()
    {
        var elements = new List<ExpressionNode>
        {
            new IntLiteralNode(0, 1),
            new IntLiteralNode(0, 2)
        };

        var node = new ArrayLiteralNode(0, elements);
        var result = new InterpreterClass(new DummyProgramNode()).InterpretArrayNode(node, null);

        Assert.Equal(new object[] { 1, 2 }, result);
    }

    [Fact]
    public void InterpretIndexingNode_ReturnsCorrectElement()
    {
        var shadow = new Table<object>();
        shadow.ForceBind("arr", new object[] { 1, 2, 3 });

        var node = new IndexingNode(0, new SingleIdentifierNode(0, "arr"), new IntLiteralNode(0, 1));
        var result = new InterpreterClass(new DummyProgramNode()).InterpretIndexingNode(node, shadow);

        Assert.Equal(2, result);
    }
}
