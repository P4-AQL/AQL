using Interpreter.AST.Nodes;

namespace Interpreter.SemanticAnalysis;

public class TypeCheckerEnvironment
{
    // E
    public Table<Node> Environment => _environment;
    readonly Table<Node> _environment = new();
    public Table<Node> ConstEnvironment => _constEnvironment;
    readonly Table<Node> _constEnvironment = new();

    // Gamma
    public Table<TypeCheckerNetworkState> LocalNetworkScopesEnvironment => _localNetworkScopesEnvironment;
    readonly Table<TypeCheckerNetworkState> _localNetworkScopesEnvironment = new();

    public Table<TypeCheckerEnvironment> Dependencies => _dependencies;
    readonly Table<TypeCheckerEnvironment> _dependencies = new();
}