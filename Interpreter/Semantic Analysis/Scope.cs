


using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.NonTerminals;

namespace Interpreter.SemanticAnalysis;
public class Scope(Scope parentScope)
{
    Scope? ParentScope = parentScope;
    Dictionary<IdentifierNode, Node> VariableTable = [];
    Dictionary<IdentifierNode, TypeNode> TypeTable = [];


}