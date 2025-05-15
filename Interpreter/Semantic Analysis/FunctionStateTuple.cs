


using Interpreter.AST.Nodes.Definitions;

namespace Interpreter.SemanticAnalysis;
public readonly struct FunctionStateTuple(FunctionNode function, Table<object> variableState)
{
    public FunctionNode Function { get; } = function;
    public Table<object> VariableState { get; } = variableState;
}