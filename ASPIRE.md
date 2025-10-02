# .NET Aspire - UBNT SecPilot

## Visão Geral

O UBNT SecPilot utiliza **.NET Aspire 8.2** para desenvolvimento distribuído moderno, proporcionando:

- **Orquestração automática** de serviços
- **Dashboard visual** para monitoramento
- **OTLP tracing** integrado
- **Logs centralizados** e estruturados
- **Ambiente de desenvolvimento** completo

## Arquitetura Aspire

```
┌─────────────────────────────────────┐
│       Aspire Dashboard              │
│      (localhost:18888)              │
└─────────────────┬───────────────────┘
                  │
    ┌─────────────▼─────────────┐
    │     AppHost Project       │
    │  (UbntSecPilot.AppHost)   │
    └─────────────┬─────────────┘
                  │
    ┌─────────────▼─────────────┐
    │     Projeto Services      │
    │   (API + Blazor + Orleans)│
    └─────────────┬─────────────┘
                  │
    ┌─────────────▼─────────────┐
    │    Recursos Compartilhados│
    │  (MongoDB, Redis, etc.)   │
    └───────────────────────────┘
```

## Projetos Aspire

### UbntSecPilot.AppHost
- **Projeto de orquestração** principal
- **Configuração de serviços** e dependências
- **Recursos compartilhados** (MongoDB, Redis)
- **Dashboard integrado**

### Recursos Configurados

#### Banco de Dados
- **MongoDB 7.0** com Mongo Express
- **Redis 7.2** com Redis Commander
- **Dados persistentes** via volumes Docker

#### Aplicações
- **API ASP.NET Core** com endpoints externos
- **Blazor Server** com interface web
- **Orleans Silo** para processamento distribuído

## Comandos Disponíveis

### Desenvolvimento com Aspire

```bash
# Iniciar ambiente completo
./build.sh debug aspire

# Apenas AppHost (se API/Blazor já estiverem rodando)
cd UbntSecPilot.AppHost
dotnet run
```

### Acessar Serviços

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **Dashboard Aspire** | http://localhost:18888 | Orquestração visual |
| **API** | http://localhost:8000/api | APIs REST |
| **Blazor UI** | http://localhost:8501 | Interface web |
| **Mongo Express** | http://localhost:8081 | Interface MongoDB |
| **Redis Commander** | http://localhost:8082 | Interface Redis |

## Recursos Avançados

### Tracing Distribuído (OTLP)
- **Endpoint OTLP**: http://localhost:18888
- **Tracing automático** entre serviços
- **Visualização de chamadas** no dashboard

### Logs Estruturados
- **Logs centralizados** no dashboard
- **Correlação automática** entre serviços
- **Filtragem avançada** por serviço/severidade

### Métricas e Health Checks
- **Health checks automáticos** de todos os serviços
- **Métricas em tempo real** no dashboard
- **Alertas configuráveis** por serviço

## Configuração

### Arquivos de Configuração

#### aspire-settings.json
```json
{
  "orchestration": {
    "mongodb": {
      "databaseName": "UbntSecPilot",
      "collections": ["NetworkEvents", "ThreatFindings"]
    }
  }
}
```

#### Program.cs (AppHost)
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongodb")
    .WithMongoExpress();

var api = builder.AddProject<Projects.UbntSecPilot_WebApi>("api")
    .WithReference(mongodb);
```

## Benefícios do Aspire

### Para Desenvolvimento
- **Ambiente consistente** entre desenvolvedores
- **Inicialização rápida** com um comando
- **Debug facilitado** com tracing visual
- **Logs centralizados** e pesquisáveis

### Para Produção
- **Mesmas configurações** de desenvolvimento
- **Transição suave** dev → staging → produção
- **Configuração como código**
- **Infraestrutura imutável**

## Troubleshooting

### Problemas Comuns

**1. Dashboard não carrega:**
```bash
# Verificar se a porta 18888 está disponível
netstat -tuln | grep 18888

# Reiniciar o ambiente
./build.sh debug aspire
```

**2. Serviços não iniciam:**
```bash
# Verificar logs do AppHost
cd UbntSecPilot.AppHost
dotnet run --verbosity detailed
```

**3. Conexão entre serviços falha:**
```bash
# Verificar se todos os serviços estão saudáveis
curl http://localhost:8000/health
curl http://localhost:8501/health
```

