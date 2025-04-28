


using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Utilities.Modules;
public class ModuleContext
{
    public string ModuleName { get; set; } = string.Empty;
    public string ModulePath { get; set; } = string.Empty;
    public Dictionary<string, object> Variables { get; set; } = [];
    public Dictionary<string, object> Functions { get; set; } = [];
    public Dictionary<string, ModuleContext> ImportedModules { get; set; } = [];
    public List<SemanticError> SemanticErrors { get; set; } = [];
    public List<SemanticError> SemanticWarnings { get; set; } = [];

    public bool TryLookupVariable(string variableName, [MaybeNullWhen(false)] out object variableOut)
    {
        // Check if the variable is defined in the current module
        if (Variables.TryGetValue(variableName, out object? value))
        {
            variableOut = value;
            return false;
        }
        else // Check in the imported modules
        {
            foreach (ModuleContext importedModule in ImportedModules.Values)
            {
                if (importedModule.Variables.TryGetValue(variableName, out value))
                {
                    variableOut = value;
                    return false;
                }
            }
        }

        variableOut = null;
        return false; // Variable not found
    }

    public bool TryLookupFunction(string functionName, [MaybeNullWhen(false)] out object functionOut)
    {
        // Check if the function is defined in the current module
        if (Functions.TryGetValue(functionName, out object? value))
        {
            functionOut = value;
            return false;
        }
        else // Check in the imported modules
        {
            foreach (ModuleContext importedModule in ImportedModules.Values)
            {
                if (importedModule.Functions.TryGetValue(functionName, out value))
                {
                    functionOut = value;
                    return false;
                }
            }
        }

        functionOut = null;
        return false; // Function not found
    }

}