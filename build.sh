#!/bin/bash

# UBNT SecPilot - Enhanced Build Script with .NET Aspire Integration
# Comprehensive build and development script for .NET 8 + Orleans + Aspire

set -e

# Color codes for better output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Default values
BUILD_CONFIG=${BUILD_CONFIG:-Release}
DEBUG_PORT=${DEBUG_PORT:-8000}
BLAZOR_PORT=${BLAZOR_PORT:-8501}
PROJECT_DIR=$(pwd)

# Function to print colored output
print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_header() {
    echo -e "${PURPLE}🚀 $1${NC}"
}

# Function to check if .NET SDK is installed
check_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET 8 SDK is not installed. Please install it first."
        echo "Visit: https://dotnet.microsoft.com/download/dotnet/8.0"
        exit 1
    fi

    local dotnet_version=$(dotnet --version | cut -d'.' -f1)
    if [ "$dotnet_version" -lt "8" ]; then
        print_error ".NET 8 SDK is required. Current version: $(dotnet --version)"
        exit 1
    fi

    print_success ".NET $(dotnet --version) is installed"
}

# Function to check if Docker is available
check_docker() {
    if ! command -v docker &> /dev/null; then
        print_warning "Docker is not installed. Docker commands will not work."
        return 1
    fi

    if ! docker info > /dev/null 2>&1; then
        print_warning "Docker is not running. Please start Docker first."
        return 1
    fi

    print_success "Docker is available"
    return 0
}

# Function to restore dependencies
restore() {
    print_header "Restoring NuGet dependencies..."
    dotnet restore UbntSecPilot.sln
    print_success "Dependencies restored"
}

# Function to build the application
build() {
    local config=${1:-$BUILD_CONFIG}
    print_header "Building application in $config mode..."

    if [ ! -f "UbntSecPilot.sln" ]; then
        print_error "Solution file not found: UbntSecPilot.sln"
        exit 1
    fi

    dotnet build UbntSecPilot.sln --configuration $config --no-restore
    print_success "Build completed in $config mode"
}

# Function to clean build artifacts
clean() {
    print_header "Cleaning build artifacts..."
    dotnet clean UbntSecPilot.sln
    rm -rf src/*/bin src/*/obj tests/*/bin tests/*/obj
    print_success "Clean completed"
}

# Function to run tests
test_app() {
    print_header "Running tests..."
    dotnet test UbntSecPilot.sln --configuration Debug --no-build --verbosity normal
    print_success "All tests passed"
}

# Function to debug API only
debug_api() {
    print_header "Starting API in debug mode..."
    print_info "API will be available at: http://localhost:$DEBUG_PORT"
    print_info "Swagger UI: http://localhost:$DEBUG_PORT/swagger"
    print_info "Stream Monitor: http://localhost:$DEBUG_PORT/stream-monitor.html"

    cd src/UbntSecPilot.WebApi
    dotnet watch run --urls "http://localhost:$DEBUG_PORT"
}

# Function to debug Blazor UI only
debug_blazor() {
    print_header "Starting Blazor UI in debug mode..."
    print_info "Blazor UI will be available at: http://localhost:$BLAZOR_PORT"

    cd src/UbntSecPilot.BlazorUI
    dotnet watch run --urls "http://localhost:$BLAZOR_PORT"
}

# Function to debug both API and Blazor
debug_full() {
    print_header "Starting full application in debug mode..."
    print_info "API: http://localhost:$DEBUG_PORT"
    print_info "Blazor UI: http://localhost:$BLAZOR_PORT"

    # Start API in background
    debug_api &
    API_PID=$!

    # Wait a moment for API to start
    sleep 3

    # Start Blazor UI
    debug_blazor &
    BLAZOR_PID=$!

    print_info "Both services are starting..."
    print_info "Press Ctrl+C to stop both services"

    # Function to cleanup background processes
    cleanup() {
        print_info "Stopping services..."
        kill $API_PID $BLAZOR_PID 2>/dev/null || true
        exit 0
    }

    # Set trap to cleanup on script exit
    trap cleanup EXIT INT TERM

    # Wait for both processes
    wait $API_PID $BLAZOR_PID
}

# Function to debug with Aspire
debug_aspire() {
    print_header "Starting application with .NET Aspire..."
    print_info "Aspire Dashboard: http://localhost:18888"
    print_info "API: http://localhost:8000/api"
    print_info "Blazor UI: http://localhost:8501"

    # Check if Aspire is available
    if ! dotnet list package --include-prerelease | grep -q "Aspire"; then
        print_error ".NET Aspire packages not found. Please ensure Aspire is installed."
        print_info "You can install Aspire with: dotnet workload install aspire"
        exit 1
    fi

    # Start Aspire AppHost
    cd src/UbntSecPilot.AppHost
    dotnet run --no-launch-profile
}

# Function to run with Docker
docker_run() {
    print_header "Starting full stack with Docker..."

    if ! check_docker; then
        print_error "Docker is required for this command"
        exit 1
    fi

    # Start services in background
    docker-compose up -d

    print_success "Services are starting..."
    print_info "This may take a few minutes for all services to be ready"

    # Show status after a delay
    sleep 10
    show_status
}

# Function to run Docker in background
docker_bg() {
    print_header "Starting Docker services in background..."

    if ! check_docker; then
        print_error "Docker is required for this command"
        exit 1
    fi

    docker-compose up -d
    print_success "Services started in background"

    # Show access information
    echo
    echo -e "${BLUE}🌐 Access URLs:${NC}"
    echo -e "  API: http://localhost:8000"
    echo -e "  Blazor UI: http://localhost:8501"
    echo -e "  Grafana: http://localhost:3000"
    echo -e "  Prometheus: http://localhost:9090"
    echo -e "  SonarQube: http://localhost:9000"

    print_info "Use 'docker-compose logs -f' to view logs"
    print_info "Use './build.sh docker-stop' to stop services"
}

