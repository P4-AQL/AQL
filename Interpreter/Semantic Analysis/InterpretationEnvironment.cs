


using Interpreter.AST.Nodes.Networks;

namespace Interpreter.SemanticAnalysis;
public struct InterpretationEnvironment
{
    public Table<FunctionStateTuple> FunctionState;
    public Table<object> VariableState;
    public Table<NetworkDeclarationNode> NetworkState;

    public static InterpretationEnvironment Empty => new()
    {
        FunctionState = new(),
        VariableState = new(),
        NetworkState = new(),
    };
}