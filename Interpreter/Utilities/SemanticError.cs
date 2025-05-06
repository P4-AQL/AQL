


namespace Interpreter.Utilities;
public class SemanticError
{
    public string Message { get; }
    public int Line { get; }
    public int Column { get; }

    public SemanticError(string message, int line, int column)
    {
        Message = message;
        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        return $"Error: {Message} at line {Line} : {Column}";
    }
}