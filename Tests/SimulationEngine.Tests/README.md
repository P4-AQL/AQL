to run the tests run command:
dotnet test

To make the coverage report:
have the following tools installed: ReportGeneratpr & Coverlet
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet tool install --global coverlet.console

(you may need to add the .NET tools to your PATH this are for ZSH)
export PATH="$HOME/.dotnet/tools:$PATH"
source ~/.zshrc

then run the commands:
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
xdg-open coveragereport/index.html
