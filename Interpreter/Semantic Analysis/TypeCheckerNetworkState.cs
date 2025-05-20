


using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Networks;

namespace Interpreter.SemanticAnalysis;

public class TypeCheckerNetworkState(NetworkDeclarationNode networkNode)
{
    public NetworkDeclarationNode NetworkNode = networkNode;
    public Table<Node> localScope = new();
}