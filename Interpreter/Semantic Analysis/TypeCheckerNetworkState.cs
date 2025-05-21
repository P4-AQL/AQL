


using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Networks;

namespace Interpreter.SemanticAnalysis;

public class TypeCheckerNetworkState(NetworkDeclarationNode networkNode, Table<Node> globalEnvironment)
{
    public NetworkDeclarationNode NetworkNode = networkNode;
    public Table<Node> localScope = new(globalEnvironment);
}