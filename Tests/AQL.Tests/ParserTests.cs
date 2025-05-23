


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
        public void IncorrectEmptyProgramWithMultiLineComment_ShouldParse()
        {
            string input =
            @"
                network hahaha hahaha 12
                * This is a comment
                This is the second line */
            ";

            AQLParser parser = SetupParser(input);
            var result = parser.program();
            Assert.NotNull(result);

            
            
            var parsed = result.GetText();
            Assert.NotEqual("", parsed);
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

            var funcDef = parser.definition();
            Assert.NotNull(funcDef);
            
            var functionContext = funcDef.functionDefinition();
            Assert.NotNull(functionContext);

            var returnType = functionContext.type();
            Assert.NotNull(returnType);
            Assert.Equal("int", returnType.GetText());
            
            var identifier = functionContext.identifier();
            Assert.NotNull(identifier);
            Assert.Equal("add", identifier.GetText());

            var paramIdentifiers = functionContext.formalParameterList().identifier();
            var paramTypes = functionContext.formalParameterList().type();
            Assert.NotNull(paramIdentifiers);
            Assert.NotNull(paramTypes);
            Assert.Equal("a", paramIdentifiers[0].GetText());
            Assert.Equal("b", paramIdentifiers[1].GetText());
            Assert.Equal("int", paramTypes[0].GetText());
            Assert.Equal("int", paramTypes[1].GetText());


        }

        [Fact]
        public void ConstDefinition_ShouldParse()
        {
            string input =
            @"
                const double pi = 3.1415;
            ";
            AQLParser parser = SetupParser(input);
            var def = parser.definition();
            Assert.NotNull(def);

            var constDef = def.constDefinition();
            Assert.NotNull(constDef);

            var type = constDef.type();
            Assert.NotNull(type);
            Assert.Equal("double", type.GetText());

            var assignStatement = constDef.assignStatement();
            Assert.NotNull(assignStatement);

            var identifier = assignStatement.identifier();
            Assert.NotNull(identifier);
            Assert.Equal("pi", identifier.GetText());


            var expression = assignStatement.expression();
            Assert.NotNull(expression);

            var number = expression.unaryExpression();
            Assert.NotNull(number);
            Assert.Equal("3.1415", number.GetText());
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

            var def = parser.definition();
            Assert.NotNull(def);

            var networks = def.networks();
            Assert.NotNull(networks);

            var queueDef = networks.queueDefinition();
            Assert.NotNull(queueDef);

            var identifier = queueDef.identifier();
            Assert.NotNull(identifier);
            Assert.Equal("myQueue", identifier.GetText());

            var num_servers = queueDef.numberOfServers;
            Assert.NotNull(num_servers);
            Assert.Equal("2", num_servers.GetText());

            var service = queueDef.service;
            Assert.NotNull(service);
            Assert.Equal("5", service.GetText());

            var capacity = queueDef.capacity;
            Assert.NotNull(capacity);
            Assert.Equal("10", capacity.GetText());
        }


        [Fact]
        public void NestedNetwork_ShouldParse()
        {
            string input =
            @"
                queue ParkingLot {
                servers: 100;
                service: 300;
                capacity: 100;
                *mrt, vrt, throughput*
                }
                queue BeforeCheckIn {
                servers: 2;
                service: 90;
                capacity: 10222220;
                *mrt, vrt, throughput*
                }
                queue CheckIn {
                servers: 1;
                service: 20;
                capacity: 10222220;
                *mrt, vrt, throughput*
                } 
                queue AfterCheckIn {
                servers: 4;
                service: 90;
                capacity: 10222220;
                *mrt, vrt, throughput*
                }
                queue SecurityA {
                servers: 1;
                service: 120;
                capacity: 10222220;
                *mrt, vrt, throughput*
                }
                queue SecurityB {
                servers: 1;
                service: 240;
                capacity: 10222220;
                *mrt, vrt, throughput*
                }
                queue GateA {
                servers: 1;
                service: 10;
                capacity: 10222220;
                *mrt, vrt, throughput*
                }
                queue GateB {
                servers: 1;
                service: 30;
                capacity: 10222220;
                *mrt, vrt, throughput*
                }
                network ArrivalArea {
                main_parking_entrance | path_to_sec_b, path_to_after_check_in, path_to_sec_a1, path_to_sec_a2;
                CheckIn : CheckInA, CheckInB;
                main_parking_entrance -> ParkingLot -> [0.49 CheckInA, 0.49 CheckInB, 0.02 BeforeCheckIn];
                BeforeCheckIn -> [0.005 BeforeCheckIn, 0.4975 CheckInA, 0.4975 CheckInB];
                CheckInA -> [0.20 path_to_sec_b, 0.10 path_to_after_check_in, 0.35 path_to_sec_a1, 0.35 path_to_sec_a2];
                CheckInB -> [0.20 path_to_sec_b, 0.10 path_to_after_check_in, 0.35 path_to_sec_a1, 0.35 path_to_sec_a2];
                }

                network ProcessingArea {
                arrival_sec_b, arrival_after_check_in, arrival_sec_a1, arrival_sec_a2 | flight;
                SecurityA : SecurityA1, SecurityA2;
                GateA : GateA1, GateA2, GateA3, GateA4;
                arrival_sec_b -> SecurityB -> [0.99 GateB, 0.01 AfterCheckIn];
                arrival_after_check_in -> AfterCheckIn -> [0.22 SecurityB, 0.38 SecurityA1, 0.38 SecurityA2, 0.02 AfterCheckIn];
                arrival_sec_a1 -> SecurityA1 -> [0.25 GateA1, 0.25 GateA2, 0.25 GateA3, 0.25 GateA4];
                arrival_sec_a2 -> SecurityA2 -> [0.25 GateA1, 0.25 GateA2, 0.25 GateA3, 0.25 GateA4];
                GateB -> flight;
                GateA1 -> flight;
                GateA2 -> flight;
                GateA3 -> flight;
                GateA4 -> flight;
                }

                network Airport {
                arrival | plane_go_zumzum;
                10 -> arrival -> ArrivalArea.main_parking_entrance;
                ArrivalArea.path_to_sec_b -> ProcessingArea.arrival_sec_b;
                ArrivalArea.path_to_after_check_in -> ProcessingArea.arrival_after_check_in;
                ArrivalArea.path_to_sec_a1 -> ProcessingArea.arrival_sec_a1;
                ArrivalArea.path_to_sec_a2 -> ProcessingArea.arrival_sec_a2;
                ProcessingArea.flight -> plane_go_zumzum;
                }
            ";

            AQLParser parser = SetupParser(input);

            var def = parser.definition();
         

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

            var def = parser.definition();
            Assert.NotNull(def);
            var networks = def.networks();
            Assert.NotNull(networks);
            var networkDef = networks.networkDefinition();
            Assert.NotNull(networkDef);
            var identifier = networkDef.identifier();
            Assert.NotNull(identifier);
            Assert.Equal("myNet", identifier.GetText());
            var networkExpression = networkDef.networkExpression();
            Assert.NotNull(networkExpression);
            Assert.Equal(4, networkExpression.Length);
            var inputOutputContext = networkExpression[0].inputOutputNetworkExpression();
            Assert.NotNull(inputOutputContext);
            var inputs = inputOutputContext.inputs;
            Assert.NotNull(inputs);
            var inputids = inputs.identifier();
            Assert.NotNull(inputids);
            Assert.Equal(2, inputids.Length);
            Assert.Equal("input1", inputids[0].GetText());
            Assert.Equal("input2", inputids[1].GetText());
            var output = inputOutputContext.outputs;
            Assert.NotNull(output);
            var outputids = output.identifier();
            Assert.NotNull(outputids);
            Assert.Single(outputids);
            Assert.Equal("output", outputids[0].GetText());

            var instanceContext = networkExpression[1].instanceNetworkExpression();
            Assert.NotNull(instanceContext);
            var instanceIdentifier = instanceContext.anyIdentifier();
            Assert.NotNull(instanceIdentifier);
            Assert.Equal("existingNetwork", instanceIdentifier.GetText());
            var instanceIdList = instanceContext.idList().GetText().Split(",");
            Assert.NotNull(instanceIdList);
            Assert.Single(instanceIdList);
            Assert.Equal("newInstance", instanceIdList[0].Trim());

            var routeContext = networkExpression[2].routes();
            Assert.NotNull(routeContext);
            var routesId = routeContext.routesId();
            Assert.NotNull(routesId);
            var routeIdentifier = routesId.anyIdentifier();
            Assert.NotNull(routeIdentifier);
            
            Assert.Equal("route", routeIdentifier[0].GetText());

            var routeDest = routesId.anyIdentifier()[1].identifier();
            Assert.NotNull(routeDest);
            Assert.Equal("node", routeDest.GetText());
            

            var metricsContext = networkExpression[3].metrics;
            Assert.NotNull(metricsContext);
            var metrics = metricsContext().metric();
            Assert.NotNull(metrics);
            
            Assert.Equal("throughput", metrics[0].GetText());
            Assert.Equal("avgNum", metrics[1].GetText());
            Assert.Equal(2, metrics.Length);


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

            var def = parser.definition();
            Assert.NotNull(def);

            var simulateDef = def.simulateDefinition();
            Assert.NotNull(simulateDef);

            var network = simulateDef.network;
            Assert.NotNull(network);

            var terminationCriteria = simulateDef.terminationCriteria;
            Assert.NotNull(terminationCriteria);

            var runs = simulateDef.runs;
            Assert.NotNull(runs);

            Assert.Equal("myNetwork", network.identifier().GetText());
            Assert.Equal("100", terminationCriteria.unaryExpression().GetText());
            Assert.Equal("5", runs.unaryExpression().GetText());

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
            var networkDef = parser.networkDefinition();
            Assert.NotNull(networkDef);
            var networkExpression = networkDef.networkExpression();
            Assert.NotNull(networkExpression);
            var inputOutputContext = networkExpression[0].inputOutputNetworkExpression();
            Assert.NotNull(inputOutputContext);
            
            var inputs = inputOutputContext.idList()[0].GetText().Split(",");
            Assert.NotNull(inputs);
            Assert.Equal(2, inputs.Length);
            Assert.Equal("in1", inputs[0].Trim());
            Assert.Equal("in2", inputs[1].Trim());
        }

        [Fact]
        public void InstanceExpression_ShouldParse()
        {
            string input =
            @"
                network test 
                { 
                    existing : new1, new2
                }
            ";
            AQLParser parser = SetupParser(input);
            var networkDef = parser.networkDefinition();
            Assert.NotNull(networkDef);
            var networkExpression = networkDef.networkExpression();
            Assert.NotNull(networkExpression);
            var instanceContext = networkExpression[0].instanceNetworkExpression();
            Assert.NotNull(instanceContext);
            var instanceIdentifier = instanceContext.anyIdentifier();
            Assert.NotNull(instanceIdentifier);
            
            var instanceIdList = instanceContext.idList().GetText().Split(",");
            Assert.NotNull(instanceIdList);
            Assert.Equal(2, instanceIdList.Length);
            Assert.Equal("new1", instanceIdList[0].Trim());
            Assert.Equal("new2", instanceIdList[1].Trim());


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

            var networkDef = parser.networkDefinition();
            Assert.NotNull(networkDef);

            var networkExpression = networkDef.networkExpression();
            Assert.NotNull(networkExpression);
            var routeContext = networkExpression[0].routes();
            Assert.NotNull(routeContext);
            var routesId = routeContext.routesId();
            Assert.NotNull(routesId);
            var routeSource = routesId.anyIdentifier();
            Assert.NotNull(routeSource);
            Assert.Equal("start", routeSource[0].GetText());

            var routeDest = routesId.probabilityIdList();
            Assert.NotNull(routeDest);

            var destIds = routeDest.anyIdentifier();
            var destProbs = routeDest.expression();
            Assert.NotNull(destIds);
            Assert.NotNull(destProbs);
            Assert.Equal(2, destIds.Length);
            Assert.Equal(2, destProbs.Length);
            Assert.Equal("node1", destIds[0].GetText());
            Assert.Equal("node2", destIds[1].GetText());
            Assert.Equal("0.6", destProbs[0].GetText());
            Assert.Equal("0.4", destProbs[1].GetText());

            
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

            var networkDef = parser.networkDefinition();
            Assert.NotNull(networkDef);

            var networkExpression = networkDef.networkExpression();
            Assert.NotNull(networkExpression);

            var metricsContext = networkExpression[0].metrics;
            Assert.NotNull(metricsContext);

            var metrics = metricsContext().metric();
            Assert.NotNull(metrics);
            Assert.Equal(2, metrics.Length);
            Assert.Equal("mrt", metrics[0].GetText());
            Assert.Equal("utilization", metrics[1].GetText());

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
                }}
            ";

            return base.SetupParser(input, customErrorListener);
        }



        [Fact]
        public void EmptyFunction_ShouldParse()
        {
            string input = string.Empty;

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();



            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);
        }

        [Fact]
        public void EmptyFunctionWithOneLineComment_ShouldParse()
        {
            string input = "// This is a comment";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);
        }


        [Fact]
        public void EmptyFunctionWithMultiLineComment_ShouldParse()
        {
            string input =
            @"
                /* This is a comment
                This is the second line */
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);
        }


        [Fact]
        public void EmptyFunctionIgnoresWhitespace_ShouldParse()
        {
            string input = "  \t\n  \t \r  \n";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);
        }


        [Fact]
        public void SummationFunction_ShouldParse()
        {
            string input =
            @"
                int a = 2;
                double b = 3;
                double result = a + b;
                return result;
            ";

            AQLParser parser = SetupParser(input);

            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);


            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableDeclarationStatement = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableDeclarationStatement);
            var type1 = variableDeclarationStatement.type();
            Assert.NotNull(type1);
            Assert.Equal("int", type1.GetText());
            var assignStatement = variableDeclarationStatement.assignStatement();
            Assert.NotNull(assignStatement);
            var identifier1 = assignStatement.identifier();
            Assert.NotNull(identifier1);
            Assert.Equal("a", identifier1.GetText());
            var expression1 = assignStatement.expression();
            Assert.NotNull(expression1);
            var unaryExpression1 = expression1.unaryExpression();
            Assert.NotNull(unaryExpression1);
            Assert.Equal("2", unaryExpression1.GetText());

            var statement2 = assignStatement.nextStatement;
            Assert.NotNull(statement2);
            var variableDeclarationStatement2 = statement2.variableDeclarationStatement();
            Assert.NotNull(variableDeclarationStatement2);
            var type2 = variableDeclarationStatement2.type();
            Assert.NotNull(type2);
            Assert.Equal("double", type2.GetText());
            var assignStatement2 = variableDeclarationStatement2.assignStatement();
            Assert.NotNull(assignStatement2);
            var identifier2 = assignStatement2.identifier();
            Assert.NotNull(identifier2);
            Assert.Equal("b", identifier2.GetText());
            var expression2 = assignStatement2.expression();
            Assert.NotNull(expression2);
            var unaryExpression2 = expression2.unaryExpression();
            Assert.NotNull(unaryExpression2);
            Assert.Equal("3", unaryExpression2.GetText());


            var statement3 = assignStatement2.nextStatement;
            Assert.NotNull(statement3);
            var variableDeclarationStatement3 = statement3.variableDeclarationStatement();
            Assert.NotNull(variableDeclarationStatement3);
            var type3 = variableDeclarationStatement3.type();
            Assert.NotNull(type3);
            Assert.Equal("double", type3.GetText());
            var assignStatement3 = variableDeclarationStatement3.assignStatement();
            Assert.NotNull(assignStatement3);
            var identifier3 = assignStatement3.identifier();
            Assert.NotNull(identifier3);
            Assert.Equal("result", identifier3.GetText());
            var expression3 = assignStatement3.expression();
            Assert.NotNull(expression3);
            var binaryOperator = expression3.binaryOperator();
            Assert.NotNull(binaryOperator);
            var operatorType = binaryOperator.addOperator();
            Assert.NotNull(operatorType);
            var leftExpression = expression3.left;
            Assert.NotNull(leftExpression);
            var unaryExpression3 = leftExpression.unaryExpression();
            Assert.NotNull(unaryExpression3);
            Assert.Equal("a", unaryExpression3.GetText());
            var rightExpression = expression3.right;
            Assert.NotNull(rightExpression);
            var unaryExpression4 = rightExpression.unaryExpression();
            Assert.NotNull(unaryExpression4);
            Assert.Equal("b", unaryExpression4.GetText());

            var statement4 = assignStatement3.nextStatement;
            Assert.NotNull(statement4);
            var returnStatement = statement4.returnStatement();
            Assert.NotNull(returnStatement);

        }

        [Fact]
        public void WhileDoStatement_ShouldParse()
        {
            string input =
            @"
                int i = 0;
                while (i < 10) do
                { 
                    i = i + 1; 
                }
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableDeclarationStatement = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableDeclarationStatement);
            var type = variableDeclarationStatement.type();
            Assert.NotNull(type);
            Assert.Equal("int", type.GetText());
            var assignStatement1 = variableDeclarationStatement.assignStatement();
            Assert.NotNull(assignStatement1);
            var identifier1 = assignStatement1.identifier();
            Assert.NotNull(identifier1);
            Assert.Equal("i", identifier1.GetText());
            var expression1 = assignStatement1.expression();
            Assert.NotNull(expression1);
            var unaryExpression1 = expression1.unaryExpression();
            Assert.NotNull(unaryExpression1);
            Assert.Equal("0", unaryExpression1.GetText());
            var statement2 = assignStatement1.nextStatement;
            Assert.NotNull(statement2);
            var whileStatement = statement2.whileStatement();
            Assert.NotNull(whileStatement);

            var conditionWithPar = whileStatement.condition;
            Assert.NotNull(conditionWithPar);
            var condition = conditionWithPar.unaryExpression().parenthesesExpression().expression();
            Assert.NotNull(condition);
            var i = condition.left;

            Assert.NotNull(i);
            var lessThanOperator = condition.binaryOperator();
            Assert.NotNull(lessThanOperator);
            var operatorType = lessThanOperator.lessThanOperator();
            Assert.NotNull(operatorType);
            var conditionValue = condition.right;
            Assert.NotNull(conditionValue);
            var unaryExpression = conditionValue.unaryExpression();
            Assert.NotNull(unaryExpression);
            Assert.Equal("10", unaryExpression.GetText());

            var whileBlock = whileStatement.body;
            Assert.NotNull(whileBlock);

            var whileBlockStatement = whileBlock.statement();
            Assert.NotNull(whileBlockStatement);
            var assignStatement = whileBlockStatement.assignStatement();
            Assert.NotNull(assignStatement);
            var identifier = assignStatement.identifier();
            Assert.NotNull(identifier);
            Assert.Equal("i", identifier.GetText());
            var expression = assignStatement.expression();
            Assert.NotNull(expression);
            var binaryOperator = expression.binaryOperator();
            Assert.NotNull(binaryOperator);
            var operatorType2 = binaryOperator.addOperator();
            Assert.NotNull(operatorType2);
            var leftExpression = expression.left;
            Assert.NotNull(leftExpression);
            var unaryExpression2 = leftExpression.unaryExpression();
            Assert.NotNull(unaryExpression2);
            Assert.Equal("i", unaryExpression2.GetText());
            var rightExpression = expression.right;
            Assert.NotNull(rightExpression);
            var unaryExpression3 = rightExpression.unaryExpression();
            Assert.NotNull(unaryExpression3);
            Assert.Equal("1", unaryExpression3.GetText());

        }

        [Fact]
        public void IfStatement_ShouldParse()
        {
            string input =
            @"
                int i = 0;
                if (i < 20) 
                { 
                    i = i + 1; 
                }
                else
                {
                    i = i - 1;
                }
            ";

            AQLParser parser = SetupParser(input);
            var functionContext = parser.functionDefinition();
            Assert.NotNull(functionContext);

            var blockContext = functionContext.block();
            Assert.NotNull(blockContext);

            var statementContext = blockContext.statement();
            Assert.NotNull(statementContext);

            var variableDeclarationStatement = statementContext.variableDeclarationStatement();
            Assert.NotNull(variableDeclarationStatement);
            var type = variableDeclarationStatement.type();
            Assert.NotNull(type);
            Assert.Equal("int", type.GetText());
            var assignStatement1 = variableDeclarationStatement.assignStatement();
            Assert.NotNull(assignStatement1);
            var identifier1 = assignStatement1.identifier();
            Assert.NotNull(identifier1);
            Assert.Equal("i", identifier1.GetText());
            var expression1 = assignStatement1.expression();
            Assert.NotNull(expression1);
            var unaryExpression1 = expression1.unaryExpression();
            Assert.NotNull(unaryExpression1);
            Assert.Equal("0", unaryExpression1.GetText());


            var firstIfStatement = assignStatement1.nextStatement;
            Assert.NotNull(firstIfStatement);
            var ifStatement = firstIfStatement.ifStatement();
            Assert.NotNull(ifStatement);
            var ifCondition = ifStatement.ifCondition.unaryExpression().parenthesesExpression().expression();
            Assert.NotNull(ifCondition);
            Console.WriteLine(ifCondition.GetText());
            var i = ifCondition.left;
            Assert.NotNull(i);
            Assert.Equal("i", i.GetText());
            var lessThanOperator = ifCondition.binaryOperator();
            Assert.NotNull(lessThanOperator);
            var operatorType = lessThanOperator.lessThanOperator();
            Assert.NotNull(operatorType);
            var conditionValue = ifCondition.right;
            Assert.NotNull(conditionValue);
            var unaryExpression = conditionValue.unaryExpression();
            Assert.NotNull(unaryExpression);
            Assert.Equal("20", unaryExpression.GetText());

            var firstIfBlock = ifStatement.ifBody;
            Assert.NotNull(firstIfBlock);
            var firstIfBlockStatement = firstIfBlock.statement();
            Assert.NotNull(firstIfBlockStatement);
            var assignStatement = firstIfBlockStatement.assignStatement();
            Assert.NotNull(assignStatement);
            var identifier = assignStatement.identifier();
            Assert.NotNull(identifier);
            Assert.Equal("i", identifier.GetText());
            var expression = assignStatement.expression();
            Assert.NotNull(expression);
            var binaryOperator = expression.binaryOperator();
            Assert.NotNull(binaryOperator);
            var operatorType2 = binaryOperator.addOperator();
            Assert.NotNull(operatorType2);
            var leftExpression = expression.left;
            Assert.NotNull(leftExpression);
            var unaryExpression2 = leftExpression.unaryExpression();
            Assert.NotNull(unaryExpression2);
            Assert.Equal("i", unaryExpression2.GetText());
            var rightExpression = expression.right;
            Assert.NotNull(rightExpression);
            var unaryExpression3 = rightExpression.unaryExpression();
            Assert.NotNull(unaryExpression3);
            Assert.Equal("1", unaryExpression3.GetText());

            var elseStatement = ifStatement.elseStatement();
            Assert.NotNull(elseStatement);
            var elseBlock = elseStatement.block();
            Assert.NotNull(elseBlock);
            var elseBlockStatement = elseBlock.statement();
            Assert.NotNull(elseBlockStatement);
            var assignStatement2 = elseBlockStatement.assignStatement();
            Assert.NotNull(assignStatement2);
            var identifier2 = assignStatement2.identifier();
            Assert.NotNull(identifier2);
            Assert.Equal("i", identifier2.GetText());
            var expression2 = assignStatement2.expression();
            Assert.NotNull(expression2);
            var binaryOperator2 = expression2.binaryOperator();
            Assert.NotNull(binaryOperator2);
            var operatorType3 = binaryOperator2.subtractOperator();
            Assert.NotNull(operatorType3);
            var leftExpression2 = expression2.left;
            Assert.NotNull(leftExpression2);
            var unaryExpression4 = leftExpression2.unaryExpression();
            Assert.NotNull(unaryExpression4);
            Assert.Equal("i", unaryExpression4.GetText());
            var rightExpression2 = expression2.right;
            Assert.NotNull(rightExpression2);
            var unaryExpression5 = rightExpression2.unaryExpression();
            Assert.NotNull(unaryExpression5);
            Assert.Equal("1", unaryExpression5.GetText());

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
