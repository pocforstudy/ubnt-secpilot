#!/bin/bash

# UBNT SecPilot - Enhanced Setup Script
# Complete setup and configuration for the .NET 8 + Orleans security platform

set -e

echo " UBNT SecPilot - Enhanced Setup"
echo "================================="

# Color codes for better output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Check if Docker is running
echo -e "\n Checking prerequisites..."
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED} Docker is not running. Please start Docker first.${NC}"
    exit 1
fi
echo -e "${GREEN} Docker is running${NC}"

# Check if Docker Compose is available
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED} Docker Compose is not installed. Please install docker-compose first.${NC}"
    exit 1
fi
echo -e "${GREEN} Docker Compose is available${NC}"

# Navigate to project directory
cd "$(dirname "$0")"
PROJECT_DIR=$(pwd)
echo -e "${BLUE} Setting up in: $PROJECT_DIR${NC}"

# Function to check service health
check_service_health() {
    local service=$1
    local url=$2
    local max_attempts=30
    local attempt=1

    echo -e "\n Waiting for $service to be ready..."

    while [ $attempt -le $max_attempts ]; do
        if curl -f -s "$url" > /dev/null 2>&1; then
            echo -e "${GREEN} $service is ready!${NC}"
            return 0
        fi

        echo -e "${YELLOW}   Attempt $attempt/$max_attempts - $service not ready yet...${NC}"
        sleep 2
        attempt=$((attempt + 1))
    done

    echo -e "${RED} $service failed to start after $max_attempts attempts${NC}"
    return 1
}

# Create .env file if it doesn't exist
echo -e "\n Checking configuration..."
if [ ! -f .env ]; then
    echo -e "${YELLOW} Creating .env file from template...${NC}"
    cp .env.example .env
    echo -e "${GREEN} .env file created. Please edit it with your configuration.${NC}"
    echo -e "${BLUE} Important: Edit .env file with your API keys and credentials${NC}"
fi

# Check if we should use simple or full compose
USE_SIMPLE_COMPOSE=false
if [ "$1" = "--simple" ]; then
    USE_SIMPLE_COMPOSE=true
    echo -e "${BLUE} Using simplified Docker Compose (without complex integrations)${NC}"
fi

# Start the services
echo -e "\n Starting all services..."
if [ "$USE_SIMPLE_COMPOSE" = true ]; then
    docker-compose -f docker-compose.simple.yml up -d
    COMPOSE_FILE="docker-compose.simple.yml"
else
    docker-compose up -d
    COMPOSE_FILE="docker-compose.yml"
fi

# Wait for services to start
echo -e "\n Waiting for services to be ready..."
sleep 15

# Check service status
echo -e "\n Service Status:"
docker-compose ps

# Check individual service health
echo -e "\n Checking service health..."

# API Health Check
check_service_health "API" "http://localhost:8000/health" || {
    echo -e "${YELLOW}⚠️  API health check failed, but continuing...${NC}"
}

# Blazor UI Health Check (if accessible)
if curl -f -s "http://localhost:8501" > /dev/null 2>&1; then
    echo -e "${GREEN} Blazor UI is accessible${NC}"
else
    echo -e "${YELLOW}⚠️  Blazor UI not accessible yet${NC}"
fi

# SonarQube Health Check (if running)
if docker-compose ps sonarqube | grep -q "Up"; then
    check_service_health "SonarQube" "http://localhost:9000/api/system/status" || {
        echo -e "${YELLOW}⚠️  SonarQube health check failed${NC}"
    }
fi

# MongoDB Health Check
if docker-compose ps mongo | grep -q "Up"; then
    echo -e "${GREEN} MongoDB is running${NC}"
fi

# Redpanda Health Check
if docker-compose ps redpanda | grep -q "Up"; then
    echo -e "${GREEN} Redpanda (Kafka) is running${NC}"
fi

echo -e "\n Setup Complete!"
echo -e "${GREEN}=================${NC}"

echo -e "\n Access URLs:"
echo -e "${BLUE}    Dashboard (Blazor):     http://localhost:8501${NC}"
echo -e "${BLUE}   🔌 API (ASP.NET Core):    http://localhost:8000${NC}"
echo -e "${BLUE}    API Documentation:     http://localhost:8000/swagger${NC}"
if docker-compose ps sonarqube | grep -q "Up"; then
    echo -e "${BLUE}    SonarQube:             http://localhost:9000${NC}"
fi
if docker-compose ps grafana | grep -q "Up"; then
    echo -e "${BLUE}   📈 Grafana:               http://localhost:3000${NC}"
fi
if docker-compose ps prometheus | grep -q "Up"; then
    echo -e "${BLUE}    Prometheus:            http://localhost:9090${NC}"
fi

echo -e "\n Default Credentials:"
echo -e "${YELLOW}   SonarQube: admin/admin (CHANGE THIS!)${NC}"
echo -e "${YELLOW}   Grafana: admin/admin${NC}"

echo -e "\n Documentation:"
echo -e "${BLUE}   📖 Main README:           README.md${NC}"
echo -e "${BLUE}   🤖 Agents Guide:          AGENTS.md${NC}"
echo -e "${BLUE}    CI/CD Guide:           .github/README.md${NC}"
echo -e "${BLUE}    Interview Process:     Interview.md${NC}"

echo -e "\n  Useful Commands:"
echo -e "${GREEN}   View logs:              docker-compose logs -f${NC}"
echo -e "${GREEN}   View specific service:  docker-compose logs -f <service-name>${NC}"
echo -e "${GREEN}   Stop all services:      docker-compose down${NC}"
echo -e "${GREEN}   Restart service:        docker-compose restart <service-name>${NC}"
echo -e "${GREEN}   Clean everything:       docker-compose down -v${NC}"

echo -e "\n Development Commands:"
echo -e "${GREEN}   Run tests:              ./build.sh test${NC}"
echo -e "${GREEN}   Code analysis:          ./scripts/sonar-local.sh${NC}"
echo -e "${GREEN}   Setup quality gates:    ./infrastructure/observability/sonarqube/quality-gates-setup.sh${NC}"

echo -e "\n Need help?"
echo -e "${BLUE}   Check README.md for detailed documentation${NC}"
echo -e "${BLUE}   Or run: ./infrastructure/observability/sonarqube/quality-gates-setup.sh${NC}"

# Optional: Show SonarQube setup if available
if [ "$USE_SIMPLE_COMPOSE" = false ] && docker-compose ps sonarqube | grep -q "Up"; then
    echo -e "\n SonarQube is running!"
    echo -e "${YELLOW}   Run quality gates setup: ./infrastructure/observability/sonarqube/quality-gates-setup.sh${NC}"
fi

echo -e "\n${GREEN} UBNT SecPilot is ready to use!${NC}"
