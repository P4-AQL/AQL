


using System.Text;
using Interpreter.AST.Nodes;

namespace Interpreter.AST;
public class ASTGraph
{

    public static void GenerateDotFile(Node root, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("node [shape=circle, style=filled, fillcolor=lightblue];");
        BuildDot(root, sb);
        sb.AppendLine("}");
        File.WriteAllText(filePath, sb.ToString());
    }

    static void BuildDot(Node node, StringBuilder sb, int? parentId = null)
    {
        int nodeId = Guid.NewGuid().GetHashCode();
        sb.AppendLine($"  {nodeId} [label=\"{node.GetNodeLabel()}\"];");

        if (parentId is not null)
        {
            sb.AppendLine($"  {parentId} -> {nodeId};");
        }

        foreach (var child in node.Children())
        {
            BuildDot(child, sb, nodeId);
        }
    }

    // Then render with: dot -Tpng tree.dot -o tree.png
}