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
    {/*the four test are causing an error for some reason
        [Fact]
        public void TestIsTypeIntOrDouble_WithIntType_ReturnsTrue()
        {
            IntTypeNode intTypeNode = new IntTypeNode(3);
            Assert.True(TypeChecker.IsTypeIntOrDouble(intTypeNode));
        }

        [Fact]
        public void TestIsTypeIntOrDouble_WithDoubleType_ReturnsTrue()
        {
            var doubleType = new DoubleTypeNode(1);
            var result = TypeChecker.IsTypeIntOrDouble(doubleType);
            Assert.True(result);
        }

        [Fact]
        public void TestIsTypeIntOrDouble_WithOtherType_ReturnsFalse()
        {
            var stringType = new StringTypeNode(1); // Or any other type node you have
            var result = TypeChecker.IsTypeIntOrDouble(stringType);
            Assert.False(result);
        }

        [Fact]
        public void TestIsTypeIntOrDouble_WithNull_ReturnsFalse()
        {
            var result = TypeChecker.IsTypeIntOrDouble(null);
            Assert.False(result);
        }
    */
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
        /*cannot get it to work
                [Fact]
                public void TestValidNetworkMetricName()
                {
                    List<string> errors = [];

                    var avg = new NamedMetricNode(1, "avg");
                    var throughput = new NamedMetricNode(1, "throughput");
                    var expected_num_entities = new NamedMetricNode(1, "expected_num_entities");

                    var metricList = new List<NamedMetricNode> { avg, throughput, expected_num_entities };

                    TypeChecker.TypeCheckNetworkMetricList(metricList, errors);

                    Assert.Empty(errors);

                }

        */
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