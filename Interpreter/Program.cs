



using Interpreter.Utilities.Modules;

try
{
    ModuleContext context = ModuleLoader.LoadModuleByName("input");
}
catch (Exception ex)
{
    Console.WriteLine($"Error {ex}");
}
