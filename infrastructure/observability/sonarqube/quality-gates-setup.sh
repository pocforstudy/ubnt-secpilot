#!/bin/bash
# SonarQube Quality Gates Configuration Script
# This script provides instructions and setup for UBNT SecPilot SonarQube quality gates

set -e

echo "🔧 SonarQube Quality Gates Setup for UBNT SecPilot"
echo "=================================================="

# Check if SonarQube is running
if ! curl -f -s http://localhost:9000/api/system/status > /dev/null 2>&1; then
    echo "❌ SonarQube is not accessible at http://localhost:9000"
    echo ""
    echo "🚀 To start SonarQube:"
    echo "   docker compose -f docker-compose.simple.yml up -d"
    echo "   # or"
    echo "   docker compose up -d sonarqube sonarqube-db"
    echo ""
    echo "⏳ Wait 30-60 seconds for SonarQube to fully start"
    echo "🔗 Then access: http://localhost:9000 (admin/admin)"
    exit 1
fi

echo "✅ SonarQube is running and accessible"

# Quality Gate configuration
QUALITY_GATE_NAME="ubnt-secpilot-quality-gate"

echo ""
echo "📋 Quality Gate Configuration: $QUALITY_GATE_NAME"
echo ""

# Display current configuration
echo "🎯 Recommended Quality Gate Conditions:"
echo ""

echo "1. Reliability Rating (new_reliability_rating)"
echo "   • Operator: GT (greater than)"
echo "   • Error threshold: E"
echo "   • Description: No new E-rated reliability issues allowed"
echo ""

echo "2. Security Rating (new_security_rating)"
echo "   • Operator: GT (greater than)"
echo "   • Error threshold: E"
echo "   • Description: No new E-rated security vulnerabilities allowed"
echo ""

echo "3. Maintainability Rating (new_maintainability_rating)"
echo "   • Operator: GT (greater than)"
echo "   • Error threshold: D"
echo "   • Description: No new D-rated maintainability issues allowed"
echo ""

echo "4. Coverage (new_coverage)"
echo "   • Operator: LT (less than)"
echo "   • Error threshold: 80"
echo "   • Description: Minimum 80% coverage on new code"
echo ""

echo "5. Duplicated Lines Density (new_duplicated_lines_density)"
echo "   • Operator: GT (greater than)"
echo "   • Error threshold: 3"
echo "   • Description: Maximum 3% duplicated lines on new code"
echo ""

echo "6. Blocker Violations (new_blocker_violations)"
echo "   • Operator: GT (greater than)"
echo "   • Error threshold: 0"
echo "   • Description: Zero new blocker violations allowed"
echo ""

echo "7. Critical Violations (new_critical_violations)"
echo "   • Operator: GT (greater than)"
echo "   • Error threshold: 0"
echo "   • Description: Zero new critical violations allowed"
echo ""

echo "🔗 Manual Configuration Steps:"
echo "============================"
echo "1. 🌐 Access SonarQube: http://localhost:9000"
echo "2. 🔑 Login: admin/admin (change password!)"
echo "3. 📋 Navigate: Quality Gates → Create Quality Gate"
echo "4. ✏️  Name: '$QUALITY_GATE_NAME'"
echo "5. ⚙️  Add Conditions: Use the specifications above"
echo "6. 💾 Save and set as default quality gate"
echo ""

echo "🔄 CI/CD Integration:"
echo "===================="
echo "• ✅ SonarQube scan runs automatically in GitHub Actions"
echo "• ✅ Quality gate status is checked after each scan"
echo "• ✅ Build fails if quality gate conditions are not met"
echo "• ✅ Coverage reports are uploaded to Codecov"
echo ""

echo "📊 Additional Setup (Optional):"
echo "=============================="
echo "• 📈 Create custom dashboards for project metrics"
echo "• 🔧 Configure webhooks for Slack/Teams notifications"
echo "• 📋 Set up quality profiles for C#/.NET rules"
echo "• 🔒 Configure user permissions and project access"
echo ""

echo "🎯 Quick Verification:"
echo "===================="
echo "After configuring quality gates:"
echo ""
echo "# Test locally:"
echo "./scripts/sonar-local.sh"
echo ""
echo "# Check GitHub Actions:"
echo "# Go to Actions tab → sonar-analysis job"
echo ""

echo "✅ Quality Gates Setup Complete!"
echo ""
echo "📚 For more details, see:"
echo "   infrastructure/observability/sonarqube/README.md"
