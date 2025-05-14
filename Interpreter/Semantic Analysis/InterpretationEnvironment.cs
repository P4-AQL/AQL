


using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public struct InterpretationEnvironment
{
    public readonly Table<FunctionStateTuple> FunctionState => _functionState;
    Table<FunctionStateTuple> _functionState;
    public readonly Table<object> VariableState => _variableState;
    Table<object> _variableState;
    public readonly Table<NetworkDeclarationNode> NetworkState => _networkState;
    Table<NetworkDeclarationNode> _networkState;
    public readonly List<string> Errors => _errors;
    List<string> _errors;

    public readonly List<InterpretationEnvironment> ModuleDependencies => _moduleDependencies;
    List<InterpretationEnvironment> _moduleDependencies;

    public ProgramNode Root;


    public static InterpretationEnvironment Empty(ProgramNode root) => new()
    {
        _functionState = new(),
        _variableState = new(),
        _networkState = new(),
        _errors = [],

        _moduleDependencies = [],

        Root = root,
    };
}