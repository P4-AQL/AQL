#!/bin/bash
set -e

# Clean up previous results
rm -rf bin obj coverage coverage-report TestResults

# Build the test project
dotnet build

# Run Coverlet manually
coverlet ./bin/Debug/net9.0/SimulationEngine.Tests.dll \
  --target "dotnet" \
  --targetargs "test --no-build --nologo" \
  --format lcov \
  --output coverage/lcov.info

# Generate HTML report, ignoring category errors
genhtml coverage/lcov.info --output-directory coverage-report --ignore-errors category

# Open the report
xdg-open coverage-report/index.html
