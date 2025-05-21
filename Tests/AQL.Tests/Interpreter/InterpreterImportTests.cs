using Xunit;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Identifiers;

public class InterpreterImportTests
{
    [Fact]
    public void InterpretImport_ShouldSetError_WhenImportFails()
    {
        const string importNamespace = "ThisDoesNotExist";
        var root = new ImportNode(0, new SingleIdentifierNode(0, importNamespace), null);

        var interpreter = new InterpreterClass(root);
        interpreter.StartInterpretation();

        var environment = interpreter.GlobalEnvironment;

        Assert.True(environment.EncounteredError);
        Assert.False(string.IsNullOrWhiteSpace(environment.ErrorMessage));
    }
}
