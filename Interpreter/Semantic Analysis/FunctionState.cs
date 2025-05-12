


using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public readonly struct FunctionState(FunctionNode function, Environment<object> variableState)
{
    public FunctionNode Function { get; } = function;
    public Environment<object> VariableState { get; } = variableState;
}