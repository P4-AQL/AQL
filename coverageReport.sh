#!/bin/bash
set -e

# Clean up previous results
rm -rf bin obj coverage coverage-report TestResults

# Build everything (you could scope this tighter if needed)
dotnet build --no-incremental

# Create the output directory
mkdir -p coverage

# Find all test DLLs (assumes they are built to bin/Debug/net9.0)
for csproj in $(find . -type f -name "*.csproj" | grep "Tests"); do
  echo "Processing $csproj..."
  # Extract the DLL name from the project file
  dll_name=$(basename "$csproj" .csproj).dll
  project_dir=$(dirname "$csproj")
  dll_path="$project_dir/bin/Debug/net9.0/$dll_name"

  if [[ -f "$dll_path" ]]; then
    coverlet "$dll_path" \
      --target "dotnet" \
      --targetargs "test $project_dir --no-build --nologo" \
      --format lcov \
      --output "coverage/$(basename $project_dir).lcov"
  else
    echo "⚠️ Skipping $dll_name — build output not found."
  fi
done

# Merge all lcov files into one
lcov_files=$(find coverage -name "*.lcov")
lcov_result="coverage/lcov.info"

echo "Combining all lcov reports..."
cat $lcov_files > "$lcov_result"

# Generate HTML report
genhtml "$lcov_result" --output-directory coverage-report --ignore-errors category

# Open the report
xdg-open coverage-report/index.html
