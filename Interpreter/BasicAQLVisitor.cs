



using Antlr4.Runtime.Misc;
using Interpreter.Utilities;
using Interpreter.Utilities.Modules;
using Interpreter.Utilities.Types;

public class BasicAQLVisitor : AQLBaseVisitor<object?>
{
    ModuleContext moduleContext { get; set; } = new();
    Dictionary<string, object> Variables => moduleContext.Variables;
    Dictionary<string, object> Functions => moduleContext.Functions;
    Dictionary<string, ModuleContext> ImportedModules => moduleContext.ImportedModules;
    List<SemanticError> SemanticErrors => moduleContext.SemanticErrors;
    List<SemanticError> SemanticWarnings => moduleContext.SemanticWarnings;

    // Predefined elements
    public BasicAQLVisitor()
    {
        Functions["get_time"] = new Func<int>(() =>
        {
            // TODO: Implement a proper time function
            return new Random().Next(0, 1000000);
        });
    }

    public override object? VisitProgram([NotNull] AQLParser.ProgramContext context)
    {
        base.VisitProgram(context);
        return moduleContext;
    }

    public override object? VisitImportStatement([NotNull] AQLParser.ImportStatementContext context)
    {
        string moduleName = context.STRING().GetText();
        moduleName = moduleName[1..^1]; // Remove quotes

        if (ImportedModules.ContainsKey(moduleName))
        {
            SemanticError error = new(
                message: $"Module '{moduleName}' is already imported",
                context.Start.Line,
                context.Start.Column
            );
            SemanticWarnings.Add(error);
            return null;
        }


        if (ModuleResolver.ResolveModule(moduleName, out string? modulePath) == false)
        {
            SemanticError error = new(
                message: $"Module '{moduleName}' not found",
                context.Start.Line,
                context.Start.Column
            );
            SemanticErrors.Add(error);
            return null;
        }

        // THIS IS NOT SAFE FOR CYCLIC DEPENDENCIES, BUT I DON'T WANT TO DO THIS NOW ~Thomas
        ModuleContext importedModule = ModuleLoader.LoadModuleByPath(modulePath);
        if (importedModule.SemanticErrors.Count > 0)
        {
            SemanticErrors.AddRange(importedModule.SemanticErrors);
            return null;
        }

        ImportedModules[moduleName] = importedModule;
        return null;
    }

    public override object? VisitAssign([NotNull] AQLParser.AssignContext context)
    {
        string variableName = context.ID().GetText();

        object? value = Visit(context.expression());
        if (value is null)
        {
            return null;
        }

        if (Variables.ContainsKey(variableName))
        {
            Variables[variableName] = value;
        }
        else
        {
            SemanticError error = new(
                message: $"Variable '{variableName}' not found",
                context.Start.Line,
                context.Start.Column
            );
            SemanticErrors.Add(error);
            return null;
        }

        return null;
    }

    public override object? VisitFunctionDefinition([NotNull] AQLParser.FunctionDefinitionContext context)
    {
        string functionName = context.ID().GetText();
        if (Functions.ContainsKey(functionName))
        {
            SemanticError error = new(
                message: $"Function '{functionName}' is already defined",
                context.Start.Line,
                context.Start.Column
            );
            SemanticWarnings.Add(error);
            return null;
        }

        object? returnTypeObject = Visit(context.type());
        if (returnTypeObject is not Type returnType)
        {
            SemanticError error = new(
                message: $"Unexpected type in function definition",
                context.Start.Line,
                context.Start.Column
            );
            SemanticErrors.Add(error);
            return null;
        }

        object? formalParameters = Visit(context.formalParameterList());
        if (formalParameters is not List<TypedIdentifier> parameters)
        {
            SemanticError error = new(
                message: $"Unexpected type in formal parameter list",
                context.Start.Line,
                context.Start.Column
            );
            SemanticErrors.Add(error);
            return null;
        }

        throw new NotImplementedException("Function body is not implemented yet");
    }

    public override object? VisitFormalParameterList([NotNull] AQLParser.FormalParameterListContext context)
    {
        List<TypedIdentifier> parameters = [];
        object? typeObject = Visit(context.type());
        if (typeObject is not Type type)
        {
            SemanticError error = new(
                message: $"Unexpected type in formal parameter list",
                context.Start.Line,
                context.Start.Column
            );
            SemanticErrors.Add(error);
            return null;
        }

        string identifier = context.ID().GetText();

        parameters.Add(new(identifier, type));

        if (context.formalParameterList() != null)
        {
            object? parametersObject = Visit(context.formalParameterList());
            if (parametersObject is List<TypedIdentifier> nestedParameters)
            {
                parameters.AddRange(nestedParameters);
            }
            else
            {
                SemanticError error = new(
                    message: $"Unexpected type in formal parameter list",
                    context.Stop.Line,
                    context.Stop.Column
                );
                SemanticErrors.Add(error);
                return null;
            }
        }

        return parameters;
    }

    public override object? VisitType([NotNull] AQLParser.TypeContext context)
    {
        if (context.TYPEKEYWORD() != null)
        {
            string typeString = context.TYPEKEYWORD().GetText();
            if (typeString.TryParseToPrimitiveType(out Type? type) == false)
            {
                SemanticError error = new(
                    message: $"Unknown type '{typeString}'",
                    context.Start.Line,
                    context.Start.Column
                );
                SemanticErrors.Add(error);
                return null;
            }
            return type;
        }
        else if (context.arrayType() != null)
        {
            return Visit(context.arrayType());
        }
        else if (context.routeType() != null)
        {
            return Visit(context.routeType());
        }
        else
        {
            SemanticError error = new(
                message: $"Exprected a type",
                context.Start.Line,
                context.Start.Column
            );
            SemanticErrors.Add(error);
            return null;
        }
    }

    public override object? VisitArrayType([NotNull] AQLParser.ArrayTypeContext context)
    {
        if (context.type() != null)
        {
            object? typeObject = Visit(context.type());
            if (typeObject is Type type)
            {
                if (type.IsArray)
                {
                    SemanticError error = new(
                        message: $"Array can not contain an array",
                        context.Start.Line,
                        context.Start.Column
                    );
                    SemanticErrors.Add(error);
                    return null;
                }
                return type;
            }
            else
            {
                SemanticError error = new(
                    message: $"Expected a type",
                    context.Start.Line,
                    context.Start.Column
                );
                SemanticErrors.Add(error);
                return null;
            }
        }
        else
        {
            SemanticError error = new(
                message: $"Expected a type",
                context.Start.Line,
                context.Start.Column
            );
            SemanticErrors.Add(error);
            return null;
        }
    }
    /*
        public override object? VisitRouteType([NotNull] AQLParser.RouteTypeContext context)
        {
            if (context.type() != null)
            {
                object? typeObject = Visit(context.type());
                if (typeObject is Type type)
                {
                    return type;
                }
                else
                {
                    SemanticError error = new(
                        message: $"Expected a type",
                        context.Start.Line,
                        context.Start.Column
                    );
                    SemanticErrors.Add(error);
                    return null;
                }
            }
            else
            {
                SemanticError error = new(
                    message: $"Expected a type",
                    context.Start.Line,
                    context.Start.Column
                );
                SemanticErrors.Add(error);
                return null;
            }
        }*/
}