using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public class Environment
{
    // Identifier -> type
    private readonly Dictionary<string, TypeNode> TypeTable = [];
    Dictionary<string, FunctionNode> FunctionTable { get; } = [];

    private void Bind<T>(string identifier, T @object, Dictionary<string, T> table) => table.Add(identifier, @object);
    private void Bind(string identifier, TypeNode nodeType) => Bind(identifier, nodeType, TypeTable);
    private void Bind(string identifier, FunctionNode nodeFunction) => Bind(identifier, nodeFunction, FunctionTable);


    public bool LookUp(string id, out TypeNode? @out) => TypeTable.TryGetValue(id, out @out);

    /// <summary>
    /// Binds a identifier to the type table, if it does not exist.
    /// </summary>
    /// <param name="identifier">The identifier to bind.</param>
    /// <param name="typeNode">The type to bind.</param>
    /// <returns>True if the variable does not already exist; Otherwise false.</returns>
    public bool TryBind(string identifier, TypeNode typeNode)
    {
        //Bind failed - variable already exists
        if (LookUp(identifier, out TypeNode? _)) return false;

        //Bind succeeded
        Bind(identifier, typeNode);
        return true;

    }
}