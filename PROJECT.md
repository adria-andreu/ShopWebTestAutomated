# PROJECT.md — ShopWebTestAutomated Specific Configuration

> **Hierarchical Subordination**: This PROJECT.md **obedece y se subordina jerárquicamente** al [`CLAUDE.md`](CLAUDE.md) maestro como norma superior. Su función es **especializar, detallar y extender** las reglas globales sin contradecirlas, nunca reducir ni anular lo definido en CLAUDE.md.

## Imports y Precedencia de Documentación

```md
# Documentos de referencia (orden estricto según CLAUDE.md)
1. @CLAUDE.md                                     <!-- Manual maestro GLOBAL -->
2. @PROJECT.md                                    <!-- Este documento (especialización) -->
3. @project_memory/memoria_proyecto_shopweb.md    <!-- Iteración vigente -->
4. @historical_project_memory/memoria_proyecto_shopweb_hist.md <!-- Histórico -->
5. @technical_debt/technical_debt_shopweb.md      <!-- Deuda técnica viva -->
6. @roadmap/roadmap_shopweb.md                    <!-- Roadmap vivo + histórico -->
7. @downloads/BIBLIOGRAPHY.md                     <!-- Bibliografía técnica -->
```

## Especialización para ShopWebTestAutomated

### Contexto del Proyecto
- **Tipo**: Framework de automatización E2E con .NET 8 + Playwright + NUnit
- **Target**: Aplicaciones web de e-commerce (demoblaze.com como Site A)
- **Arquitectura**: Tests → Flows → Pages (POM) → Playwright
- **Operación**: Agente IA supervisado con quality gates automáticos

### Reglas Específicas del Proyecto

#### Stack Técnico Obligatorio
- **.NET 8** como runtime base
- **Playwright** para automatización de browsers
- **NUnit** como framework de testing con paralelización
- **Allure** para reporting avanzado
- **Docker** para ejecución reproducible

#### Reglas de Arquitectura (Extensión de CLAUDE.md)
- **Prohibido absolutamente**: selectores en tests, `Thread.Sleep`, asserts en POM
- **Dependencias permitidas**: Tests ▶ Flows ▶ Pages ▶ Playwright (unidireccional)
- **Paralelización**: `[Parallelizable]` obligatorio, sin estado compartido
- **Métricas**: Todos los tests DEBEN generar métricas JSON para quality gates

#### Quality Gates Específicos
```json
{
  "PR": { "MinPassRate": 0.90, "MaxP95DurationMs": 720000, "MaxFlakyRatio": 0.05 },
  "Main": { "MinPassRate": 0.95, "MaxP95DurationMs": 600000, "MaxFlakyRatio": 0.05 }
}
```

### Estructura de Carpetas del Proyecto

```
ShopWebTestAutomated/
├── CLAUDE.md                          # Manual maestro global
├── PROJECT.md                         # Este archivo (especialización)
├── project_memory/                    # Memoria vigente (según CLAUDE.md)
├── historical_project_memory/         # Histórico de iteraciones
├── technical_debt/                    # Deuda técnica viva
├── roadmap/                          # Planificación y backlog
├── downloads/                        # Manuales técnicos y bibliografía
├── templates/                        # Plantillas reutilizables
├── ShopWeb.E2E.Tests/               # Código fuente del framework
│   ├── Browsers/                     # BrowserFactory thread-safe
│   ├── Pages/                        # POM interfaces + implementaciones
│   ├── Flows/                        # Casos de uso de dominio
│   ├── Tests/                        # NUnit test fixtures
│   ├── Utilities/                    # Helpers, métricas, waits
│   └── Config/                       # Configuración y settings
├── tools/GateCheck/                  # CLI para quality gates
└── .github/workflows/                # CI/CD con GitHub Actions
```

### Épicas y Hitos del Proyecto

#### M0: Base Framework ✅
- Arquitectura base (BrowserFactory, BaseTest, POM)
- Configuración y métricas básicas
- Tests de ejemplo funcionando

#### M1: CI/CD + Quality Gates ✅
- GitHub Actions con matriz de browsers
- GateCheck tool operativo
- Allure reporting integrado

#### M2: Portabilidad Multi-sitio
- Site B implementation
- Profile abstraction
- Config-driven site switching

#### M3: Observabilidad Avanzada
- Flaky detection avanzado
- Historical metrics analysis
- Performance trending

### Conflictos y Resolución

- **Precedencia**: CLAUDE.md ▶ PROJECT.md ▶ docs ▶ código
- **Desviaciones detectadas**: Se registrarán en technical_debt/ con ID TD-XXX
- **Principio de mínimo riesgo**: Ante ambigüedad, elegir opción más conservadora

### Definition of Done Específico

Además del DoD de CLAUDE.md, este proyecto requiere:

- [ ] **Tests E2E verdes** en 3 browsers (Chromium, Firefox, WebKit)
- [ ] **Quality gates pasando** (pass rate, p95, flaky ratio)
- [ ] **Artifacts generados**: run-metrics.json, test-metrics.jsonl, traces
- [ ] **Allure report** publicado con attachments
- [ ] **Zero violations**: no selectores en tests, no asserts en POM, no Thread.Sleep

### Datos de Prueba y Secretos

- **TestData**: Builders deterministas con `TestDataId.New("prefix")`
- **Secrets**: Solo por env vars o GitHub Secrets, NUNCA en código
- **Cleanup**: Flows implementan `CleanupAsync()` obligatorio

---

**Nota**: Cualquier contradicción entre este PROJECT.md y CLAUDE.md debe resolverse a favor de CLAUDE.md y documentarse como TD-XXX en technical_debt/.