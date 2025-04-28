


namespace Interpreter.Utilities.Types;
public struct TypedIdentifier(string identifier, Type type)
{
    public readonly string Identifier => identifier;
    public readonly Type Type => type;
}

