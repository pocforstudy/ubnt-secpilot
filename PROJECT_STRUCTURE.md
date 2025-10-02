# Estrutura do Projeto UBNT SecPilot

## Organização por Clean Architecture

Este projeto segue os princípios da **Clean Architecture** com organização clara por responsabilidades e dependências.

## Estrutura de Pastas

```
ubnt-secpilot/
├── 📁 src/                          # Código fonte principal
│   ├── 📁 UbntSecPilot.Domain/      # Camada de domínio (regras de negócio puras)
│   │   ├── Models/                  # Entidades e value objects
│   │   ├── Services/                # Serviços de domínio
│   │   ├── Repositories/            # Interfaces de repositório
│   │   └── Events/                  # Eventos de domínio
│   │
│   ├── 📁 UbntSecPilot.Application/ # Camada de aplicação (casos de uso)
│   │   ├── Commands/                # Comandos CQRS
│   │   ├── Queries/                 # Consultas CQRS
│   │   ├── Handlers/                # Handlers MediatR
│   │   ├── Services/                # Serviços de aplicação
│   │   └── DTOs/                    # Data Transfer Objects
│   │
│   ├── 📁 UbntSecPilot.Infrastructure/ # Camada de infraestrutura
│   │   ├── Repositories/            # Implementações de repositório
│   │   ├── Services/                # Serviços externos (HTTP clients)
│   │   ├── Data/                    # Contextos de dados e configurações
│   │   └── Transactions/            # Gerenciamento de transações
│   │
│   ├── 📁 UbntSecPilot.Agents/      # Interfaces Orleans (contrato)
│   │   ├── Interfaces/              # Definições de grains
│   │   └── Models/                  # DTOs para comunicação distribuída
│   │
│   ├── 📁 UbntSecPilot.Agents.Orleans/ # Implementações distribuídas
│   │   ├── Grains/                  # Implementações de grains
│   │   ├── Services/                # Serviços distribuídos
│   │   └── Transactions/            # Controle de transações distribuídas
│   │
│   ├── 📁 UbntSecPilot.WebApi/      # Camada de apresentação (API)
│   │   ├── Controllers/             # Controllers REST
│   │   ├── Hosted/                  # Serviços hospedados em background
│   │   ├── Filters/                 # Filtros e middleware
│   │   └── Properties/              # Configurações do projeto
│   │
│   ├── 📁 UbntSecPilot.BlazorUI/    # Interface web
│   │   ├── Components/              # Componentes Blazor
│   │   ├── Pages/                   # Páginas da aplicação
│   │   ├── Services/                # Serviços de integração
│   │   └── wwwroot/                 # Assets estáticos
│   │
│   └── 📁 UbntSecPilot.AppHost/     # Orquestração .NET Aspire
│       ├── Program.cs               # Configuração de serviços
│       └── appsettings.json        # Configurações Aspire
│
├── 📁 tests/                        # Projetos de teste
│   ├── UbntSecPilot.Application.Tests/ # Testes de aplicação
│   └── UbntSecPilot.Orleans.Tests/  # Testes distribuídos
│
├── 📁 build/                        # Scripts e ferramentas
│   ├── build.sh                     # Script principal de build
│   ├── setup.sh                     # Script de instalação
│   └── scripts/                     # Scripts auxiliares
│
├── 📁 docs/                         # Documentação técnica
│   ├── images/                      # Diagramas e imagens
│   ├── ARCHITECTURE.md              # Documentação de arquitetura
│   ├── ASPIRE.md                    # Guia .NET Aspire
│   └── API.md                       # Documentação da API
│
├── 📁 infrastructure/               # Infraestrutura e DevOps
│   ├── observability/               # SonarQube, Prometheus, Grafana
│   ├── kubernetes/                  # Manifestos K8s
│   └── docker/                      # Configurações Docker
│
└── 📁 .github/                      # CI/CD e comunidade
    ├── workflows/                   # GitHub Actions
    └── README.md                    # Guia de contribuição
```

## Princípios de Organização

### Clean Architecture
- **Dependências apontam para dentro**: Camadas externas dependem de internas
- **Independência de frameworks**: Lógica de negócio isolada
- **Testabilidade**: Cada camada pode ser testada independentemente

### Diretrizes de Nomenclatura
- **Namespaces** seguem padrão `Company.Product.Layer`
- **Classes** usam PascalCase
- **Métodos** usam PascalCase
- **Variáveis privadas** usam camelCase com underscore

### Estrutura de Projeto
- **Cada projeto** tem responsabilidade única
- **Referências** seguem fluxo de dependências
- **Testes** organizados por camada
- **Configurações** centralizadas por ambiente

## Fluxo de Dependências

```
┌─────────────────────────────────────┐
│        UbntSecPilot.WebApi          │ ←── Presentation Layer
└─────────────────┬───────────────────┘
                  │ Depends on ↓
┌─────────────────▼───────────────────┐
│     UbntSecPilot.Application        │ ←── Application Layer
└─────────────────┬───────────────────┘
                  │ Depends on ↓
┌─────────────────▼───────────────────┐
│      UbntSecPilot.Domain            │ ←── Domain Layer
└─────────────────┬───────────────────┘
                  │ Depends on ↓
┌─────────────────▼───────────────────┐
│   UbntSecPilot.Infrastructure       │ ←── Infrastructure Layer
└─────────────────────────────────────┘
```

## Organização Física vs Lógica

### Física (Estrutura de Arquivos)
- **Pastas por projeto** (.csproj)
- **Separação clara** entre código e testes
- **Build outputs** fora da estrutura fonte

### Lógica (Namespaces)
- **Company.Product.Layer** (UbntSecPilot.Domain)
- **Funcionalidade específica** dentro da camada
- **Agrupamento por responsabilidade**

## Desenvolvimento e Manutenção

### Adicionar Nova Funcionalidade
1. **Identificar camada** apropriada
2. **Criar interfaces** na camada de domínio
3. **Implementar** na camada específica
4. **Adicionar testes** na camada correspondente
5. **Atualizar documentação**

### Refatoração
1. **Analisar impacto** nas dependências
2. **Atualizar interfaces** se necessário
3. **Manter compatibilidade** com versões anteriores
4. **Executar testes** completos

## Benefícios da Organização

### Para Desenvolvedores
- **Navegação fácil** e intuitiva
- **Localização rápida** de código
- **Testabilidade alta** por camada
- **Debug simplificado** por responsabilidade

### Para Arquitetura
- **Manutenibilidade** alta
- **Escalabilidade** preparada
- **Evolução independente** por camada
- **Reutilização** de componentes

### Para DevOps
- **Build eficiente** por projeto
- **Deploy granular** por serviço
- **Monitoramento** por camada
- **CI/CD** bem estruturado
