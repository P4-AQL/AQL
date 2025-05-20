


using Antlr4.Runtime;

namespace AQL.Tests;

public class ParserTests
{
    protected virtual AQLParser SetupParser(string input, bool customErrorListener = false)
    {
        AntlrInputStream inputStream = new(input);
        AQLLexer lexer = new(inputStream);
        CommonTokenStream commonTokenStream = new(lexer);
        AQLParser parser = new(commonTokenStream);

        if (customErrorListener)
        {
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ConsoleErrorListener<IToken>());
        }

        return parser;
    }

    public class ParseProgram : ParserTests
    {
        [Fact]
        public void EmptyProgram_ShouldParse()
        {
            string input = string.Empty;

            AQLParser parser = SetupParser(input);
            var result = parser.program();
            Assert.NotNull(result);
        }

        [Fact]
        public void EmptyProgramWithOneLineComment_ShouldParse()
        {
            string input = "// This is a comment";

            AQLParser parser = SetupParser(input);
            var result = parser.program();
            Assert.NotNull(result);
        }

        [Fact]
        public void EmptyProgramWithMultiLineComment_ShouldParse()
        {
            string input =
            @"
                /* This is a comment
                This is the second line */
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.program();
            Assert.NotNull(result);
        }

        [Fact]
        public void EmptyProgramIgnoresWhitespace_ShouldParse()
        {
            string input = "  \t\n  \t \r  \n";

            AQLParser parser = SetupParser(input);
            var result = parser.program();
            Assert.NotNull(result);
        }

        [Fact]
        public void ImportStatements_ShouldParse()
        {
            string input =
            @"
                import test
                import test2
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.program();
            Assert.NotNull(result);
            var importStatement = result.importStatement();
            Assert.NotNull(importStatement);
            var nestedProgram = importStatement.program();
            Assert.NotNull(nestedProgram);
            var importStatement2 = nestedProgram.importStatement();
            Assert.NotNull(importStatement2);
        }


    }

    public class ParseDefinition : ParserTests
    {
        [Fact]
        public void FunctionDefinition_ShouldParse()
        {
            string input =
            @"
                function int add(int a, int b) 
                { 
                    return a + b; 
                }
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.definition();
            Assert.NotNull(result);
        }

        [Fact]
        public void ConstDefinition_ShouldParse()
        {
            string input =
            @"
                const double pi = 3.1415;
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.definition();
            Assert.NotNull(result);
        }

        [Fact]
        public void QueueDefinition_ShouldParse()
        {
            string input =
            @"
                queue myQueue 
                { 
                    servers: 2; 
                    service: 5; 
                    capacity: 10; 
                }
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.definition();
            Assert.NotNull(result);
        }

        [Fact]
        public void NetworkDefinition_ShouldParse()
        {
            string input =
            @"
                network myNet 
                {
                    input1, input2 | output;
                    existingNetwork: newInstance;
                    route -> node;
                    *throughput, avgNum*;
                }
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.definition();
            Assert.NotNull(result);
        }

        [Fact]
        public void SimulateDefinition_ShouldParse()
        {
            string input =
            @"
                simulate 
                { 
                    run: myNetwork; 
                    until: 100; 
                    times: 5; 
                }
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.definition();
            Assert.NotNull(result);
        }
    }

    public class ParseNetwork : ParserTests
    {
        [Fact]
        public void InputOutputExpression_ShouldParse()
        {
            string input =
            @"
                network test 
                { 
                    in1, in2 | out1; 
                }
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.networkDefinition();
            Assert.NotNull(result);
        }

        [Fact]
        public void InstanceExpression_ShouldParse()
        {
            string input =
            @"
                network test 
                { 
                    existing: new1, new2; 
                }
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.networkDefinition();
            Assert.NotNull(result);
        }

        [Fact]
        public void RouteExpression_ShouldParse()
        {
            string input =
            @"
                network test 
                { 
                    start -> [0.6 node1, 0.4 node2]; 
                }
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.networkDefinition();
            Assert.NotNull(result);
        }

        [Fact]
        public void MetricsExpression_ShouldParse()
        {
            string input =
            @"
                network test 
                { 
                    *mrt, utilization*; 
                }
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.networkDefinition();
            Assert.NotNull(result);
        }
    }

    public class ParseInsideFunction : ParserTests
    {
        protected override AQLParser SetupParser(string input, bool customErrorListener = false)
        {
            input =
            $@"
                function int NameOfFunction()
                {{
                    {input}
                    return 0;
                }}
            ";

            return base.SetupParser(input, customErrorListener);
        }
    }

    public class ParseStatements : ParseInsideFunction
    {

    }

    public class ParseExpression : ParseInsideFunction
    {
        [Fact]
        public void AdvancedArithmaticExpression_ShouldParse()
        {
            string input =
            @"
                int result = (5 + 3) * 2 / 4 - 1;
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            // Expression: (5 + 3) * 2 / 4 - 1
            // L: (5 + 3) * 2 / 4
            // R: 1
            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);
            {
                var @operator = expressionContext.binaryOperator();
                Assert.NotNull(@operator);
                var operatorType = @operator.subtractOperator();
                Assert.NotNull(operatorType);
            }

            // L: (5 + 3) * 2 / 4
            // LL: (5 + 3) * 2
            // LR: 4
            var expressionL = expressionContext.left;
            Assert.NotNull(expressionL);
            {
                var @operator = expressionL.binaryOperator();
                Assert.NotNull(@operator);
                var operatorType = @operator.divisionOperator();
                Assert.NotNull(operatorType);
            }

            // LL: (5 + 3) * 2
            // LLL: (5 + 3)
            // LLR: 2
            var expressionLL = expressionL.left;
            Assert.NotNull(expressionLL);
            {
                var @operator = expressionLL.binaryOperator();
                Assert.NotNull(@operator);
                var operatorType = @operator.multiplicationOperator();
                Assert.NotNull(operatorType);
            }

            // LLL: (5 + 3)
            var expressionLLL = expressionLL.left;
            Assert.NotNull(expressionLLL);
            {
                var unaryExpression = expressionLLL.unaryExpression();
                Assert.NotNull(unaryExpression);
                var parenthesesContext = unaryExpression.parenthesesExpression();
                Assert.NotNull(parenthesesContext);

                // Expression: 5 + 3
                // L: 5
                // R: 3
                var nestedExpressionContext = parenthesesContext.expression();
                Assert.NotNull(nestedExpressionContext);

                // L: 5
                var nestedExpressionL = nestedExpressionContext.left;
                Assert.NotNull(nestedExpressionL);
                {
                    var unaryContext = nestedExpressionL.unaryExpression();
                    Assert.NotNull(unaryContext);
                    var valueContext = unaryContext.value();
                    Assert.NotNull(valueContext);
                    var intContext = valueContext.@int();
                    Assert.NotNull(intContext);
                    var intValue = intContext.GetText();
                    Assert.Equal("5", intValue);
                }

                // R: 3
                var nestedExpressionR = nestedExpressionContext.right;
                Assert.NotNull(nestedExpressionR);
                {
                    var unaryContext = nestedExpressionR.unaryExpression();
                    Assert.NotNull(unaryContext);
                    var valueContext = unaryContext.value();
                    Assert.NotNull(valueContext);
                    var intContext = valueContext.@int();
                    Assert.NotNull(intContext);
                    var intValue = intContext.GetText();
                    Assert.Equal("3", intValue);
                }
            }

            // LLR: 2
            var expressionLLR = expressionLL.right;
            Assert.NotNull(expressionLLR);
            {
                var unaryContext = expressionLLR.unaryExpression();
                Assert.NotNull(unaryContext);
                var valueContext = unaryContext.value();
                Assert.NotNull(valueContext);
                var intContext = valueContext.@int();
                Assert.NotNull(intContext);
                var intValue = intContext.GetText();
                Assert.Equal("2", intValue);
            }

            // LR: 4
            var expressionLR = expressionL.right;
            Assert.NotNull(expressionLR);
            {
                var unaryContext = expressionLR.unaryExpression();
                Assert.NotNull(unaryContext);
                var valueContext = unaryContext.value();
                Assert.NotNull(valueContext);
                var intContext = valueContext.@int();
                Assert.NotNull(intContext);
                var intValue = intContext.GetText();
                Assert.Equal("4", intValue);
            }

            // R: 1
            var expressionR = expressionContext.right;
            Assert.NotNull(expressionR);
            {
                var unaryContext = expressionR.unaryExpression();
                Assert.NotNull(unaryContext);
                var valueContext = unaryContext.value();
                Assert.NotNull(valueContext);
                var intContext = valueContext.@int();
                Assert.NotNull(intContext);
                var intValue = intContext.GetText();
                Assert.Equal("1", intValue);
            }
        }

        [Fact]
        public void LessThan_ShouldParse()
        {
            string input =
            @"
                bool result = x < 5;
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.expression();
            Assert.NotNull(result);

            var @operator = result.binaryOperator();
            Assert.NotNull(@operator);
            var operatorType = @operator.lessThanOperator();
            Assert.NotNull(operatorType);
        }

        [Fact]
        public void GreaterThan_ShouldParse()
        {
            string input =
            @"
                bool result = x > 5;
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expression = assignContext.expression();
            Assert.NotNull(expression);

            var @operator = expression.binaryOperator();
            Assert.NotNull(@operator);
            var operatorType = @operator.greaterThanOperator();
            Assert.NotNull(operatorType);
        }

        [Fact]
        public void LessThanOrEqual_ShouldParse()
        {
            string input =
            @"
                bool result = x <= 5;
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);

            var @operator = expressionContext.binaryOperator();
            Assert.NotNull(@operator);
            var operatorType = @operator.lessThanOrEqualOperator();
            Assert.NotNull(operatorType);
        }

        [Fact]
        public void GreaterThanOrEqual_ShouldParse()
        {
            string input =
            @"
                bool result = x >= 5;
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);

            var @operator = expressionContext.binaryOperator();
            Assert.NotNull(@operator);
            var operatorType = @operator.greaterThanOrEqualOperator();
            Assert.NotNull(operatorType);
        }

        [Fact]
        public void Equal_ShouldParse()
        {
            string input =
            @"
                bool result = x == 5;
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);

            var @operator = expressionContext.binaryOperator();
            Assert.NotNull(@operator);
            var operatorType = @operator.equalOperator();
            Assert.NotNull(operatorType);
        }

        [Fact]
        public void NotEqual_ShouldParse()
        {
            string input =
            @"
                bool result = x != 5;
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);

            var @operator = expressionContext.binaryOperator();
            Assert.NotNull(@operator);
            var operatorType = @operator.inEqualOperator();
            Assert.NotNull(operatorType);
        }

        [Fact]
        public void LogicalAnd_ShouldParse()
        {
            string input =
            @"
                bool result = x && y;
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);

            var @operator = expressionContext.binaryOperator();
            Assert.NotNull(@operator);
            var operatorType = @operator.andOperator();
            Assert.NotNull(operatorType);
        }

        [Fact]
        public void LogicalOr_ShouldParse()
        {
            string input =
            @"
                bool result = x || y;
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);

            var @operator = expressionContext.binaryOperator();
            Assert.NotNull(@operator);
            var operatorType = @operator.orOperator();
            Assert.NotNull(operatorType);
        }

        [Fact]
        public void FunctionCall_ShouldParse()
        {
            string input =
            @"
                int result = calculateSum(1, 2, 3);
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);

            var unaryExpression = expressionContext.unaryExpression();
            Assert.NotNull(unaryExpression);

            var valueContext = unaryExpression.value();
            Assert.NotNull(valueContext);

            var functionCallContext = valueContext.functionCall();
            Assert.NotNull(functionCallContext);

            var functionNameContext = functionCallContext.anyIdentifier();
            Assert.NotNull(functionNameContext);
            var functionNameText = functionNameContext.GetText();
            Assert.Equal("calculateSum", functionNameText);

            var expressionListContext = functionCallContext.expressionList();
            Assert.NotNull(expressionListContext);

            var expressionContexts = expressionListContext.expression();
            Assert.Equal(3, expressionContexts.Length);
            Assert.NotNull(expressionContexts[0]);
            Assert.NotNull(expressionContexts[1]);
            Assert.NotNull(expressionContexts[2]);
        }

        [Fact]
        public void ArrayIndexing_ShouldParse()
        {
            string input =
            @"
                int result = arr[10 + index];
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);

            var unaryExpression = expressionContext.unaryExpression();
            Assert.NotNull(unaryExpression);

            var valueContext = unaryExpression.value();
            Assert.NotNull(valueContext);

            var arrayContext = valueContext.arrayIndexing();
            Assert.NotNull(arrayContext);
        }

        [Fact]
        public void ArrayCreation_ShouldParse()
        {
            string input =
            @"
                [int] arr = {1, 2, 3};
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableContext = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableContext);

            var assignContext = variableContext.assignStatement();
            Assert.NotNull(assignContext);

            var expressionContext = assignContext.expression();
            Assert.NotNull(expressionContext);

            var unaryExpression = expressionContext.unaryExpression();
            Assert.NotNull(unaryExpression);

            var valueContext = unaryExpression.value();
            Assert.NotNull(valueContext);

            var arrayCreationContext = valueContext.arrayInitialization();
            Assert.NotNull(arrayCreationContext);

            var expressionListContext = arrayCreationContext.expressionList();
            Assert.NotNull(expressionListContext);

            var expressionContexts = expressionListContext.expression();
            Assert.Equal(3, expressionContexts.Length);
            Assert.NotNull(expressionContexts[0]);
            Assert.NotNull(expressionContexts[1]);
            Assert.NotNull(expressionContexts[2]);
        }
    }

    public class ParseInsideNetwork : ParserTests
    {
        protected override AQLParser SetupParser(string input, bool customErrorListener = false)
        {
            input =
            $@"
                network nameOfNetwork
                {{
                    {input}
                }}
            ";

            return base.SetupParser(input, customErrorListener);
        }
    }

    public class ParseRoute : ParseInsideNetwork
    {
        [Fact]
        public void SimpleRoute_ShouldParse()
        {
            string input =
            @"
                source -> destination;
            ";

            AQLParser parser = SetupParser(input);
            var networkDefinitionContext = parser.networkDefinition();
            Assert.NotNull(networkDefinitionContext);

            var networkExpressionContexts = networkDefinitionContext.networkExpression();
            Assert.NotNull(networkExpressionContexts);
            Assert.Single(networkExpressionContexts);

            var routesContext = networkExpressionContexts[0].routes();
            Assert.NotNull(routesContext);

            var routesIdContext = routesContext.routesId();
            Assert.NotNull(routesIdContext);

            var anyIdentifierContexts = routesIdContext.anyIdentifier();
            Assert.Equal(2, anyIdentifierContexts.Length);

            var sourceContext = anyIdentifierContexts[0];
            Assert.NotNull(sourceContext);
            var sourceText = sourceContext.GetText();
            Assert.Equal("source", sourceText);

            var destinationContext = anyIdentifierContexts[1];
            Assert.NotNull(destinationContext);
            var destinationText = destinationContext.GetText();
            Assert.Equal("destination", destinationText);
        }

        [Fact]
        public void ProbabilityRoute_ShouldParse()
        {
            string input =
            @"
                A -> [0.3 B, 0.7 C];
            ";

            AQLParser parser = SetupParser(input);
            var networkDefinitionContext = parser.networkDefinition();
            Assert.NotNull(networkDefinitionContext);

            var networkExpressionContexts = networkDefinitionContext.networkExpression();
            Assert.NotNull(networkExpressionContexts);
            Assert.Single(networkExpressionContexts);

            var routesContext = networkExpressionContexts[0].routes();
            Assert.NotNull(routesContext);

            var routesIdContext = routesContext.routesId();
            Assert.NotNull(routesIdContext);

            var anyIdentifierContexts = routesIdContext.anyIdentifier();
            Assert.Single(anyIdentifierContexts);

            var sourceContext = anyIdentifierContexts[0];
            Assert.NotNull(sourceContext);
            var sourceText = sourceContext.GetText();
            Assert.Equal("A", sourceText);

            var probabilityContext = routesIdContext.probabilityIdList();
            Assert.NotNull(probabilityContext);

            var expressionContexts = probabilityContext.expression();
            Assert.Equal(2, expressionContexts.Length);

            var expressionLContext = expressionContexts[0];
            Assert.NotNull(expressionLContext);
            var expressionLText = expressionLContext.GetText();
            Assert.Equal("0.3", expressionLText);

            var expressionRContext = expressionContexts[1];
            Assert.NotNull(expressionRContext);
            var expressionRText = expressionRContext.GetText();
            Assert.Equal("0.7", expressionRText);

            var identifierContexts = probabilityContext.anyIdentifier();
            Assert.Equal(2, identifierContexts.Length);

            var identifierLContext = identifierContexts[0];
            Assert.NotNull(identifierLContext);
            var identifierLText = identifierLContext.GetText();
            Assert.Equal("B", identifierLText);

            var identifierRContext = identifierContexts[1];
            Assert.NotNull(identifierRContext);
            var identifierRText = identifierRContext.GetText();
            Assert.Equal("C", identifierRText);
        }

        [Fact]
        public void ChainedRoute_ShouldParse()
        {
            string input =
            @"
                A -> B -> C -> D;
            ";

            AQLParser parser = SetupParser(input);
            var networkDefinitionContext = parser.networkDefinition();
            Assert.NotNull(networkDefinitionContext);

            var networkExpressionContexts = networkDefinitionContext.networkExpression();
            Assert.NotNull(networkExpressionContexts);
            Assert.Single(networkExpressionContexts);

            // A -> ...
            var routesContext = networkExpressionContexts[0].routes();
            Assert.NotNull(routesContext);

            var routesIdContext = routesContext.routesId();
            Assert.NotNull(routesIdContext);

            var anyIdentifierContexts = routesIdContext.anyIdentifier();
            Assert.Single(anyIdentifierContexts);

            var sourceContext = anyIdentifierContexts[0];
            Assert.NotNull(sourceContext);
            var sourceText = sourceContext.GetText();
            Assert.Equal("A", sourceText);

            // B -> ...
            var nestedRoutesIdContext1 = routesIdContext.routesId();
            Assert.NotNull(nestedRoutesIdContext1);

            var nestedAnyIdentifierContexts1 = nestedRoutesIdContext1.anyIdentifier();
            Assert.Single(nestedAnyIdentifierContexts1);

            sourceContext = nestedAnyIdentifierContexts1[0];
            Assert.NotNull(sourceContext);
            sourceText = sourceContext.GetText();
            Assert.Equal("B", sourceText);

            // C -> D
            var nestedRoutesIdContext2 = nestedRoutesIdContext1.routesId();
            Assert.NotNull(nestedRoutesIdContext2);

            var nestedAnyIdentifierContexts2 = nestedRoutesIdContext2.anyIdentifier();
            Assert.Equal(2, nestedAnyIdentifierContexts2.Length);

            sourceContext = nestedAnyIdentifierContexts2[0];
            Assert.NotNull(sourceContext);
            sourceText = sourceContext.GetText();
            Assert.Equal("C", sourceText);

            var destinationContext = nestedAnyIdentifierContexts2[1];
            Assert.NotNull(destinationContext);
            var destinationText = destinationContext.GetText();
            Assert.Equal("D", destinationText);
        }
    }
}