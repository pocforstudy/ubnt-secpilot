# Docker Files Organization

Esta pasta contém todos os arquivos Docker necessários para o projeto UBNT SecPilot.

## Arquivos

### `Dockerfile`
- **Descrição**: Dockerfile principal para a API ASP.NET Core
- **Imagem Base**: .NET 8 SDK para build + runtime
- **Funcionalidades**:
  - Multi-stage build (build + runtime)
  - Otimizado para produção
  - Health checks integrados
  - Configuração de ambiente

### `Dockerfile.BlazorUI`
- **Descrição**: Dockerfile para a interface Blazor Server
- **Imagem Base**: .NET 8 runtime (já que é apenas execução)
- **Funcionalidades**:
  - Otimizado para UI web
  - Configuração Nginx para servir conteúdo estático
  - Health checks para monitoramento

## Uso nos Docker Compose

Os Dockerfiles são referenciados nos arquivos `docker-compose.yml`:

```yaml
services:
  api:
    build:
      context: .
      dockerfile: docker/Dockerfile

  dashboard:
    build:
      context: .
      dockerfile: docker/Dockerfile.BlazorUI
```

## Convenções

- **Context**: Sempre usar `.` (raiz do projeto)
- **Caminhos**: Referenciar como `docker/Dockerfile`
- **Build Args**: Definir variáveis necessárias para cada ambiente
- **Labels**: Incluir metadados para organização

## Desenvolvimento

Para modificar os Dockerfiles:

1. Edite os arquivos nesta pasta
2. Teste localmente: `docker build -f docker/Dockerfile .`
3. Atualize se necessário nos ambientes de CI/CD
4. Documente mudanças no changelog
