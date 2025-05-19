


using System.Diagnostics.CodeAnalysis;

namespace Interpreter.SemanticAnalysis;

public class Table<T>()
{
    // Identifier -> type
    private readonly Dictionary<string, T> Dictionary = [];

    public Table(Table<T> copy) : this()
    {
        Dictionary = new Dictionary<string, T>(copy.Dictionary);
    }

    public void ForceBind(string identifier, T @object) => Dictionary[identifier] = @object;


    public bool Lookup(string id, [MaybeNullWhen(false)] out T? @out)
    {
        bool found = Dictionary.TryGetValue(id, out T? value);
        @out = value;
        return found;
    }

    /// <summary>
    /// Binds a identifier to the type table, if it does not exist.
    /// </summary>
    /// <param name="identifier">The identifier to bind.</param>
    /// <param name="object">The object to bind.</param>
    /// <returns>True if the variable does not already exist; Otherwise false.</returns>
    public bool TryBindIfNotExists(string identifier, T @object)
    {
        //Bind failed - variable already exists
        if (Lookup(identifier, out T? _)) return false;

        //Bind succeeded
        ForceBind(identifier, @object);
        return true;

    }
}