


namespace Interpreter.SemanticAnalysis;

public class ValueAndType(object value, Type type)
{
    public object Value { get; } = value;
    public Type Type { get; } = type;

    public T Convert<T>(T targetType) where T : Type
    {
        if (targetType == typeof(T))
        {
            return (T)Value;
        }

        throw new InvalidCastException($"Cannot convert {Value} of type {Type} to {targetType}.");
    }
}