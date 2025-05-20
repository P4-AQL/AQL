


using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;

public struct InterpretationEnvironment
{
    public readonly bool EncounteredError => _encounteredError;
    bool _encounteredError;
    public readonly string ErrorMessage => _errorMessage;
    string _errorMessage;
    public readonly Table<FunctionStateTuple> FunctionState => _functionState;
    Table<FunctionStateTuple> _functionState;
    public readonly Table<object> VariableState => _variableState;
    Table<object> _variableState;
    public readonly Table<NetworkDeclarationNode> NetworkState => _networkState;
    Table<NetworkDeclarationNode> _networkState;
    public readonly QueueableManager QueueableManager => _queueableManager;
    QueueableManager _queueableManager;

    public readonly Table<InterpretationEnvironment> ModuleDependencies => _moduleDependencies;
    Table<InterpretationEnvironment> _moduleDependencies;

    public ProgramNode Root;

    public static InterpretationEnvironment Empty(ProgramNode root) => new()
    {
        _functionState = new(),
        _variableState = new(),
        _networkState = new(),
        _queueableManager = new(),

        _moduleDependencies = new(),

        Root = root,
    };

    public void SetError(string message)
    {
        _encounteredError = true;
        _errorMessage = message;
    }

}