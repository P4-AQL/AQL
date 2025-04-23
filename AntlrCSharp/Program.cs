



using Antlr4.Runtime;

try
{
    string input = "";

    AntlrInputStream inputStream = new(input);
    AQLLexer lexer = new(inputStream);
    CommonTokenStream commonTokenStream = new(lexer);
    AQLParser parser = new(commonTokenStream);

    AQLParser.ProgContext progContext = parser.prog();
    BasicAQLVisitor visitor = new();
    object result = visitor.Visit(progContext);

    Console.WriteLine(result.ToString());
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
