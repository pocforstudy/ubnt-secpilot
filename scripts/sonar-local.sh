#!/bin/bash

# UBNT SecPilot - SonarQube Local Analysis Script
# This script runs SonarQube analysis for the project

set -e

echo "🔍 UBNT SecPilot - SonarQube Local Analysis"
echo "==========================================="

# Check if SonarQube is running
if ! curl -f -s http://localhost:9000/api/system/status > /dev/null 2>&1; then
    echo "❌ SonarQube is not accessible at http://localhost:9000"
    echo ""
    echo "🚀 To start SonarQube:"
    echo "   docker compose -f docker-compose.simple.yml up -d"
    echo ""
    echo "⏳ Wait 30-60 seconds for SonarQube to fully start"
    echo "🔗 Then access: http://localhost:9000 (admin/admin)"
    exit 1
fi

echo "✅ SonarQube is running"

# Check if sonar-scanner is available
if ! command -v sonar-scanner &> /dev/null; then
    echo "⚠️  sonar-scanner not found. Installing dotnet-sonarscanner..."

    # Install dotnet-sonarscanner
    dotnet tool install --global dotnet-sonarscanner

    echo "✅ dotnet-sonarscanner installed"
fi

echo ""
echo "🏃 Running SonarQube analysis..."

# Begin SonarQube analysis
dotnet sonarscanner begin \
    /k:"ubnt-secpilot" \
    /d:sonar.host.url="http://localhost:9000" \
    /d:sonar.login="admin" \
    /d:sonar.password="admin" \
    /d:sonar.cs.opencover.reportsPaths="**/coverage.xml" \
    /d:sonar.exclusions="**/bin/**,**/obj/**,**/*.Tests/**,**/Migrations/**"

# Build and test
echo "🔨 Building and testing..."
dotnet build UbntSecPilot.sln
dotnet test UbntSecPilot.sln --collect:"XPlat Code Coverage" --logger trx --results-directory TestResults

# End SonarQube analysis
echo "📊 Publishing results to SonarQube..."
dotnet sonarscanner end /d:sonar.login="admin" /d:sonar.password="admin"

echo ""
echo "✅ SonarQube analysis completed!"
echo "🔗 View results at: http://localhost:9000/dashboard?id=ubnt-secpilot"
