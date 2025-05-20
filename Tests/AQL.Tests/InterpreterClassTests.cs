


using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;
using Interpreter.SemanticAnalysis;

namespace AQL.Tests;

public class InterpreterClassTests
{
    /*[Fact]
    public void InterpretImport_ShouldAddEnvironmentToModuleDependencies()
    {
        
        // Arrange
        const string importNamespace = "stdlib";
        var root = new ImportNode(
            0,
            new(
                0,
                importNamespace
            ),
            null
        );
        var interpreter = new InterpreterClass(root);

        // Act
        interpreter.InterpretImport(root);
        var environment = interpreter.GlobalEnvironment;

        // Assert
        environment.ModuleDependencies.Lookup(importNamespace, out var foundEnvironment);
#pragma warning disable xUnit2002 // Do not use null check on value type (for some reason the compiler thinks it can't be null)
        Assert.NotNull(foundEnvironment);
#pragma warning restore xUnit2002 // Do not use null check on value type
        
    }*/

    [Fact]
    public void InterpretImport_ShouldSetError_WhenImportFails()
    {
        // Arrange
        const string importNamespace = "ThisDoesNotExist";
        var root = new ImportNode(
            0,
            new(
                0,
                importNamespace
            ),
            null
        );
        var interpreter = new InterpreterClass(root);

        // Act
        interpreter.StartInterpretation();
        var environment = interpreter.GlobalEnvironment;

        // Assert
        Assert.True(environment.EncounteredError);
    }
}