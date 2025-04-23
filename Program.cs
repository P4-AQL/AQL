using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        AntlrFileStream inputStream = new("input.txt");
        AQLLexer lexer = new(inputStream);
        CommonTokenStream tokens = new(lexer);
        AQLParser parser = new(tokens);

        var tree = parser.Program();

        System.Console.WriteLine(tree.ToStringTree(parser));
    }
}
