using System.Diagnostics.CodeAnalysis;
using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public class Environment<T>
{
    // Identifier -> type
    private readonly Dictionary<string, T> Table = [];
    private void Bind(string identifier, T @object) => Table.Add(identifier, @object);


    public bool LookUp(string id, [MaybeNullWhen(false)] out T @out) => Table.TryGetValue(id, out @out);

    /// <summary>
    /// Binds a identifier to the type table, if it does not exist.
    /// </summary>
    /// <param name="identifier">The identifier to bind.</param>
    /// <param name="object">The object to bind.</param>
    /// <returns>True if the variable does not already exist; Otherwise false.</returns>
    public bool TryBindIfNotExists(string identifier, T @object)
    {
        //Bind failed - variable already exists
        if (LookUp(identifier, out T? _)) return false;

        //Bind succeeded
        Bind(identifier, @object);
        return true;

    }
}