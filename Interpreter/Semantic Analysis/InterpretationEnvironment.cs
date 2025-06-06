


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
    public readonly NetworkDefinitionManager NetworkDeclarationManager => _networkDeclarationManager;
    NetworkDefinitionManager _networkDeclarationManager;

    public readonly Table<InterpretationEnvironment> ModuleDependencies => _moduleDependencies;
    Table<InterpretationEnvironment> _moduleDependencies;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public readonly TimeSpan Duration => EndTime - StartTime;

    public ProgramNode Root;

    public static InterpretationEnvironment Empty(ProgramNode root)
    {
        InterpretationEnvironment emptyEnv = new()
        {
            _functionState = new(),
            _variableState = new(),
            _networkState = new(),

            _moduleDependencies = new(),

            Root = root,
        };
        emptyEnv._networkDeclarationManager = new NetworkDefinitionManager(emptyEnv);

        return emptyEnv;
    }

    public void SetError(string message)
    {
        _encounteredError = true;
        _errorMessage = message;
    }

}