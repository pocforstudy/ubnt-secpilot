# 🛠️ UBNT SecPilot Build Script

A comprehensive build and development script for the UBNT SecPilot .NET Core 8 application. This script provides utilities for building, running, debugging, and managing the application in various environments.

## 🚀 Quick Start

### Basic Usage
```bash
# Show available commands
./build.sh help

# Build the application
./build.sh build

# Run API in debug mode
./build.sh debug api

# Run full application with Docker
./build.sh docker
```

## 📋 Available Commands

### 🔧 Build Commands
```bash
./build.sh build [config]    # Build the application (default: Release)
./build.sh restore           # Restore NuGet dependencies
./build.sh clean             # Clean build artifacts
./build.sh test              # Run unit tests
```

### 🐛 Debug Commands
```bash
./build.sh debug api         # Debug API only (default)
./build.sh debug blazor      # Debug Blazor UI only
./build.sh debug full        # Debug both API and Blazor
```

### 🐳 Docker Commands
```bash
./build.sh docker            # Run full stack with Docker
./build.sh docker-bg         # Run with Docker in background
./build.sh docker-stop       # Stop Docker containers
```

### 📊 Information Commands
```bash
./build.sh status            # Show project status
./build.sh help              # Show help message
```

## 🎯 Common Development Workflows

### **Development Setup**
```bash
# 1. Restore dependencies
./build.sh restore

# 2. Build in debug mode
./build.sh build Debug

# 3. Run API for development
./build.sh debug api
```

### **Full Stack Development**
```bash
# Run both API and Blazor UI
./build.sh debug full
```

### **Production Deployment**
```bash
# Build for production
./build.sh build Release

# Deploy with Docker
./build.sh docker
```

### **Testing**
```bash
# Run all tests
./build.sh test

# Build and test
./build.sh build Debug && ./build.sh test
```

## 🔧 Configuration

### Environment Variables
```bash
# Custom ports (optional)
export DEBUG_PORT=8001          # API debug port (default: 8000)
export BLAZOR_PORT=8502         # Blazor debug port (default: 8501)
export BUILD_CONFIG=Debug       # Build configuration (default: Release)

# Run with custom configuration
DEBUG_PORT=8001 BLAZOR_PORT=8502 ./build.sh debug full
```

### Project Structure
```
ubnt-secpilot/
├── UbntSecPilot.WebApi/        # API project
├── UbntSecPilot.BlazorUI/      # Blazor UI project
├── UbntSecPilot.Infrastructure/ # Infrastructure layer
├── UbntSecPilot.Application/   # Application layer
├── UbntSecPilot.Domain/        # Domain layer
├── docker-compose.yml          # Docker configuration
├── build.sh                    # This script
└── UbntSecPilot.sln           # Solution file (optional)
```

## 🐛 Debug Mode Details

### **API Debug Mode**
- **URL**: `http://localhost:8000` (configurable)
- **Swagger UI**: `http://localhost:8000/swagger`
- **Stream Monitor**: `http://localhost:8000/stream-monitor.html`
- **Hot Reload**: Enabled for development
- **Debugging**: Full debugging support

### **Blazor Debug Mode**
- **URL**: `http://localhost:8501` (configurable)
- **Hot Reload**: Enabled for development
- **Debugging**: Full debugging support

### **Full Debug Mode**
- **API**: `http://localhost:8000`
- **Blazor UI**: `http://localhost:8501`
- **Both services run simultaneously**

## 🐳 Docker Integration

### **Full Stack with Docker**
```bash
# Start all services
./build.sh docker

# Services available:
# - API: http://localhost:8000
# - Blazor UI: http://localhost:8501
# - Grafana: http://localhost:3000
# - Prometheus: http://localhost:9090
# - Redpanda: localhost:9092
```

### **Background Mode**
```bash
# Start in background
./build.sh docker-bg

# View logs
docker-compose logs -f

# Stop services
./build.sh docker-stop
```

## 🧪 Testing

### **Run All Tests**
```bash
./build.sh test
```

### **Build and Test**
```bash
./build.sh build Debug && ./build.sh test
```

### **Test with Coverage** (if configured)
```bash
dotnet test --configuration Debug --collect:"XPlat Code Coverage"
```

## 📊 Status and Information

### **Project Status**
```bash
./build.sh status
```

Shows:
- ✅ Project structure validation
- ✅ Dependency availability (.NET, Docker)
- ✅ Available commands
- ✅ Configuration summary

### **Help**
```bash
./build.sh help
```

Shows detailed help with examples and configuration options.

## 🔍 Troubleshooting

### **Common Issues**

#### **.NET SDK Not Found**
```bash
# Install .NET 8 SDK
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

#### **Permission Denied**
```bash
# Make script executable
chmod +x build.sh
```

#### **Docker Not Available**
```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
```

#### **Port Already in Use**
```bash
# Use custom ports
DEBUG_PORT=8001 BLAZOR_PORT=8502 ./build.sh debug full

# Or kill existing processes
sudo lsof -ti:8000 | xargs kill -9
```

### **Debug Information**
```bash
# Check .NET installation
dotnet --version
dotnet --info

# Check Docker status
docker version
docker-compose version

# Check ports
netstat -tlnp | grep :8000
```

## 🚀 Advanced Usage

### **Custom Build Configuration**
```bash
# Build with specific configuration
./build.sh build Debug

# Run with custom ports
DEBUG_PORT=8001 BLAZOR_PORT=8502 ./build.sh debug full
```

### **CI/CD Integration**
```bash
#!/bin/bash
# Example CI/CD script

# Build
./build.sh build Release

# Test
./build.sh test

# Deploy with Docker
./build.sh docker-bg

# Health check
curl http://localhost:8000/health
```

### **Development Workflow**
```bash
#!/bin/bash
# Example development workflow

# Restore and build
./build.sh restore && ./build.sh build Debug

# Run tests
./build.sh test

# Start development servers
./build.sh debug full &

# Wait for startup
sleep 10

# Run integration tests
curl http://localhost:8000/health
curl http://localhost:8501/health
```

## 📝 Script Features

### **Safety Features**
- ✅ **Error handling**: Stops on any error
- ✅ **Dependency validation**: Checks prerequisites
- ✅ **Colored output**: Easy to read status messages
- ✅ **Help system**: Comprehensive documentation

### **Development Features**
- ✅ **Hot reload**: Instant updates during development
- ✅ **Multiple modes**: API, Blazor, or both
- ✅ **Flexible ports**: Configurable development ports
- ✅ **Clean builds**: Removes artifacts between builds

### **Docker Integration**
- ✅ **Full stack**: All services with one command
- ✅ **Background mode**: Run without blocking terminal
- ✅ **Easy management**: Start/stop with simple commands
- ✅ **Health monitoring**: Built-in health checks

### **Testing Support**
- ✅ **Unit tests**: Run all test projects
- ✅ **Integration tests**: Full application testing
- ✅ **Test reporting**: Clear test results
- ✅ **Coverage support**: Code coverage integration

This build script provides a complete development environment for the UBNT SecPilot application, supporting everything from local development to production deployment! 🎉
