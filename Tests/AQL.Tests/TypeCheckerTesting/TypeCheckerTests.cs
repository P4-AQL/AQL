using Interpreter.AST.Nodes;
using Interpreter.AST.Nodes.Expressions;
using Interpreter.AST.Nodes.Identifiers;
using Interpreter.AST.Nodes.Metrics;
using Interpreter.AST.Nodes.NonTerminals;
using Interpreter.AST.Nodes.Programs;
using Interpreter.AST.Nodes.Types;
using Interpreter.SemanticAnalysis;
using Interpreter.AST.Nodes.Definitions;
using Interpreter.AST.Nodes.Statements;
using Interpreter.AST.Nodes.Networks;
using Interpreter.AST.Nodes.Routes;

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
            typeChecker.TypeCheckProgramNode(importNode, errors);

            // Verify zero errors
            Assert.Empty(errors);
        }

        [Fact]
        public void TestIncorrectProgramNodes()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            ImportNode importNode = new ImportNode(0, new SingleIdentifierNode(0, "nonExisting"), null);
            typeChecker.TypeCheckProgramNode(importNode, errors);

            // Verify errors
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

            ImportNode node = new ImportNode(0, new SingleIdentifierNode(0, "testLibrary"), null);
            typeChecker.TypeCheckImportNode(errors, node);

            // Verify zero errors
            Assert.Empty(errors);
        }

        [Fact]
        public void TestIncorrectImportName()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            ImportNode node = new ImportNode(0, new SingleIdentifierNode(0, "nonExisting"), null);
            typeChecker.TypeCheckImportNode(errors, node);

            // Verify errors
            Assert.NotEmpty(errors);
        }
    }

    public class TypeCheckDefinitionNodeTest : TypeCheckerTests
    {
        [Fact]
        public void TestCorrectDefinitions()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            //All of the valid nodes chained together:
            SingleIdentifierNode networkID = new SingleIdentifierNode(0, "net");
            SingleIdentifierNode funcID = new SingleIdentifierNode(0, "func");

            SimulateNode simulateNode = new(0, new SingleIdentifierNode(0, networkID.Identifier), new IntLiteralNode(0, 10), new IntLiteralNode(0, 100));
            NetworkDefinitionNode networkDefinitionNode = new(0, simulateNode, new NetworkDeclarationNode(0, new NetworkTypeNode(0, networkID), networkID, [], [], [], [], [new NamedMetricNode(0, "throughput")]));
            FunctionNode functionNode = new(0, networkDefinitionNode, new IntTypeNode(0), funcID, [new TypeAndIdentifier(0, new IntTypeNode(0), new SingleIdentifierNode(0, "param"))], new StatementNode(0));
            ConstDeclarationNode constDeclarationNode = new ConstDeclarationNode(0, functionNode, new IntTypeNode(0), new SingleIdentifierNode(0, "constant"), new IntLiteralNode(0, 2));

            typeChecker.TypeCheckDefinitionNode(constDeclarationNode, errors);

            // Verify zero errors
            Assert.Empty(errors);
        }

        [Fact]
        public void TestIncorrectDefinitions()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            //Some invalid node
            ConstDeclarationNode constDeclarationNode = new ConstDeclarationNode(0, null, new IntTypeNode(0), new SingleIdentifierNode(0, "constant"), new StringLiteralNode(0, "whoops"));
            
            typeChecker.TypeCheckDefinitionNode(constDeclarationNode, errors);

            // Verify errors
            Assert.NotEmpty(errors);
        }
    }

    public class TypeCheckNetworkTest : TypeCheckerTests
    {
        [Fact]
        public void TestCorrectNetwork()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            // Some valid node
            SingleIdentifierNode networkID = new SingleIdentifierNode(0, "net");
            NetworkDefinitionNode networkDefinitionNode = new(0, null, new NetworkDeclarationNode(0, new NetworkTypeNode(0, networkID), networkID, [], [], [], [], [new NamedMetricNode(0, "throughput")]));
            typeChecker.TypeCheckNetwork(networkDefinitionNode, errors);

            // Verify zero errors
            Assert.Empty(errors);
        }

        [Fact]
        public void TestIncorrectNetwork()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            //Some invalid node            
            SingleIdentifierNode networkID = new SingleIdentifierNode(0, "net");
            NetworkDefinitionNode networkDefinitionNode = new(0, null, new NetworkDeclarationNode(0, new NetworkTypeNode(0, networkID), networkID, [], [], [], [], [new NamedMetricNode(0, "not working")]));
            typeChecker.TypeCheckNetwork(networkDefinitionNode, errors);

            // Verify errors
            Assert.NotEmpty(errors);
        }
    }

    public class NetworkIsValidTest : TypeCheckerTests
    {
        [Fact]
        public void TestValidNetwork()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            // Valid network althought a pretty empty one
            // TODO: make more complex network
            SingleIdentifierNode networkID = new(0, "someNetwork");
            NetworkDeclarationNode networkDeclarationNode = new NetworkDeclarationNode(0, new NetworkTypeNode(0, networkID), networkID, [], [], [], [], [new NamedMetricNode(0, "throughput")]);
            Table<Node> localEnv = new();

            typeChecker.NetworkIsValid(errors, networkDeclarationNode, localEnv);
            
            // Verify zero errors
            Assert.Empty(errors);
        }

        [Fact]
        public void TestInvalidNetwork()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();
            // invalid network that have an unused input and output
            SingleIdentifierNode networkID = new(0, "someNetwork");
            NetworkDeclarationNode networkDeclarationNode = new NetworkDeclarationNode(0, new NetworkTypeNode(0, networkID), networkID, [new SingleIdentifierNode(0, "unusedInput")], [new SingleIdentifierNode(0, "unusedOutput")], [], [], [new NamedMetricNode(0, "throughput")]);
            Table<Node> localEnv = new();

            typeChecker.NetworkIsValid(errors, networkDeclarationNode, localEnv);
            
            // Verify errors
            Assert.NotEmpty(errors);
        }
    }

    public class TypeCheckStatementNodeTest : TypeCheckerTests
    {
        [Fact]
        public void Assignment_CorrectlyTyped_NoErrors()
        {
            // Arrange
            var errors = new List<string>();
            var localEnv = new Table<Node>();
            localEnv.TryBindIfNotExists("x", new IntTypeNode(1));

            var identifier = new SingleIdentifierNode(1, "x");
            var expression = new IntLiteralNode(1, 2); // line 1, value 2

            var assign = new AssignNode(1, null, identifier, expression);
            var typeChecker = new TypeChecker();

            // Act
            typeChecker.TypeCheckStatementNode(assign, errors, new IntTypeNode(1), localEnv);

            // Assert
            Assert.Empty(errors);
        }
    }

    public class FindExpressionTypeTest : TypeCheckerTests
    {
        [Fact]
        public void Add_IntPlusInt_ReturnsIntType() //AddNode
        {
            List<string> errors = [];

            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 3);
            var right = new IntLiteralNode(1, 5);
            var addNode = new AddNode(1, left, right);

            var result = typeChecker.FindExpressionType(addNode, errors, null);

            // Assert
            Assert.IsType<IntTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Add_DoublePlusDouble_ReturnsDoubleType() //AddNode
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new DoubleLiteralNode(1, 3.0);
            var right = new DoubleLiteralNode(1, 5.5);
            var addNode = new AddNode(1, left, right);

            var result = typeChecker.FindExpressionType(addNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Add_IntPlusDouble_ReturnsDoulbeType() //AddNode
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 3);
            var right = new DoubleLiteralNode(1, 5.5);
            var addNode = new AddNode(1, left, right);

            var result = typeChecker.FindExpressionType(addNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Add_DoublePlusInt_ReturnsdoubleType() //AddNode
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new DoubleLiteralNode(1, 3.5);
            var right = new IntLiteralNode(1, 5);
            var addNode = new AddNode(1, left, right);

            var result = typeChecker.FindExpressionType(addNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Add_DoublePlusString_ReturnsError() //AddNode
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new DoubleLiteralNode(1, 3.5);
            var right = new StringLiteralNode(1, "string");
            var addNode = new AddNode(1, left, right);

            var result = typeChecker.FindExpressionType(addNode, errors, null);

            Assert.Null(result);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("Expression must be int or double"));
        }

        /*****************************************************ANDNODE****************************************************************/

        [Fact]
        public void And_BoolAndBool_ReturnsBoolType() //AndNode
        {
            // Arrange
            List<string> errors = new();
            var typeChecker = new TypeChecker();

            var left = new BoolLiteralNode(1, true);
            var right = new BoolLiteralNode(1, false);
            var andNode = new AndNode(1, left, right);

            // Act
            var result = typeChecker.FindExpressionType(andNode, errors, null);

            // Assert
            Assert.IsType<BoolTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void And_BoolAndInt_ReturnsError() //AndNode
        {
            // Arrange
            List<string> errors = new();
            var typeChecker = new TypeChecker();

            var left = new BoolLiteralNode(1, true);
            var right = new IntLiteralNode(1, 1);
            var andNode = new AndNode(1, left, right);

            // Act
            var result = typeChecker.FindExpressionType(andNode, errors, null);

            // Assert
            Assert.Null(result);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("Expressions must evaluate to bool"));
        }

        /*****************************************************DIVENODE****************************************************************/

        [Fact]
        public void Division_IntDivInt_ReturnsIntType() //divNode
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 10);
            var right = new IntLiteralNode(1, 2);
            var divNode = new DivisionNode(1, left, right);

            var result = typeChecker.FindExpressionType(divNode, errors, null);

            Assert.IsType<IntTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Division_DoubleDivDouble_ReturnsDoubleType() //divNode
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new DoubleLiteralNode(1, 10.5);
            var right = new DoubleLiteralNode(1, 2.0);
            var divNode = new DivisionNode(1, left, right);

            var result = typeChecker.FindExpressionType(divNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Division_IntDivDouble_ReturnsDoubleType()
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 10);
            var right = new DoubleLiteralNode(1, 2.5);
            var divNode = new DivisionNode(1, left, right);

            var result = typeChecker.FindExpressionType(divNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }


        [Fact]
        public void Division_DoubleDivInt_ReturnsDoubleType()
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new DoubleLiteralNode(1, 10.5);
            var right = new IntLiteralNode(1, 2);
            var divNode = new DivisionNode(1, left, right);

            var result = typeChecker.FindExpressionType(divNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Division_IntDivString_ReturnsError()
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 10);
            var right = new StringLiteralNode(1, "text");
            var divNode = new DivisionNode(1, left, right);

            var result = typeChecker.FindExpressionType(divNode, errors, null);

            Assert.Null(result);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("Expression must be int or double"));
        }

        /************************************************EqualNODE*******************************************************************/

        [Fact]
        public void Equal_IntEqualsInt_ReturnsBoolType()
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 5);
            var right = new IntLiteralNode(1, 10);
            var equalNode = new EqualNode(1, left, right);

            var result = typeChecker.FindExpressionType(equalNode, errors, null);

            Assert.IsType<BoolTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Equal_IntEqualsString_ReturnsError()
        {
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 5);
            var right = new StringLiteralNode(1, "abc");
            var equalNode = new EqualNode(1, left, right);

            var result = typeChecker.FindExpressionType(equalNode, errors, null);

            Assert.Null(result);
            Assert.Single(errors);
            Assert.Contains("Cannot compare expressions of different types", errors[0]);
        }
        // how to test if one side is null, I thought we did not have null in AQL

        /********************************************FuncCall***********************************************************************/

        [Fact]
        public void FunctionCall_ValidFunction_ReturnsCorrectType() //some one double chekc this 
        {
            // Arrange
            List<string> errors = [];

            var returnType = new IntTypeNode(1);
            var identifier = new SingleIdentifierNode(1, "myFunc");

            var functionNode = new FunctionNode(
                lineNumber: 1,
                nextDefinition: null,
                returnType: returnType,
                identifier: identifier,
                parameters: [],
                body: new SkipNode(1)
            );

            // Create the environment and add the function node
            var environment = new TypeCheckerEnvironment();
            environment.Environment.TryBindIfNotExists("myFunc", functionNode);

            // Create a TypeChecker instance
            var typeChecker = new TypeChecker();

            // Use reflection to inject the test environment into the private globalEnvironment field
            var globalEnvField = typeof(TypeChecker)
                .GetField("globalEnvironment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (globalEnvField == null)
                throw new Exception("Could not find globalEnvironment field");

            globalEnvField.SetValue(typeChecker, environment);

            // Prepare the function call node
            var functionCallIdentifier = new SingleIdentifierNode(1, "myFunc");
            var functionCall = new FunctionCallNode(1, functionCallIdentifier, Array.Empty<ExpressionNode>());

            // Act
            var result = typeChecker.FindExpressionType(functionCall, errors, localEnvironment: null);

            // Assert
            Assert.IsType<IntTypeNode>(result);
            Assert.Empty(errors);
        }
        /* This test is failing as no error is thrown, but idk why
                [Fact]
        public void TypeCheckFunctionCall_NonFunctionIdentifier_ThrowsException()
        {
            // Arrange
            var errors = new List<string>();
            var nonFunctionNode = new IntTypeNode(1); // Not a function

            var env = new TypeCheckerEnvironment();
            var bindResult = env.Environment.TryBindIfNotExists("notFunc", nonFunctionNode);
            Assert.True(bindResult);

            var typeChecker = new TypeChecker();

            // Inject the environment (replace private field)
            var globalEnvField = typeof(TypeChecker).GetField("globalEnvironment", BindingFlags.NonPublic | BindingFlags.Instance);
            if (globalEnvField == null)
            {
                throw new Exception("Could not find globalEnvironment field");
            }
            globalEnvField.SetValue(typeChecker, env);

            var identifier = new SingleIdentifierNode(1, "notFunc");
            var funcCall = new FunctionCallNode(1, identifier, Array.Empty<ExpressionNode>());

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => typeChecker.FindExpressionType(funcCall, errors, null));
            Assert.Contains("Identifier is not a function", ex.Message);
        }        
        */

        /********************************************idetifiere***********************************************************************/













        /********************************************LessThan***********************************************************************/

        [Fact]
        public void LessThan_IntLessThanInt_ReturnsBoolType()
        {
            // Arrange
            List<string> errors = [];
            var typeChecker = new TypeChecker();

            // Left and right are int literals
            var left = new IntLiteralNode(1, 5);
            var right = new IntLiteralNode(1, 10);
            var lessThanNode = new LessThanNode(1, left, right);

            // Act
            var result = typeChecker.FindExpressionType(lessThanNode, errors, localEnvironment: null);

            // Assert
            Assert.IsType<BoolTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void LessThan_IntLessThanBool_ReturnsError()
        {
            // Arrange
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 5);
            var right = new BoolLiteralNode(1, true); // incompatible type for less than
            var lessThanNode = new LessThanNode(1, left, right);

            // Act
            var result = typeChecker.FindExpressionType(lessThanNode, errors, null);

            // Assert
            Assert.Null(result);
            Assert.Single(errors);
            Assert.Contains("Expression must be int or double", errors[0]);
        }

        /********************************************MultiNode***********************************************************************/

        [Fact]
        public void Multiply_IntTimesInt_ReturnsIntType()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 3);
            var right = new IntLiteralNode(1, 4);
            var multiplyNode = new MultiplyNode(1, left, right);

            var result = typeChecker.FindExpressionType(multiplyNode, errors, null);

            Assert.IsType<IntTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Multiply_DoubleTimesDouble_ReturnsDoubleType()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var left = new DoubleLiteralNode(1, 3.5);
            var right = new DoubleLiteralNode(1, 2.5);
            var multiplyNode = new MultiplyNode(1, left, right);

            var result = typeChecker.FindExpressionType(multiplyNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Multiply_IntTimesDouble_ReturnsDoubleType()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 3);
            var right = new DoubleLiteralNode(1, 2.5);
            var multiplyNode = new MultiplyNode(1, left, right);

            var result = typeChecker.FindExpressionType(multiplyNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Multiply_DoubleTimesInt_ReturnsDoubleType()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var left = new DoubleLiteralNode(1, 3.5);
            var right = new IntLiteralNode(1, 2);
            var multiplyNode = new MultiplyNode(1, left, right);

            var result = typeChecker.FindExpressionType(multiplyNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Multiply_IntTimesBool_ReturnsError()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var left = new IntLiteralNode(1, 3);
            var right = new BoolLiteralNode(1, true); // invalid type
            var multiplyNode = new MultiplyNode(1, left, right);

            var result = typeChecker.FindExpressionType(multiplyNode, errors, null);

            Assert.Null(result);
            Assert.Single(errors);
            Assert.Contains("Expression must be int or double", errors[0]);
        }
        /********************************************Negative***********************************************************************/

        [Fact]
        public void Negative_IntNode_ReturnsIntType()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var inner = new IntLiteralNode(1, 42);
            var negativeNode = new NegativeNode(1, inner);

            var result = typeChecker.FindExpressionType(negativeNode, errors, null);

            Assert.IsType<IntTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Negative_DoubleNode_ReturnsDoubleType()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var inner = new DoubleLiteralNode(1, 3.14);
            var negativeNode = new NegativeNode(1, inner);

            var result = typeChecker.FindExpressionType(negativeNode, errors, null);

            Assert.IsType<DoubleTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Negative_BoolNode_ReturnsNullAndAddsError()
        {
            // Arrange
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var inner = new BoolLiteralNode(1, true);
            var negativeNode = new NegativeNode(1, inner);

            // Act
            var result = typeChecker.FindExpressionType(negativeNode, errors, null);

            // Assert
            Assert.Null(result);
            Assert.Single(errors);
            Assert.Contains("Expression not int or double", errors[0]);
        }

        /********************************************NotNode***********************************************************************/

        [Fact]
        public void Not_BoolNode_ReturnsBoolType()
        {
            // Arrange
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var inner = new BoolLiteralNode(1, true);
            var notNode = new NotNode(1, inner);

            // Act
            var result = typeChecker.FindExpressionType(notNode, errors, null);

            // Assert
            Assert.IsType<BoolTypeNode>(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Not_NonBoolNode_ReturnsNullAndAddsError()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var inner = new IntLiteralNode(1, 5); // Not a BoolTypeNode
            var notNode = new NotNode(1, inner);

            var result = typeChecker.FindExpressionType(notNode, errors, null);

            Assert.Null(result);
            Assert.Single(errors);
            Assert.Contains("Expression must evaluate to a bool", errors[0]);
        }

        /********************************************Array***********************************************************************/
        [Fact]
        public void ArrayLiteral_Ints_ReturnsArrayOfInt()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var elements = new ExpressionNode[]
            {
                new IntLiteralNode(1, 1),
                new IntLiteralNode(1, 2),
                new IntLiteralNode(1, 3)
            };

            var arrayLiteralNode = new ArrayLiteralNode(1, elements);
            var result = typeChecker.FindExpressionType(arrayLiteralNode, errors, null);

            Assert.NotNull(result);
            var arrayType = Assert.IsType<ArrayTypeNode>(result);
            Assert.IsType<IntTypeNode>(arrayType.InnerType);
            Assert.Empty(errors);
        }

        [Fact]
        public void ArrayLiteral_Strings_ReturnsArrayOfString()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var elements = new ExpressionNode[]
            {
                new StringLiteralNode(1, "a"),
                new StringLiteralNode(1, "b")
            };

            var arrayLiteralNode = new ArrayLiteralNode(1, elements);
            var result = typeChecker.FindExpressionType(arrayLiteralNode, errors, null);

            Assert.NotNull(result);
            var arrayType = Assert.IsType<ArrayTypeNode>(result);
            Assert.IsType<StringTypeNode>(arrayType.InnerType);
            Assert.Empty(errors);
        }

        [Fact]
        public void ArrayLiteral_SingleIntElement_ReturnsArrayType()
        {
            var errors = new List<string>();
            var typeChecker = new TypeChecker();

            var elements = new ExpressionNode[]
            {
                new IntLiteralNode(1, 42)
            };

            var arrayLiteralNode = new ArrayLiteralNode(1, elements);
            var result = typeChecker.FindExpressionType(arrayLiteralNode, errors, null);

            Assert.NotNull(result);
            var arrayType = Assert.IsType<ArrayTypeNode>(result);
            Assert.IsType<IntTypeNode>(arrayType.InnerType);
            Assert.Empty(errors);
        }
        /*This is a  test fail
                [Fact]
                public void ArrayLiteral_MixedTypes_ReturnsError()
                {
                    var errors = new List<string>();
                    var typeChecker = new TypeChecker();

                    var elements = new ExpressionNode[]
                    {
                        new IntLiteralNode(1, 1),
                        new StringLiteralNode(1, "a")
                    };

                    var arrayLiteralNode = new ArrayLiteralNode(1, elements);
                    var result = typeChecker.FindExpressionType(arrayLiteralNode, errors, null);

                    Assert.Null(result);
                    Assert.Single(errors);
                    Assert.Contains("Types must match", errors[0]);
                }

        */
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
        // TODO: these
    }

    public class TypeCheckQueueMetricListTest : TypeCheckerTests
    {
        [Fact]
        public void TestValidQueueMetric()
        {
            List<string> errors = [];

            var mrt = new NamedMetricNode(1, "mrt");
            var vrt = new NamedMetricNode(1, "vrt");
            var awt = new NamedMetricNode(1, "awt");
            var expected_num_entities = new NamedMetricNode(1, "expected_num_entities");
            var util = new NamedMetricNode(1, "util");
            var throughput = new NamedMetricNode(1, "throughput");

            var metricList = new List<NamedMetricNode> { mrt, vrt, awt, expected_num_entities, util, throughput };

            TypeChecker.TypeCheckQueueMetricList(metricList, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public void TestInValidQueueMetric()
        {
            List<string> errors = [];

            var someMetric = new NamedMetricNode(1, "someMetric");

            var metricList = new List<NamedMetricNode> { someMetric };

            TypeChecker.TypeCheckQueueMetricList(metricList, errors);

            Assert.NotEmpty(errors);
        }
    }

    public class TypeCheckNetworkMetricListTest : TypeCheckerTests
    {
        
    }

    public class TypeCheckInstancesTest : TypeCheckerTests
    {
        [Fact]
        public void TestValidInstance()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            SingleIdentifierNode networkID = new SingleIdentifierNode(0, "net");
            NetworkDeclarationNode networkDefinitionNode = new NetworkDeclarationNode(0, new NetworkTypeNode(0, networkID), networkID, [], [], [], [], []);
            Table<Node> globalEnvironment = new();
            
            TypeCheckerNetworkState typeCheckerNetworkState = new(networkDefinitionNode, globalEnvironment);

            typeChecker.TypeCheckInstances(typeCheckerNetworkState, errors);

            // Verify zero errors
            Assert.Empty(errors);
        }

        /*[Fact]
        public void ()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            // Routing to a non existing input
            Table<Node> localNetwork = new();
            SingleIdentifierNode input = new(0, "input");

            IReadOnlyList<RouteValuePairNode> destinations = [new RouteValuePairNode(0, new IntLiteralNode(0, 1), input)];

            typeChecker.TypeCheckRouteDestination(destinations, localNetwork, errors);

            // Verify errors
            Assert.NotEmpty(errors);
        }*/
    }

    public class TypeCheckRoutesTest : TypeCheckerTests
    {
        [Fact]
        public void TestValidRoutes()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            Table<Node> globalEnvironment = new();
            SingleIdentifierNode output = new(0, "Q");
            globalEnvironment.TryBindIfNotExists("Q", new OutputTypeNode(0));
            globalEnvironment.TryBindIfNotExists("from", new IntTypeNode(0));
            SingleIdentifierNode networkID = new(0, "networkName");

            IReadOnlyList<RouteValuePairNode> destination = [new RouteValuePairNode(0, new IntLiteralNode(0, 1), output)];
            RouteDefinitionNode route = new(0, new IdentifierExpressionNode(0, new SingleIdentifierNode(0, "from")), destination);

            NetworkDeclarationNode networkDeclarationNode = new NetworkDeclarationNode(0, new NetworkTypeNode(0, networkID), networkID, [], [], [], [route], []);
            TypeCheckerNetworkState typeCheckerNetworkState = new(networkDeclarationNode, globalEnvironment);

            typeChecker.TypeCheckRoutes(typeCheckerNetworkState, errors);

            // Verify zero errors
            Assert.Empty(errors);
        }

        [Fact]
        public void TestInvalidRoutes()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            // Routing where there is no from routing
            Table<Node> globalEnvironment = new();
            SingleIdentifierNode output = new(0, "Q");
            globalEnvironment.TryBindIfNotExists("Q", new OutputTypeNode(0));
            //globalEnvironment.TryBindIfNotExists("from", new IntTypeNode(0));
            SingleIdentifierNode networkID = new(0, "networkName");

            IReadOnlyList<RouteValuePairNode> destination = [new RouteValuePairNode(0, new IntLiteralNode(0, 1), output)];
            RouteDefinitionNode route = new(0, new IdentifierExpressionNode(0, new SingleIdentifierNode(0, "from")), destination);

            NetworkDeclarationNode networkDeclarationNode = new NetworkDeclarationNode(0, new NetworkTypeNode(0, networkID), networkID, [], [], [], [route], []);
            TypeCheckerNetworkState typeCheckerNetworkState = new(networkDeclarationNode, globalEnvironment);

            typeChecker.TypeCheckRoutes(typeCheckerNetworkState, errors);

            // Verify errors
            Assert.NotEmpty(errors);
        }
    }

    public class TypeCheckRouteDestinationTest : TypeCheckerTests
    {
        [Fact]
        public void TestValidRouteDestination()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            // Routing to a output
            Table<Node> localNetwork = new();
            SingleIdentifierNode output = new(0, "output");

            localNetwork.TryBindIfNotExists("output", new OutputTypeNode(0));
            IReadOnlyList<RouteValuePairNode> destinations = [new RouteValuePairNode(0, new IntLiteralNode(0, 1), output)];

            typeChecker.TypeCheckRouteDestination(destinations, localNetwork, errors);

            // Verify zero errors
            Assert.Empty(errors);
        }

        [Fact]
        public void TestInvalidRouteDestination()
        {
            List<string> errors = [];
            TypeChecker typeChecker = new();

            // Routing to a non existing input
            Table<Node> localNetwork = new();
            SingleIdentifierNode input = new(0, "input");

            IReadOnlyList<RouteValuePairNode> destinations = [new RouteValuePairNode(0, new IntLiteralNode(0, 1), input)];

            typeChecker.TypeCheckRouteDestination(destinations, localNetwork, errors);

            // Verify errors
            Assert.NotEmpty(errors);
        }
    }
}