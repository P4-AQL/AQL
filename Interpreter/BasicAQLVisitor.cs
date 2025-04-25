



using Antlr4.Runtime.Misc;

public class BasicAQLVisitor : AQLBaseVisitor<object?>
{
    Dictionary<string, object> Variables { get; } = [];

    public override object? VisitAssign([NotNull] AQLParser.AssignContext context)
    {
        string variableName = context.ID().GetText();

        object? value = Visit(context.expr());
        if (value is null)
        {
            // Error handling: value is null
            return null;
        }

        if (Variables.ContainsKey(variableName))
        {
            Variables[variableName] = value;
        }
        else
        {
            // Error handling: variable not declared
        }

        return null;
    }
}