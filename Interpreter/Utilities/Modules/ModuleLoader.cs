


using Antlr4.Runtime;

namespace Interpreter.Utilities.Modules;
public static class ModuleLoader
{
    /// <summary>
    /// Loads a module from the specified path.
    /// </summary>
    /// <param name="modulePath">The path to the module.</param>
    /// <returns> The loaded module context.</returns>
    public static string /*ModuleContext*/ LoadModuleByPath(string modulePath)
    {
        if (string.IsNullOrEmpty(modulePath))
        {
            throw new ArgumentException("Module path cannot be null or empty.", nameof(modulePath));
        }

        if (File.Exists(modulePath) == false)
        {
            throw new FileNotFoundException($"Module not found: {modulePath}");
        }

        /*string moduleContent =*/
        return File.ReadAllText(modulePath);
        /*
        AntlrInputStream inputStream = new(moduleContent);
        AQLLexer lexer = new(inputStream);
        CommonTokenStream commonTokenStream = new(lexer);
        AQLParser parser = new(commonTokenStream);

        AQLParser.ProgramContext progContext = parser.program();

        BasicAQLVisitor visitor = new();
        object? result = visitor.Visit(progContext);

        if (result is ModuleContext moduleContext)
        {
            moduleContext.ModuleName = Path.GetFileNameWithoutExtension(modulePath);
            moduleContext.ModulePath = modulePath;
            return moduleContext;
        }
        else
        {
            throw new InvalidOperationException("Failed to load module");
        }*/
    }

    /// <summary>
    /// Loads a module by its name.
    /// </summary>
    /// <param name="moduleName">The name of the module.</param>
    /// <returns>The loaded module context.</returns>
    public static string /*ModuleContext*/ LoadModuleByName(string moduleName)
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