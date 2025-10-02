# Agents Overview

Cada agente combina uma coleção de ferramentas para atingir um objetivo específico.
Atualmente existe o **ThreatEnrichmentAgent**, responsável por transformar eventos de
telemetria Kafka em findings priorizados usando Spark (ou fallback heurístico) e persistindo no MongoDB.

## Componentes principais
- `EventCollector`: usa o `KafkaEventRepository` (ou outro `EventRepository`) para puxar lotes respeitando offsets.
- `EventEnricher`: delega ao `SparkThreatIntel` para enriquecer em lote (fallback: `HeuristicThreatIntel`).
- `OllamaThreatIntel`: quando `UBNT_INTEL_ENGINE=ollama`, usa um modelo local exposto via Ollama para atribuir severidade, resumo e tags a cada evento.
- `FindingPersistor`: salva eventos, findings e decisões através do `MongoStorageRepository`, que indexa coleções e expõe dados para a API/UI.
- Ingestão API: o `UdmApiKafkaCollector` transforma chamadas autenticadas da Integration API (`X-API-KEY`) em eventos no tópico `udm.events`, respeitando `udm.poll_interval_seconds`.
- `scripts/run_stack.sh` carrega variáveis do `.env`, expõe a flag `--skip-udm-api` e executa o coletor automaticamente quando `UBNT_UDM_BASE_URL` estiver definido.
- Observabilidade: métricas Prometheus (`agent_events_collected_total`, `agent_findings_produced_total`, etc.) são servidas conforme `observability.metrics_port`.
- Prometheus/Grafana: `infrastructure/observability/prometheus.yml` e `infrastructure/observability/grafana/` já provisionam datasource e dashboards (`udm_api_requests_total`, `udm_api_fetch_duration_seconds`, `udm_api_events_emitted_total`) para o coletor na porta definida por `udm.metrics_port`. O dashboard principal fica em `http://localhost:3000/d/udm-collector-overview/udm-collector-overview` (credenciais padrão `admin`/`admin`).

## Ciclo
1. Coleta eventos disponíveis.
2. Envia cada evento para enriquecimento.
3. Persiste entradas e resultados.
4. Retorna uma decisão resumindo o trabalho executado.

Substitua qualquer ferramenta em `infrastructure/tools/` para integrar fontes reais (UDM API,
Spark jobs personalizados, storage persistente alternativo) sem alterar as camadas superiores. Ao trocar adaptadores, lembre-se de registrar novos recursos nas métricas/closers apropriados e, se necessário, instrumentar contadores/latências compatíveis com os padrões Prometheus expostos (`udm_api_requests_total`, `udm_api_fetch_duration_seconds`, etc.).
