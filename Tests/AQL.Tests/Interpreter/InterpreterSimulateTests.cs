using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;

public class InterpreterSimulationTests
{
    /*
    [Fact]
            [Fact]
            public void InterpretSimulate_ShouldSetSimulationParameters()
            {
                // Arrange
                var simulate = new SimulateNode(
                    0,
                    new SingleIdentifierNode(0, "net"),
                    new IntLiteralNode(0, 5),
                    new IntLiteralNode(0, 100)
                );


                var interpreter = new InterpreterClass(new DummyProgramNode());

                // Act
                interpreter.InterpretSimulate(simulate);

                // Assert
                Assert.NotNull(interpreter.LastEngine);
                Assert.NotNull(interpreter.LastEngine.Stats);

                var stats = interpreter.LastEngine.Stats;

                Assert.NotNull(stats.QueueStats);
                Assert.Equal(5, stats.SimulationRunTimes.Count);
                Assert.All(stats.QueueStats, stat =>
                {
                    Assert.Equal(100, stat.SimulationTimePerRun);
                });
            }
        */
}