# Function to stop Docker services
docker_stop() {
    print_header "Stopping Docker services..."

    if ! check_docker; then
        print_error "Docker is required for this command"
        exit 1
    fi

    docker-compose down
    print_success "All services stopped"
}

# Function to show Docker status
show_docker_status() {
    echo
    echo -e "${BLUE}🌐 Access URLs:${NC}"
    echo -e "  API: http://localhost:8000"
    echo -e "  Blazor UI: http://localhost:8501"
    echo -e "  Trace Viewer: http://localhost:8000/api/trace"
    echo -e "  Grafana: http://localhost:3000"
    echo -e "  Prometheus: http://localhost:9090"
    echo -e "  SonarQube: http://localhost:9000"

    echo
    echo -e "${BLUE}📊 Useful Commands:${NC}"
    echo -e "  View logs: docker-compose logs -f"
    echo -e "  Stop all: ./build.sh stop"
    echo -e "  Restart: docker-compose restart"
}

# Quick start commands for beginners
quick_start() {
    print_header "🚀 UBNT SecPilot - Quick Start Guide"

    echo
    echo -e "${BLUE}🎯 Most Common Commands:${NC}"
    echo -e "  ${GREEN}./build.sh dev${NC}           # Start development environment"
    echo -e "  ${GREEN}./build.sh prod${NC}          # Start production environment"
    echo -e "  ${GREEN}./build.sh api${NC}           # Start API only"
    echo -e "  ${GREEN}./build.sh web${NC}           # Start web interface only"
    echo -e "  ${GREEN}./build.sh stop${NC}          # Stop all services"
    echo
    echo -e "${BLUE}📋 For Beginners:${NC}"
    echo -e "  ${YELLOW}./build.sh dev${NC}           # This is all you need to start!"
    echo
    echo -e "${BLUE}🔧 Advanced Commands:${NC}"
    echo -e "  ${PURPLE}./build.sh build${NC}         # Build application"
    echo -e "  ${PURPLE}./build.sh test${NC}          # Run tests"
    echo -e "  ${PURPLE}./build.sh clean${NC}         # Clean build files"
    echo -e "  ${PURPLE}./build.sh status${NC}        # Show project status"
}

# Function for simple development start
dev() {
    print_header "Starting development environment..."
    print_info "This will start the complete development stack"

    # Check prerequisites
    check_dotnet

    # Restore dependencies if needed
    if [ ! -d "src/UbntSecPilot.WebApi/bin" ] || [ ! -d "src/UbntSecPilot.BlazorUI/bin" ]; then
        print_info "Restoring dependencies..."
        restore
        build Debug
    fi

    # Start full development environment
    debug_full
}

# Function for production deployment
prod() {
    print_header "Starting production environment..."

    if ! check_docker; then
        print_error "Docker is required for production deployment"
        exit 1
    fi

    print_info "Building production image..."
    docker-compose build --parallel

    print_info "Starting production stack..."
    docker-compose up -d

    print_success "Production environment started!"
    show_docker_status
}

# Function to start API only
api() {
    print_header "Starting API server..."
    check_dotnet

    if [ ! -d "src/UbntSecPilot.WebApi/bin" ]; then
        print_info "Building API..."
        restore
        build Debug
    fi

    debug_api
}

# Function to start web interface only
web() {
    print_header "Starting web interface..."
    check_dotnet

    if [ ! -d "src/UbntSecPilot.BlazorUI/bin" ]; then
        print_info "Building web interface..."
        restore
        build Debug
    fi

    debug_blazor
}

# Function to stop all services
stop() {
    print_header "Stopping all services..."

    # Stop Docker services if running
    if docker-compose ps -q | grep -q .; then
        print_info "Stopping Docker services..."
        docker-compose down
    fi

    # Kill any running .NET processes
    if pgrep -f "dotnet watch" > /dev/null; then
        print_info "Stopping .NET processes..."
        pkill -f "dotnet watch" || true
    fi

    print_success "All services stopped"
}

# Main command dispatcher
case "${1:-help}" in
    "restore")
        check_dotnet
        restore
        ;;
    "build")
        check_dotnet
        build $2
        ;;
    "clean")
        check_dotnet
        clean
        ;;
    "test")
        check_dotnet
        test_app
        ;;
    "debug")
        check_dotnet
        case "${2:-api}" in
            "api")
                debug_api
                ;;
            "blazor")
                debug_blazor
                ;;
            "full")
                debug_full
                ;;
            "aspire")
                debug_aspire
                ;;
            *)
                print_error "Unknown debug target: $2"
                echo "Available targets: api, blazor, full, aspire"
                exit 1
                ;;
        esac
        ;;
    "dev")
        dev
        ;;
    "prod")
        prod
        ;;
    "api")
        api
        ;;
    "web")
        web
        ;;
    "stop")
        stop
        ;;
    "docker")
        docker_run
        ;;
    "docker-bg")
        docker_bg
        ;;
    "docker-stop")
        docker_stop
        ;;
    "status")
        show_status
        ;;
    "quick-start")
        quick_start
        ;;
    "help"|"-h"|"--help")
        show_help
        ;;
    *)
        print_error "Unknown command: $1"
        echo
        echo -e "${YELLOW}💡 Tip: Try './build.sh quick-start' for beginners!${NC}"
        echo
        show_help
        exit 1
        ;;
esac
