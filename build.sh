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

# Function to show project status
show_status() {
    print_header "Project Status"

    echo
    echo -e "${BLUE}📋 Project Structure:${NC}"
    if [ -f "UbntSecPilot.sln" ]; then
        print_success "✅ Solution file found"
    else
        print_error "❌ Solution file missing"
    fi

    echo
    echo -e "${BLUE}🔧 Dependencies:${NC}"
    check_dotnet

    echo
    echo -e "${BLUE}🐳 Docker:${NC}"
    check_docker

    echo
    echo -e "${BLUE}📦 Available Commands:${NC}"
    echo -e "  Build:        ./build.sh build [config]"
    echo -e "  Restore:      ./build.sh restore"
    echo -e "  Clean:        ./build.sh clean"
    echo -e "  Test:         ./build.sh test"
    echo
    echo -e "${BLUE}🐛 Debug Commands:${NC}"
    echo -e "  API:          ./build.sh debug api"
    echo -e "  Blazor:       ./build.sh debug blazor"
    echo -e "  Full:         ./build.sh debug full"
    echo -e "  Aspire:       ./build.sh debug aspire"
    echo
    echo -e "${BLUE}🐳 Docker Commands:${NC}"
    echo -e "  Run:          ./build.sh docker"
    echo -e "  Background:   ./build.sh docker-bg"
    echo -e "  Stop:         ./build.sh docker-stop"
    echo
    echo -e "${BLUE}📊 Information:${NC}"
    echo -e "  Status:       ./build.sh status"
    echo -e "  Help:         ./build.sh help"

    echo
    echo -e "${BLUE}⚙️  Current Configuration:${NC}"
    echo -e "  Build Config: $BUILD_CONFIG"
    echo -e "  API Port: $DEBUG_PORT"
    echo -e "  Blazor Port: $BLAZOR_PORT"
}

# Function to show help
show_help() {
    echo "UBNT SecPilot Build Script - Help"
    echo "================================"
    echo
    echo "A comprehensive build and development script for the UBNT SecPilot"
    echo ".NET 8 application with Orleans and .NET Aspire integration."
    echo
    echo -e "${BLUE}📋 BUILD COMMANDS${NC}"
    echo -e "  ${GREEN}./build.sh build [config]${NC}    Build the application (default: Release)"
    echo -e "  ${GREEN}./build.sh restore${NC}           Restore NuGet dependencies"
    echo -e "  ${GREEN}./build.sh clean${NC}             Clean build artifacts"
    echo -e "  ${GREEN}./build.sh test${NC}              Run unit tests"
    echo
    echo -e "${BLUE}🐛 DEBUG COMMANDS${NC}"
    echo -e "  ${GREEN}./build.sh debug api${NC}         Debug API only"
    echo -e "  ${GREEN}./build.sh debug blazor${NC}      Debug Blazor UI only"
    echo -e "  ${GREEN}./build.sh debug full${NC}        Debug both API and Blazor"
    echo -e "  ${GREEN}./build.sh debug aspire${NC}      Debug with .NET Aspire orchestration"
    echo
    echo -e "${BLUE}🐳 DOCKER COMMANDS${NC}"
    echo -e "  ${GREEN}./build.sh docker${NC}            Run full stack with Docker"
    echo -e "  ${GREEN}./build.sh docker-bg${NC}         Run Docker in background"
    echo -e "  ${GREEN}./build.sh docker-stop${NC}       Stop Docker containers"
    echo
    echo -e "${BLUE}📊 INFORMATION${NC}"
    echo -e "  ${GREEN}./build.sh status${NC}            Show project status"
    echo -e "  ${GREEN}./build.sh help${NC}              Show this help message"
    echo
    echo -e "${BLUE}🎯 EXAMPLES${NC}"
    echo -e "  ${YELLOW}./build.sh restore && ./build.sh build Debug${NC}"
    echo -e "  ${YELLOW}./build.sh debug aspire${NC}                      # With Aspire dashboard"
    echo -e "  ${YELLOW}./build.sh debug full${NC}                        # API + Blazor"
    echo -e "  ${YELLOW}DEBUG_PORT=8001 ./build.sh debug api${NC}        # Custom port"
    echo
    echo -e "${BLUE}📚 DOCUMENTATION${NC}"
    echo -e "  See BUILD_README.md for detailed documentation"
    echo -e "  See ASPIRE.md for .NET Aspire specific information"
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
    "help"|"-h"|"--help")
        show_help
        ;;
    *)
        print_error "Unknown command: $1"
        echo
        show_help
        exit 1
        ;;
esac
