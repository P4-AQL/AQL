using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Types;
using Interpreter.AST.Nodes.Metrics;
using System.Collections.Generic;
using Interpreter.AST.Nodes.Routes;



public class InterpreterQueueDeclarationTests
{
    [Fact]
    public void InterpretQueueDeclaration_AddsQueueToManager()
    {
        var networkId = new SingleIdentifierNode(0, "net");
        var interpreter = new InterpreterClass(new DummyProgramNode());

        var networkDecl = new NetworkDeclarationNode(
            0,
            new NetworkTypeNode(0, new SingleIdentifierNode(0, "net")),
            new SingleIdentifierNode(0, "net"),
            new List<SingleIdentifierNode>(),
            new List<SingleIdentifierNode>(),
            new List<InstanceDeclaration>(),
            new List<RouteDefinitionNode>(),
            new List<NamedMetricNode>()
        );
        interpreter.InterpretNetworkDeclaration(networkDecl);

        var queueId = new SingleIdentifierNode(0, "q1");
        var servers = new IntLiteralNode(0, 2);
        var capacity = new IntLiteralNode(0, 5);
        var dist = new IntLiteralNode(0, 42);

        var queueNode = new QueueDeclarationNode(
            0,
            new NetworkTypeNode(0, networkId),
            queueId,
            servers,
            capacity,
            dist,
            new List<NamedMetricNode>()
        );

        interpreter.InterpretQueueDeclaration(queueNode);

        var flat = interpreter.QueueableManager.FindQueueable("q1");
        Assert.NotNull(flat);
        Assert.Equal("q1", flat.Name);
    }
}
