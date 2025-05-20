



using System.Data;
using Antlr4.Runtime;
using Interpreter.AST;
using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.SemanticAnalysis;
using Interpreter.Utilities.Modules;
using Interpreter.Visitors;

try
{
    ProgramNode astRoot = ModuleLoader.LoadModuleByName("input");
    TypeChecker typeChecker = new();
    List<string> errors = [];
    typeChecker.TypeCheckNode(astRoot, errors);
    foreach (string error in errors.Distinct()) // Distinct for no duplicates
    {
        Console.WriteLine(error);
    }

    if (errors.Count == 0)
    {
        InterpreterClass interpreter = new(astRoot);
        interpreter.StartInterpretation();
    }

    string? path = Environment.CurrentDirectory.EndsWith("net9.0")
    ? new DirectoryInfo(Environment.CurrentDirectory).Parent?.Parent?.Parent?.FullName
    : Environment.CurrentDirectory;

    if (path is not null)
    {
        path = Path.Combine(path, "graphviz.dot");
        ASTGraph.GenerateDotFile(root: astRoot, filePath: path);
    }
    else
    {
        throw new NoNullAllowedException("path is null");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error {ex}");
}