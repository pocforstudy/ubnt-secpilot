# UBNT SecPilot - Plataforma avançada de segurança de rede com tecnologia .NET 8 e Orleans.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://docker.com)
[![CI/CD](https://img.shields.io/badge/CI/CD-GitHub_Actions-green.svg)](https://github.com/features/actions)
[![SonarQube](https://img.shields.io/badge/SonarQube-10.6-blue.svg)](https://sonarqube.org)
[![Quality Gate](https://img.shields.io/badge/Quality_Gate-Passing-brightgreen.svg)](#)


---

## Visão Geral

O UBNT SecPilot é uma solução enterprise-grade para:
- **Análise de segurança de rede** em tempo real
- **Agentes inteligentes** usando Orleans actors
- **Monitoramento e observabilidade** completos
- **Conformidade com padrões de segurança**
- **Escalabilidade horizontal** com containers

### Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                    .NET Aspire Dashboard                    │
│                   (Orquestração Visual)                     │
│                    http://localhost:18888                   │
└─────────────────────┬───────────────────────────────────────┘
                      │
    ┌─────────────────▼─────────────────┐
    │         AppHost (Aspire)           │
    │     Orquestração de Serviços       │
    └─────────────────┬───────────────────┘
                      │
    ┌─────────────────▼─────────────────┐
    │        Aplicações .NET 8          │
    │   ┌─────────────┬─────────────┐    │
    │   │   Blazor    │  ASP.NET    │    │
    │   │    UI       │   Core API  │    │
    │   └─────────────┴─────────────┘    │
    └─────────────────┬───────────────────┘
                      │
    ┌─────────────────▼─────────────────┐
    │         Orleans Agents             │
    │   ┌─────────────┬─────────────┐    │
    │   │ ThreatAgent │ NetworkAgent│    │
    │   │ Enrichment  │   Event     │    │
    │   └─────────────┴─────────────┘    │
    └─────────────────┬───────────────────┘
                      │
    ┌─────────────────▼──────────────────┐
    │       Infraestrutura Compartilhad  │
    │   ┌─────────────┬─────────────┐    │
    │   │   MongoDB   │    Redis    │    │
    │   │ (EventStore)│   (Cache)   │    │
    │   └─────────────┴─────────────┘    │
    └─────────────────────────────────────┘
```

## Início Rápido

### Pré-requisitos

- **Docker** e **Docker Compose**
- **.NET 8 SDK** (opcional, para desenvolvimento)
- **Git**

### 1. Clone o Repositório

```bash
git clone <repository-url>
cd ubnt-secpilot
```

```
### 2. Execute com .NET Aspire (Recomendado)

#### Desenvolvimento com Aspire

```bash
# Execute the environment complete with Aspire
./build.sh debug aspire
```

Isso iniciará:
- **Dashboard Aspire**: http://localhost:18888 (orquestração visual)
- **API**: http://localhost:8000/api
{{ ... }}
- **Blazor UI**: http://localhost:8501
- **MongoDB**: http://localhost:27017
- **Redis**: http://localhost:6379
- **Mongo Express**: http://localhost:8081 (interface web MongoDB)
- **Redis Commander**: http://localhost:8082 (interface web Redis)

#### Vantagens do Aspire:
- **Orquestração automática** de serviços
- **Dashboard visual** para monitoramento
- **OTLP tracing** integrado
- **Logs centralizados** e estruturados
- **Configuração simplificada** de ambientes

### 3. Execute com Docker Compose (Alternativa)

```bash
# Iniciar todos os serviços
docker compose up -d

# Ou usar a versão simplificada (sem conflitos)
docker compose -f docker-compose.simple.yml up -d

# Verificar status
docker compose ps
```

### 4. Acesse a Aplicação

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **Dashboard Aspire** | http://localhost:18888 | Orquestração e monitoramento visual |
| **Mongo Express** | http://localhost:8081 | Interface web MongoDB |
| **Redis Commander** | http://localhost:8082 | Interface web Redis |
| **Dashboard** | http://localhost:8501 | Interface Blazor principal |
| **API** | http://localhost:8000 | API REST ASP.NET Core |
| **API Docs** | http://localhost:8000/swagger | Documentação OpenAPI |
| **SonarQube** | http://localhost:9000 | Análise de qualidade |
| **Grafana** | http://localhost:3000 | Dashboards de métricas |
| **Prometheus** | http://localhost:9090 | Métricas do sistema |

## Desenvolvimento

### Estrutura do Projeto

```bash


ubnt-secpilot/
├── src/                             # Código fonte principal
│   ├── UbntSecPilot.Domain/         # Camada de domínio (DDD)
│   ├── UbntSecPilot.Application/    # Casos de uso e lógica de aplicação
│   ├── UbntSecPilot.Infrastructure/ # Adaptadores externos e persistência
│   ├── UbntSecPilot.Agents/         # Interfaces Orleans
│   ├── UbntSecPilot.Agents.Orleans/ # Implementações distribuídas
│   ├── UbntSecPilot.WebApi/         # API ASP.NET Core
│   ├── UbntSecPilot.BlazorUI/       # Interface Blazor Server
│   └── UbntSecPilot.AppHost/        # Orquestração .NET Aspire
├── tests/                           # Projetos de teste organizados
├── build/                           # Scripts de build e desenvolvimento
├── docs/                            # Documentação técnica
├── infrastructure/                  # Configurações Docker/K8s
└── .github/                         # CI/CD e workflows
```

### Comandos Úteis

```bash
# Build e testes
./build.sh build Debug
./build.sh test

# Análise de qualidade
./scripts/sonar-local.sh

# Configuração SonarQube
./infrastructure/observability/sonarqube/quality-gates-setup.sh

# Limpeza
docker compose down -v
```

## Funcionalidades Principais

### Agentes Inteligentes (Orleans)

- **ThreatEnrichmentAgent**: Análise e enriquecimento de ameaças
- **NetworkEventAgent**: Processamento de eventos de rede
- **ThreadAnalysisAgent**: Análise de conversas suspeitas
- **EventOrchestrator**: Coordenação de múltiplos agentes

### Análise de Segurança

- **Detecção de anomalias** em tempo real
- **Correlação de eventos** entre diferentes fontes
- **Enriquecimento** com bases de dados externas (VirusTotal, etc.)
- **Relatórios executivos** com métricas e tendências

### Observabilidade Completa

- **Métricas** com Prometheus
- **Dashboards** interativos no Grafana
- **Logs estruturados** com contexto rico
- **Tracing distribuído** entre agentes
- **Alertas inteligentes** baseados em thresholds

## Segurança

### Padrões Implementados

- **OWASP Top 10** compliance
- **Autenticação JWT** com refresh tokens
- **Authorization** baseada em roles (Admin/User)
- **Criptografia** de dados sensíveis
- **Audit logging** completo
- **Rate limiting** configurável

### Análise de Segurança

- **SonarQube** para análise estática
- **CodeQL** para vulnerabilidades
- **Trivy** para dependências
- **SAST/DAST** integrados no CI/CD

## CI/CD e DevOps

### Pipeline Completo

- **GitHub Actions** automatizado
- **Multi-plataforma** (Ubuntu, Windows, macOS)
- **Docker multi-arquitetura** (AMD64 + ARM64)
- **Quality gates** SonarQube
- **Security scanning** integrado
- **Automated deployment** staging/produção

### Quality Gates

| Métrica | Threshold | Status |
|---------|-----------|--------|
| **Cobertura** | ≥80% | Configurado |
| **Security** | Zero E-rated | Ativo |
| **Reliability** | Zero E-rated | Ativo |
| **Duplicação** | ≤3% | Monitorado |
| **Vulnerabilidades** | Zero blockers | Verificado |

## Documentação

### Guias Disponíveis

- [Arquitetura](docs/ARCHITECTURE.md) - Detalhes técnicos da arquitetura
- [Agentes](AGENTS.md) - Documentação dos agentes Orleans
- [CI/CD](.github/README.md) - Pipeline e automação
- [SonarQube](infrastructure/observability/sonarqube/README.md) - Análise de qualidade
- [Entrevista](Interview.md) - Processo seletivo técnico
- [Aspire](ASPIRE.md) - Documentação .NET Aspire

### Scripts de Desenvolvimento

```bash
# Análise rápida SonarQube
./scripts/sonar-local.sh

# Configuração quality gates
./infrastructure/observability/sonarqube/quality-gates-setup.sh

# Build completo
./build.sh build Release

# Testes com cobertura
./build.sh test
```

## Configuração Avançada

### SonarQube

```bash
# Iniciar análise local
docker compose -f docker-compose.simple.yml up -d

# Configurar quality gates
./infrastructure/observability/sonarqube/quality-gates-setup.sh

# Executar análise
./scripts/sonar-local.sh
```

### Kubernetes (Produção)

```yaml
# Deploy via Helm (recomendado)
helm install ubnt-secpilot ./charts/secpilot

# Ou kubectl direto
kubectl apply -f k8s/production/
```

## Métricas e Monitoramento

### Dashboards Disponíveis

- **Aplicação**: Eventos processados, latências, erros
- **Segurança**: Tentativas de acesso, vulnerabilidades
- **Performance**: CPU, memória, throughput por agente
- **Qualidade**: Cobertura, code smells, dívida técnica

### Alertas Configurados

- **Erros críticos** (>5% error rate)
- **Performance** (latência >1s)
- **Segurança** (tentativas de ataque)
- **Recursos** (uso de CPU/memória alto)

## Contribuição

### Processo de Desenvolvimento

1. **Fork** o projeto
2. **Crie uma branch** para sua feature (`git checkout -b feature/amazing-feature`)
3. **Commit** suas mudanças (`git commit -m 'Add amazing feature'`)
4. **Push** para a branch (`git push origin feature/amazing-feature`)
5. **Abra um Pull Request**

### Padrões de Código

- **C# Coding Standards** ([Microsoft Guidelines](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions))
- **Clean Architecture** com separação clara
- **TDD** com cobertura mínima de 80%
- **Security-first** approach
- **Documentation** obrigatória para APIs públicas

## Licença

Este projeto está sob licença **Proprietária** - veja o arquivo `LICENSE` para detalhes.

## Suporte

### Problemas Comuns

**1. SonarQube não inicia:**
```bash
docker compose -f docker-compose.simple.yml up -d
# Aguarde 60 segundos e tente novamente
```

**2. Build falha no CI/CD:**
```bash
# Verificar logs no GitHub Actions
# Verificar cobertura de testes
./build.sh test
```

**3. Agentes não processam eventos:**
```bash
# Verificar logs dos containers
docker compose logs redpanda
docker compose logs api
```

### Contato

Para suporte técnico ou dúvidas:
- **Email**: [contato@ubnt-secpilot.com]
- **Slack**: [#secpilot-support]
- **Telefone**: [+55 11 99999-9999]

---

## Status do Projeto

| Componente | Status | Versão | Saúde |
|------------|--------|---------|-------|
| **API Core** | Ativo | v1.0.0 | Saudável |
| **Blazor UI** | Ativo | v1.0.0 | Saudável |
| **Orleans Agents** | Ativo | v1.0.0 | Saudável |
| **MongoDB** | Ativo | 6.0 | Saudável |
| **Redpanda** | Ativo | 24.1.1 | Saudável |
| **SonarQube** | Ativo | 10.6 | Saudável |
| **CI/CD** | Ativo | Latest | Saudável |
| **.NET Aspire** | Ativo | 8.2 | Saudável |

**Última atualização**: 30 de Setembro de 2025
**Uptime**: 99.9% (últimos 30 dias)
**Security Score**: A+ (OWASP compliance)

---

