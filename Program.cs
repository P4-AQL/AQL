using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        var inputStream = new AntlrFileStream("input.txt");
        var lexer = new AQLLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new AQLParser(tokens);

        // Start parsing at the top-level rule (e.g., 'prog')
        var tree = parser.prog();

        // Print the parse tree
        System.Console.WriteLine(tree.ToStringTree(parser));
    }
}
