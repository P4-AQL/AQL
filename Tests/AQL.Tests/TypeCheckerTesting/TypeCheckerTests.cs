using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Types;
using Interpreter.SemanticAnalysis;

namespace AQL.Tests;

public class TypeCheckerTests
{
    public class TypeCheckNodeTest : TypeCheckerTests
    {
        [Fact]
        public void TestCorrectRootNode()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            Node root = new DefinitionProgramNode(0, new DefinitionNode(0));
            typeChecker.TypeCheckNode(root, errors);

            // Verify 0 errors when typechecker gets a program node
            Assert.Empty(errors);
        }

        [Fact]
        public void TestIncorrectRootNode()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            Node root = new StatementNode(0);
            typeChecker.TypeCheckNode(root, errors);

            // Verify errors when typechecker gets something that is not a program node
            Assert.NotEmpty(errors);
        }
    }

    public class TypeCheckProgramNodeTest : TypeCheckerTests
    {
        [Fact]
        public void TestCorrectProgramNodes()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            // Test import node which have a definition node chained to it
            DefinitionProgramNode definitionNode = new DefinitionProgramNode(0, new DefinitionNode(0));
            // Test library is a file which can be found at bin/Debug/net9.0
            ImportNode importNode = new ImportNode(0, new SingleIdentifierNode(0, "testLibrary"), definitionNode);
            typeChecker.TypeCheckNode(importNode, errors);

            // Verify zero errors
            Assert.Empty(errors);
        }

        [Fact]
        public void TestIncorrectProgramNodes()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            Node root = new StatementNode(0);
            typeChecker.TypeCheckNode(root, errors);

            // Verify zero errors
            Assert.NotEmpty(errors);
        }
    }

    public class TypeCheckImportNodeTest : TypeCheckerTests
    {
        [Fact]
        public void TestCorrectImportName()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            IntTypeNode node = new IntTypeNode(0);
            typeChecker.TypeCheckNode(node, errors);

            // Verify zero errors
            Assert.NotEmpty(errors);
        }

    }

    public class TypeCheckDefinitionNodeTest : TypeCheckerTests
    {

    }

    public class TypeCheckNetworkTest : TypeCheckerTests
    {

    }

    public class NetworkIsValidTest : TypeCheckerTests
    {

    }

    public class TypeCheckStatementNodeTest : TypeCheckerTests
    {

    }

    public class FindExpressionTypeTest : TypeCheckerTests
    {

    }

    public class IsTypeIntOrDoubleTest : TypeCheckerTests
    {

    }

    public class GetTypeFromIdentifierTest : TypeCheckerTests
    {

    }

    public class TypeCheckInputsTest : TypeCheckerTests
    {

    }

    public class TypeCheckOutputsTest : TypeCheckerTests
    {

    }

    public class TypeCheckQueueMetricListTest : TypeCheckerTests
    {

    }

    public class TypeCheckNetworkMetricListTest : TypeCheckerTests
    {

    }

    public class TypeCheckInstancesTest : TypeCheckerTests
    {

    }

    public class TypeCheckRoutesTest : TypeCheckerTests
    {

    }

    public class TypeCheckRouteDestinationTest : TypeCheckerTests
    {

    }
}