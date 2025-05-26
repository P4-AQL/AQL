


using Antlr4.Runtime;
using Interpreter.SemanticAnalysis;
using Interpreter.Visitors;

public class InterpreterIntergrationTests
{
    readonly string inputWithoutErrors =
        """
            // Airport.AQL
            queue securityQueue {
                servers: 8;
                service: 5;
                capacity: 100;
                * *
            }

            // Stdlib.AQL
            function int get_time_in_hours() {
                return 10; 
            }

            function int exponential(int x) {
                return x*x;
            }

            // Input.AQL
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
                ArrivalArea : ArrivalArea1;
                ProcessingArea : ProcessingArea1;
                10 -> arrival -> ArrivalArea1.main_parking_entrance;
                ArrivalArea1.path_to_sec_b -> ProcessingArea1.arrival_sec_b;
                ArrivalArea1.path_to_after_check_in -> ProcessingArea1.arrival_after_check_in;
                ArrivalArea1.path_to_sec_a1 -> ProcessingArea1.arrival_sec_a1;
                ArrivalArea1.path_to_sec_a2 -> ProcessingArea1.arrival_sec_a2;
                ProcessingArea1.flight -> plane_go_zumzum;
            }

            simulate {
                run: Airport;
                until: 60000;
                times: 1;
            }
        """;

    readonly string inputWithFiveErrors =
        """
            // Airport.AQL
            queue securityQueue {
                servers: 8;
                service: 5;
                capacity: 100;
                * *
            }

            // Stdlib.AQL
            function int get_time_in_hours() {
                return 10.5; 
            }

            function int exponential(bool x) {
                return x*x;
            }

            // Input.AQL
            queue ParkingLot {
                servers: 100;
                service: 300;
                capacity: 100;
                *mrt, vrt, gg*
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
                ArrivalArea : ArrivalArea1;
                ProcessingArea : ProcessingArea1;
                10 -> arrival -> ArrivalArea1.main_parking_entrance;
                ArrivalArea1.path_to_sec_b -> ProcessingArea1.arrival_sec_b;
                ArrivalArea1.path_to_after_check_in -> ProcessingArea1.arrival_after_check_in;
                ArrivalArea1.path_to_sec_a1 -> ProcessingArea1.arrival_sec_a1;
                ArrivalArea1.path_to_sec_a2 -> ProcessingArea1.arrival_sec_a2;
                ProcessingArea1.flight -> plane_go_zumzum;
            }

            simulate {
                run: AirportGG;
                until: 6000;
                times: 1;
            }
        """;

    public static AQLParser Setup(string input)
    {
        AntlrInputStream antlrFileStream = new(input);
        AQLLexer lexer = new(antlrFileStream);
        CommonTokenStream commonTokenStream = new(lexer);
        AQLParser parser = new(commonTokenStream);
        parser.RemoveErrorListeners();
        return parser;
    }

    [Fact]
    public void InterpreterIntegrationTest_WithoutErrors_ShouldPass()
    {
        var parser = Setup(inputWithoutErrors);

        ASTAQLVisitor visitor = new();
        var ast = visitor.VisitProgramEOF(parser.programEOF());

        TypeChecker typeChecker = new();
        List<string> errors = [];
        typeChecker.TypeCheckNode(ast, errors);

        Assert.Empty(errors);

        var interpreter = new InterpreterClass(ast);
        var interpretedEnvironment = interpreter.StartInterpretation();

        Assert.False(interpretedEnvironment.EncounteredError);
    }

    [Fact]
    public void InterpreterIntegrationTest_WithErrors_ShouldFail()
    {
        var parser = Setup(inputWithFiveErrors);

        ASTAQLVisitor visitor = new();
        var ast = visitor.VisitProgramEOF(parser.programEOF());

        TypeChecker typeChecker = new();
        List<string> errors = [];
        typeChecker.TypeCheckNode(ast, errors);

        Assert.NotEmpty(errors);
        Assert.Equal(5, errors.Count);
    }
}