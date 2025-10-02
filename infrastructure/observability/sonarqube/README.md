#  SonarQube Integration - UBNT SecPilot

##  Visão Geral

SonarQube foi integrado ao pipeline de CI/CD para fornecer análise de qualidade de código avançada, incluindo métricas de cobertura, análise de segurança, detecção de code smells e muito mais.

## 🚀 SonarQube no Docker Compose

SonarQube está disponível em desenvolvimento através do Docker Compose:

```bash
# Iniciar SonarQube
docker-compose up -d sonarqube sonarqube-db

# Verificar status
docker-compose ps sonarqube

# Acessar interface web
open http://localhost:9000

# Credenciais padrão:
# Username: admin
# Password: admin
```

##  Configuração Inicial

### 1. Primeiro Acesso
- Acesse: http://localhost:9000
- Username: `admin`
- Password: `admin`
- **Altere a senha** após o primeiro login

### 2. Criar Projeto
```bash
# Via SonarQube Scanner CLI (em CI/CD)
sonar-scanner \
  -Dsonar.projectKey=ubnt-secpilot \
  -Dsonar.sources=. \
  -Dsonar.host.url=http://localhost:9000 \
  -Dsonar.login=your-token-here
```

### 3. Quality Gates
Execute o script de configuração:
```bash
chmod +x infrastructure/observability/sonarqube/quality-gates-setup.sh
./infrastructure/observability/sonarqube/quality-gates-setup.sh
```

##  Métricas Analisadas

### Code Quality
- **Complexidade ciclomática**
- **Linhas duplicadas**
- **Code smells**
- **Cobertura de testes**
- **Dívida técnica**

### Security Analysis
- **Vulnerabilidades** (OWASP Top 10)
- **Security hotspots**
- **Injection flaws**
- **Authentication issues**

### Maintainability
- **Complexidade de código**
- **Duplicação**
- **Cobertura de testes**
- **Documentação**

##  Quality Gates Configurados

| **Métrica** | **Condição** | **Valor** |
|-------------|--------------|-----------|
| **Reliability Rating** | No new E-rated issues | E |
| **Security Rating** | No new E-rated issues | E |
| **Maintainability Rating** | No new D-rated issues | D |
| **Coverage** | Minimum on new code | 80% |
| **Duplicated Lines** | Maximum on new code | 3% |
| **Blocker Violations** | Zero new issues | 0 |
| **Critical Violations** | Zero new issues | 0 |

## 🔄 Integração com CI/CD

### GitHub Actions Integration
O pipeline inclui automaticamente:
-  **SonarQube scanning** em todos os PRs
-  **Quality gate checks** antes do merge
-  **Coverage reporting** para Codecov
-  **Security analysis** com CodeQL

### Workflow Triggers
```yaml
# .github/workflows/ci-cd.yml
sonarqube-analysis:
  needs: build-and-test
  steps:
    - name: SonarQube Scan
      uses: sonarsource/sonarqube-scan-action@master
    - name: Quality Gate Check
      uses: sonarsource/sonarqube-quality-gate-action@master
```

##  Dashboard e Relatórios

### SonarQube Dashboard
- **URL**: http://localhost:9000/projects
- **Projeto**: `ubnt-secpilot`
- **Branches**: main, develop, feature branches

### Relatórios Disponíveis
- **Coverage Report**: Cobertura detalhada por arquivo/classe
- **Security Report**: Vulnerabilidades encontradas
- **Code Smells**: Problemas de qualidade identificados
- **Duplication Report**: Código duplicado detectado

##  Desenvolvimento Local

### SonarQube Scanner CLI
```bash
# Instalar scanner
dotnet tool install --global dotnet-sonarscanner

# Executar análise local
dotnet sonarscanner begin /k:"ubnt-secpilot" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="your-token"
dotnet build UbntSecPilot.sln
dotnet test UbntSecPilot.sln --collect:"XPlat Code Coverage"
dotnet sonarscanner end /d:sonar.login="your-token"
```

### Configuração de Projeto
Arquivo `sonar-project.properties` na raiz do projeto configura:
- 📁 **Fontes**: Diretórios de código fonte
- 🧪 **Testes**: Diretórios de testes
-  **Cobertura**: Exclusões e configurações
-  **Segurança**: Regras habilitadas

## 🔐 Segurança e Tokens

### Criar Authentication Token
1. **SonarQube Web**: Administration → Security → Users
2. **Meu Perfil**: Account → Security → Generate Token
3. **Nome**: `github-actions-ubnt-secpilot`
4. **Tipo**: Project analysis token

### Secrets no GitHub
```yaml
# .github/secrets
SONARQUBE_TOKEN: ghp_your-token-here
SONARQUBE_HOST_URL: http://localhost:9000
```

##  Troubleshooting

### Problemas Comuns

**1. SonarQube não inicia:**
```bash
# Verificar logs
docker-compose logs sonarqube

# Reset se necessário
docker-compose down -v
docker-compose up -d sonarqube sonarqube-db
```

**2. Quality Gate falhando:**
```bash
# Verificar métricas específicas
# Acesse SonarQube web interface
# Vá para o projeto → Quality Gate
```

**3. Coverage não reportada:**
```bash
# Verificar formato do relatório
dotnet test --collect:"XPlat Code Coverage" --results-directory coverage
ls -la coverage/
```

##  Recursos Adicionais

- **📖 SonarQube Docs**: https://docs.sonarqube.org/
- **🎓 .NET Guide**: https://docs.sonarqube.org/latest/analysis/languages/csharp/
- ** Security Rules**: https://rules.sonarsource.com/csharp/
- ** Metrics Guide**: https://docs.sonarqube.org/latest/user-guide/metric-definitions/

---

**Status**:  **SonarQube Totalmente Integrado**
**Quality Gates**:  **Configuradas e Operacionais**
**CI/CD Integration**:  **100% Automatizado**
