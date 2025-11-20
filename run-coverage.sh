#!/bin/bash

# Code Coverage Script for TeCLI
# Requires: dotnet-coverage tool
# Install: dotnet tool install --global dotnet-coverage

set -e

echo "Running tests with code coverage..."

# Clean previous coverage results
rm -rf coverage/
mkdir -p coverage

# Run tests with coverage
dotnet test TeCLI.Tests/TeCLI.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage \
  --logger "console;verbosity=detailed"

# Find the coverage file
COVERAGE_FILE=$(find ./coverage -name "coverage.cobertura.xml" | head -1)

if [ -z "$COVERAGE_FILE" ]; then
    echo "❌ Coverage file not found!"
    exit 1
fi

echo "✅ Coverage file generated: $COVERAGE_FILE"

# Generate HTML report using reportgenerator (optional)
if command -v reportgenerator &> /dev/null; then
    echo "Generating HTML coverage report..."
    reportgenerator \
      -reports:"$COVERAGE_FILE" \
      -targetdir:"coverage/html" \
      -reporttypes:Html

    echo "✅ HTML report generated at: coverage/html/index.html"
else
    echo "ℹ️  Install reportgenerator for HTML reports:"
    echo "   dotnet tool install --global dotnet-reportgenerator-globaltool"
fi

# Display summary
echo ""
echo "📊 Coverage Summary"
echo "=================="
grep -A 5 '<coverage' "$COVERAGE_FILE" || true
