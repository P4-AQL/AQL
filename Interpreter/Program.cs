



using System.Data;
using Antlr4.Runtime;
using Interpreter.AST;
using Interpreter.AST.Nodes;
using Interpreter.Utilities.Modules;
using Interpreter.Visitors;

try
{
    string content = ModuleLoader.LoadModuleByName("input");

    AntlrInputStream inputStream = new(content);
    AQLLexer lexer = new(inputStream);
    CommonTokenStream commonTokenStream = new(lexer);
    AQLParser parser = new(commonTokenStream);

    AQLParser.ProgramContext progContext = parser.program();

    Console.WriteLine(progContext.ToStringTree());

    ASTAQLVisitor visitor = new();
    Node result = visitor.VisitProgram(progContext);



    string? path = Environment.CurrentDirectory.EndsWith("net9.0") ? new DirectoryInfo(Environment.CurrentDirectory).Parent?.Parent?.Parent?.Parent?.FullName : Environment.CurrentDirectory;

    if (path is not null)
    {
        path = Path.Combine(path, "graphviz.txt");
        ASTGraph.GenerateDotFile(root: result, filePath: path);
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
