


using System.Diagnostics.CodeAnalysis;

public static class Extensions
{
    public static bool TryParseToPrimitiveType(this string typeString, [MaybeNullWhen(false)] out Type type)
    {
        if (typeString == "int")
        {
            type = typeof(int);
            return true;
        }
        else if (typeString == "double")
        {
            type = typeof(double);
            return true;
        }
        else if (typeString == "string")
        {
            type = typeof(string);
            return true;
        }
        else if (typeString == "bool")
        {
            type = typeof(bool);
            return true;
        }
        else
        {
            type = null;
            return false;
        }
    }
}