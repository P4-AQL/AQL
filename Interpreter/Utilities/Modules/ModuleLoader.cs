


using Antlr4.Runtime;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.SemanticAnalysis;
using Interpreter.Visitors;

namespace Interpreter.Utilities.Modules;

public static class ModuleLoader
{
    /// <summary>
    /// Loads a module from the specified path.
    /// </summary>
    /// <param name="modulePath">The path to the module.</param>
    /// <returns> The loaded module context.</returns>
    public static InterpretationEnvironment LoadModuleByPath(string modulePath)
    {
        if (string.IsNullOrEmpty(modulePath))
        {
            throw new ArgumentException("Module path cannot be null or empty.", nameof(modulePath));
        }

        if (File.Exists(modulePath) == false)
        {
            throw new FileNotFoundException($"Module not found: {modulePath}");
        }

        string moduleContent = File.ReadAllText(modulePath);

        AntlrInputStream inputStream = new(moduleContent);
        AQLLexer lexer = new(inputStream);
        CommonTokenStream commonTokenStream = new(lexer);
        AQLParser parser = new(commonTokenStream);

        AQLParser.ProgramEOFContext progContext = parser.programEOF();

        ASTAQLVisitor visitor = new();
        ProgramNode result = visitor.VisitProgramEOF(progContext);

        TypeChecker typeChecker = new();
        List<string> errors = [];
        typeChecker.TypeCheckNode(result, errors);
        foreach (string error in errors)
        {
            Console.WriteLine(error);
        }

        InterpreterClass interpreter = new(result);
        InterpretationEnvironment environment = interpreter.StartInterpretation();

        return environment;
    }

    /// <summary>
    /// Loads a module by its name.
    /// </summary>
    /// <param name="moduleName">The name of the module.</param>
    /// <returns>The loaded module context.</returns>
    public static InterpretationEnvironment LoadModuleByName(string moduleName)
    {
        if (string.IsNullOrEmpty(moduleName))
        {
            throw new ArgumentException("Module name cannot be null or empty.", nameof(moduleName));
        }

        if (ModuleResolver.ResolveModule(moduleName, out string? modulePath) == false)
        {
            throw new FileNotFoundException($"Module not found: {moduleName}");
        }

        return LoadModuleByPath(modulePath);
    }
}