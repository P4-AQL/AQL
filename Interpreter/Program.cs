



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
    InterpretationEnvironment interpretedModule = ModuleLoader.LoadModuleByName("input");

    string? path = Environment.CurrentDirectory.EndsWith("net9.0")
    ? new DirectoryInfo(Environment.CurrentDirectory).Parent?.Parent?.Parent?.FullName
    : Environment.CurrentDirectory;

    if (path is not null)
    {
        path = Path.Combine(path, "graphviz.dot");
        ASTGraph.GenerateDotFile(root: interpretedModule.Root, filePath: path);
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