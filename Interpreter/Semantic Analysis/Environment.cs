using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public class Environment
{
    // Identifier -> type
    private readonly Dictionary<string, TypeNode> Table = [];
    private void Bind(string identifier, TypeNode nodeType) => Table.Add(identifier, nodeType);

    public bool LookUp(string id, out TypeNode? @out) => Table.TryGetValue(id, out @out);

    // return true means success
    public bool TryBind(string identifier, TypeNode typeNode) {
        //Bind failed
        if (LookUp(identifier, out TypeNode? _)) return false;
        
        //Bind succeeded
        Bind(identifier, typeNode);
        return true;

    }
}