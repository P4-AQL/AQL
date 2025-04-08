Start off by installing dotnet, antlr and java

**Commands to run the things:**

antlr -Dlanguage=CSharp -visitor -o GeneratedCode MyGrammar.g4 (Assuming you have antlr as alias for: java -jar ~/.local/lib/antlr/antlr-4.13.1-complete.jar (or whatever path you have chosen))

dotnet build

dotnet run
