# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0.10 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 8000
EXPOSE 8501

# Use the SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0.403 AS build
WORKDIR /src

# Copy project files
COPY ["UbntSecPilot.WebApi/UbntSecPilot.WebApi.csproj", "UbntSecPilot.WebApi/"]
COPY ["UbntSecPilot.Application/UbntSecPilot.Application.csproj", "UbntSecPilot.Application/"]
COPY ["UbntSecPilot.Domain/UbntSecPilot.Domain.csproj", "UbntSecPilot.Domain/"]
COPY ["UbntSecPilot.Infrastructure/UbntSecPilot.Infrastructure.csproj", "UbntSecPilot.Infrastructure/"]
COPY ["UbntSecPilot.BlazorUI/UbntSecPilot.BlazorUI.csproj", "UbntSecPilot.BlazorUI/"]

# Restore dependencies
RUN dotnet restore "UbntSecPilot.WebApi/UbntSecPilot.WebApi.csproj"
RUN dotnet restore "UbntSecPilot.BlazorUI/UbntSecPilot.BlazorUI.csproj"

# Copy source code
COPY . .

# Build the applications
WORKDIR "/src/UbntSecPilot.WebApi"
RUN dotnet build "UbntSecPilot.WebApi.csproj" -c Release -o /app/build

WORKDIR "/src/UbntSecPilot.BlazorUI"
RUN dotnet build "UbntSecPilot.BlazorUI.csproj" -c Release -o /app/build

# Publish the applications
FROM build AS publish
WORKDIR "/src/UbntSecPilot.WebApi"
RUN dotnet publish "UbntSecPilot.WebApi.csproj" -c Release -o /app/publish

WORKDIR "/src/UbntSecPilot.BlazorUI"
RUN dotnet publish "UbntSecPilot.BlazorUI.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UbntSecPilot.WebApi.dll"]
