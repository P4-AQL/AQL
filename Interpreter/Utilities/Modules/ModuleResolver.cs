


using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Utilities.Modules;
public static class ModuleResolver
{
    #region Search Path Lazy Initialization
    private static Lazy<List<string>> SearchPaths => new(GetSearchPaths);

    private static List<string> GetSearchPaths()
    {
        List<string> searchPaths = [];

        // Look for modules in the current directory and the parent directory
        searchPaths.Add(Environment.CurrentDirectory);

        // Look for modules in the standard library directory
        string environmentVariableValue = Environment.GetEnvironmentVariable("AQL_STANDARD_LIB") ?? string.Empty;

        if (string.IsNullOrEmpty(environmentVariableValue) == false)
        {
            // Split path by the appropriate separator for the OS
            char separator = Path.PathSeparator;

            string[] environmentVariablePaths = environmentVariableValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            searchPaths.AddRange(environmentVariablePaths);
        }

        return searchPaths;
    }
    #endregion

    /// <summary>
    /// Resolves the path of a module given its name. Prioritizing local libraries over builtin libraries.
    /// </summary>
    /// <param name="moduleName">The name of the module to resolve.</param>
    /// <returns>
    /// The path to the module if found; otherwise, null.
    /// </returns>
    public static bool ResolveModule(string moduleName, [MaybeNullWhen(false)] out string modulePath)
    {
        if (string.IsNullOrEmpty(moduleName))
        {
            modulePath = null;
            return false;
        }

        List<string> searchPaths = SearchPaths.Value;
        foreach (string searchPath in searchPaths)
        {
            string moduleSearchPath = Path.Combine(searchPath, moduleName + ".aql");

            if (File.Exists(moduleSearchPath))
            {
                modulePath = moduleSearchPath;
                return true;
            }
        }

        // If the module is not found in the search paths, return null
        throw new FileNotFoundException($"Module '{moduleName}' not found in search paths: {string.Join(", ", searchPaths)}");
    }

}