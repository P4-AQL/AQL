



using Antlr4.Runtime;

try
{
    string input = File.ReadAllText("input.AQL");

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
    Console.WriteLine($"Error {ex}");
}
