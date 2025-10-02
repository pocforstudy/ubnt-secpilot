# Pasta Tests - Organização dos Projetos de Teste

Esta pasta contém todos os projetos de teste do UBNT SecPilot, organizados de forma clara e profissional.

## Estrutura

```
tests/
├── UbntSecPilot.Application.Tests/     # Testes da camada Application
│   ├── UbntSecPilot.Application.Tests.csproj
│   ├── Unit/                           # Testes unitários
│   └── Integration/                    # Testes de integração
└── UbntSecPilot.Orleans.Tests/         # Testes da camada Orleans
    ├── UbntSecPilot.Orleans.Tests.csproj
    ├── Unit/                           # Testes unitários dos agentes
    └── Integration/                    # Testes de integração distribuída
```

## Projetos de Teste

### UbntSecPilot.Application.Tests
- **Responsabilidade**: Testar casos de uso e handlers MediatR
- **Framework**: xUnit
- **Cobertura**: Casos de uso, validações, regras de negócio
- **Mocks**: Repositórios, serviços externos, Orleans client

### UbntSecPilot.Orleans.Tests
- **Responsabilidade**: Testar agentes Orleans e lógica distribuída
- **Framework**: xUnit com Orleans TestHost
- **Cobertura**: Grains, algoritmos de análise, comunicação distribuída
- **Mocks**: Orleans silo, repositórios externos

## Cobertura de Testes

### Objetivos de Cobertura
- **Unit Tests**: ≥90% cobertura
- **Integration Tests**: ≥80% cobertura
- **Critical Paths**: 100% cobertura

### Ferramentas Utilizadas
- **xUnit**: Framework de testes
- **Moq**: Mocking framework
- **FluentAssertions**: Asserções legíveis
- **Coverlet**: Análise de cobertura

## Execução dos Testes

### Comandos Disponíveis

```bash
# Executar todos os testes
./build.sh test

# Executar apenas testes específicos
dotnet test tests/UbntSecPilot.Application.Tests/UbntSecPilot.Application.Tests.csproj
dotnet test tests/UbntSecPilot.Orleans.Tests/UbntSecPilot.Orleans.Tests.csproj

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Com filtro específico
dotnet test --filter "Category=Unit"
```

### CI/CD Integration

Os testes são executados automaticamente no pipeline GitHub Actions:

```yaml
- name: Run Tests
  run: ./build.sh test

- name: Upload Coverage
  uses: codecov/codecov-action@v3
```

## Boas Práticas

### Organização dos Testes

#### Testes Unitários
- **Um teste por método público**
- **Arrange-Act-Assert** padrão
- **Nomenclatura clara**: `MethodName_Should_ExpectedBehavior_When_StateUnderTest`

#### Testes de Integração
- **Testar fluxos completos**
- **Usar dados reais quando possível**
- **Limpar dados entre testes**

### Convenções de Nomenclatura

```csharp
// ✅ Correto
public class ThreatAnalysisServiceTests
{
    [Fact]
    public void AnalyzeThreat_Should_ReturnAnalysisResult_When_ValidInput()
    {
        // Arrange
        var service = new ThreatAnalysisService(mockRepository);

        // Act
        var result = service.AnalyzeThreat(threatData);

        // Assert
        result.Should().NotBeNull();
    }
}
```

## Troubleshooting

### Problemas Comuns

**1. Testes não encontram projetos movidos:**
```bash
# Atualizar referências na solution
dotnet sln UbntSecPilot.sln remove tests/UbntSecPilot.Application.Tests/UbntSecPilot.Application.Tests.csproj
dotnet sln UbntSecPilot.sln add tests/UbntSecPilot.Application.Tests/UbntSecPilot.Application.Tests.csproj
```

**2. Cobertura baixa:**
```bash
# Analisar cobertura detalhada
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

**3. Testes de integração falham:**
```bash
# Verificar se serviços externos estão rodando
docker compose ps

# Verificar configurações de ambiente
cat .env
```
