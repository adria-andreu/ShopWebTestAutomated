# PROJECT.md — Canvas extendido del proyecto

<p align="right">
  <a href="./README.md">README (front-door)</a> ·
  <a href="./docs/runbook.md">Runbook</a>
</p>

> 📘 **Versión condensada**: usa el [README.md](./README.md) para una vista rápida de uso, comandos y CI.
> Este **Canvas** contiene la guía completa, políticas y ejemplos enlazados.

## 🧭 Índice rápido

* [1) AI Purpose & Enforcement](#1-ai-purpose--enforcement)
* [2) 🎯 Objeto del Proyecto](#2--objeto-del-proyecto-definición-operativa)
* [3) 🔍 Project Overview](#3--project-overview)
* [4) Alcance y Limitaciones](#4-alcance-y-limitaciones)
* [5) Arquitectura del repo + Quién llama a quién](#5-arquitectura-del-repo--quién-llama-a-quién)
* [6) Líneas maestras esenciales](#6-líneas-maestras-esenciales)
* [7) Plan de implementación (A/B/C)](#7-plan-de-implementación-abc)

  * [A) Métricas & reporting](#a-métricas--reporting)
  * [B) Portabilidad multi-sitio](#b-portabilidad-multi-sitio)
  * [C) MVP técnico & backlog](#c-mvp-técnico--backlog)
* [8) CI/CD (GitHub Actions + Docker)](#8-cicd-github-actions--docker)
* [9) SLI/SLO (KPIs)](#9-slislo-kpis)
* [10) Políticas (Flaky, Retries, Seguridad)](#10-políticas-flaky-retries-seguridad)
* [11) Quality Gates (enforce)](#11-quality-gates-enforce)
* [12) Artefactos & Ejemplos](#12-artefactos--ejemplos)
* [13) Definition of Done](#13-definition-of-done)
* [14) Roadmap (Hitos)](#14-roadmap-hitos)
* [15) Unit Tests (NUnit, .NET 8)](#12-unit-tests-nunit-net-8)

---

## 1) AI Purpose & Enforcement

* **Precedencia de reglas:** `CLAUDE` primario ▶ `PROJECT.md` ▶ docs ▶ código.
* **Rechazo inmediato** si: selectores en tests, `Thread.Sleep`, asserts en POM, romper capas, saltar quality gates, secretos en repo/logs.
* **PR Template obligatorio** con declaración de cumplimiento y evidencias (Allure, `artifacts/run-metrics.json`, traces).
* **CODEOWNERS** exige revisión humana en carpetas clave.
* **Conflictos:** prevalece la regla de nivel superior; en ambigüedad, principio de **mínimo riesgo** + issue.

**Artefactos**: `.github/PULL_REQUEST_TEMPLATE.md`, `.github/CODEOWNERS`, chequeos "PolicyCheck" en CI.

---

## 2) 🎯 Objeto del Proyecto (definición operativa)

**Propósito**: Framework de automatización UI con **C#/.NET 8 + Playwright + NUnit (POM)**, **fiable, mantenible y portable** para una web pública de shopping; ejecutable en **local** y **CI (GitHub Actions/Docker)**; operado por un **agente de IA** supervisado.

**Outcomes**:

* Infra base (POM delgado, Flows, BrowserFactory thread-safe, paralelización).
* Observabilidad (Allure + métricas JSON, traces/screenshots en fallo).
* CI reproducible (matriz navegadores+SiteId, quality gates).
* Portabilidad por config (perfiles A/B) **sin tocar tests**.

**KPIs iniciales**: pass rate PR ≥ 90% (main/nightly ≥ 95%), p95 ≤ 12/10 min, flaky ratio ≤ 5%, suite E2E corre en A y B cambiando solo `SiteId`.

---

## 3) 🔍 Project Overview

* **Stack**: C# .NET 8 · Playwright · NUnit · POM.
* **Entorno**: local + GitHub Actions; Docker disponible.
* **Foco**: paralelización, matriz de navegadores, trazas, portabilidad.

---

## 4) Alcance y Limitaciones

* **Alcance**: Happy path, errores, edge, negativos, test data, seguridad base, **perf ligero** (p50/p95).
* **Limitaciones**: sin entornos dedicados; servicios externos limitados; seguridad/perf avanzados después.

**Alcance de pruebas** (aclaración explícita)
Este repositorio cubre **pruebas E2E de UI** con Playwright + NUnit y **Unit Tests** para componentes del framework.
No se incluyen **pruebas de API** en este proyecto.

- Terminología: "E2E tests" = UI automation con Playwright; "Unit tests" = componentes del framework sin dependencias externas.
- Métricas de E2E: passRate, p95, flakyRatio. Métricas de Unit: coverage, execution time.
- Ambos tipos de pruebas son requeridos para el Definition of Done del proyecto.


---

## 5) Arquitectura del repo + Quién llama a quién

```
ShopWebTestAutomated/
  Browsers/           # BrowserFactory, context options
  Pages/              # POMs (interfaces + impl por sitio)
  Flows/              # Casos de uso (dominio)
  Tests/              # NUnit (orquesta flows)
  Utilities/          # Helpers, builders, waits, assertions
  Config/             # appsettings.tests.json, profiles, TestData
  artifacts/          # run-metrics.json, test-metrics.jsonl, traces
  tools/GateCheck/    # Quality gate CLI
  .github/workflows/  # CI (tests.yml)
  docs/examples/      # Ejemplos largos y guías
```

**Dependencias permitidas**: Tests ▶ Flows ▶ Pages (POM) ▶ Playwright.
**Reglas**:

* Tests **sin selectores ni waits**; solo llaman **Flows**.
* POM **sin asserts** ni negocio; waits encapsulados.
* Flows exponen `CleanupAsync` si crean datos.
* Utilities libres de UI (reusables).

---

## 6) Líneas maestras esenciales

* **Convenciones**: código/comentarios en inglés; `PascalCase` público, `_camelCase` privado; sufijo `Async`.
* **Prohibido**: `Thread.Sleep`, selectores en tests, asserts en POM.
* **Datos de prueba**: `TestDataId.New("prefix")`, builders deterministas, JSON por suite, cleanup en flows.
* **Paralelización**: `[Parallelizable]`, sin estado compartido, `-m:<workers>`.

---

## 7) Plan de implementación (A/B/C)

### A) Métricas & reporting

**Solución**: Observabilidad en doble nivel →  
1. **Per-test** (`test-metrics.jsonl` + adjuntos).  
2. **Per-suite** (`run-metrics.json` para Quality Gates).  
3. **Visualización en Allure** con artefactos y labels obligatorios.  
4. **Control automático en CI** mediante `GateCheck`.

---

**Instrucciones:**

* `BaseTest` debe:
  - Iniciar **Playwright Tracing** por test según `TraceMode` (configurable en `appsettings.tests.json` o env vars).
  - Adjuntar **artefactos**:
    - `trace.zip` siempre (según TraceMode).
    - `screenshot.png` en fallo.
    - Opcional: `page.html` y `console.log.txt`.
  - Escribir 1 línea en `artifacts/test-metrics.jsonl` con al menos:
    ```json
    {
      "name": "Cart_When_Adding_Item",
      "status": "Passed|Failed|Skipped",
      "durationMs": 5231,
      "artifactsPath": "artifacts/Cart_When_Adding_Item_20250820_101530123",
      "timestampUtc": "2025-08-20T10:15:35Z",
      "browser": "chromium",
      "siteId": "A",
      "commitSha": "abc1234",
      "retries": 0
    }
    ```
  - Al finalizar la suite, generar `artifacts/run-metrics.json` con resumen:
    ```json
    {
      "total": 120,
      "passed": 115,
      "failed": 3,
      "skipped": 2,
      "passRate": 0.958,
      "passRateEffective": 0.942,
      "flakyRatio": 0.033,
      "p95DurationMs": 10800,
      "startedAtUtc": "2025-08-20T10:15:30Z",
      "finishedAtUtc": "2025-08-20T10:15:55Z"
    }
    ```

* **Allure results**:
  - Attachments mínimos: `trace.zip`, `screenshot.png`.
  - Labels obligatorios: `browser`, `siteId`, `commitSha`, `pipelineId`.
  - Links a issues para tests en quarantine.

* **Política de tracing (`TraceMode`)**:
  - **Local**: On (diagnóstico completo).
  - **PR**: OnFailure (solo en fallos).
  - **Nightly**: On (regresión completa).
  - Configurable vía `Config/appsettings.tests.json` o env vars (`TRACE_MODE`).

* **Flaky ratio**:
  - Un test es **flaky** si cambia de estado dentro de un run (falló → pasó en retry) o en la ventana de 10 runs.
  - Registrado en `run-metrics.json`.
  - Salida de quarantine: 10 runs verdes consecutivos sin retry.

* **Retención histórica**:
  - Artefactos y métricas retenidos mín. 30 días en CI.
  - Snapshots de `run-metrics.json` en `artifacts/history/` para análisis de tendencias.

* **Quality Gates** (enforce):
  - **PR**: `passRate ≥ 0.90`, `p95DurationMs ≤ 12min`, `flakyRatio ≤ 0.05`.
  - **main/nightly**: `passRate ≥ 0.95`, `p95DurationMs ≤ 10min`, `flakyRatio ≤ 0.05`.
  - **Fail inmediato** si `run-metrics.json` falta o es inválido.

---

## 📌 BaseTest — Directrices obligatorias

> **Objetivo:** Homogeneizar la inicialización y limpieza de los tests, centralizar la gestión de navegador/contexto, timeouts y artefactos. Evitar duplicación de código en las suites y garantizar trazabilidad (métricas, screenshots, traces).

### Responsabilidades del `BaseTest`

* **Setup por test**
  - Crear un `IBrowserContext` y `IPage` aislados.
  - Configurar **timeouts globales** (acciones/navegación).
  - Iniciar tracing según `TraceMode`.

* **TearDown por test**
  - Detener y adjuntar `trace.zip`.
  - Capturar screenshot en fallos.
  - Opcional: guardar HTML y console logs.
  - Registrar métricas en `test-metrics.jsonl`.

* **Ciclo de vida del navegador**
  - Usar `BrowserFactory` thread-safe (un `IBrowser` compartido por worker).
  - Prohibido crear Browser/Context/Page fuera del `BaseTest`.

* **Integración con reporting**
  - Adjuntar artefactos a NUnit y Allure.
  - Publicar `run-metrics.json` al final de la suite.

* **Cumplimiento de reglas del repo**
  - Sin `Thread.Sleep`, sin selectores en tests, sin asserts en POM.
  - Todo test debe heredar de `BaseTest`.

---

### Organización de artefactos

* Carpeta raíz: `/artifacts/`.
* Subcarpeta por test: `artifacts/<TestName>_<UTC_yyyyMMdd_HHmmssfff>/`.
* Contenido mínimo:
  - `trace.zip` (siempre).
  - `screenshot.png` (en fallo).
  - `page.html` y `console.log.txt` (opcionales).
* Todos los artefactos deben añadirse como attachments.

---

### Checklist de adopción

* [ ] `BrowserFactory` thread-safe en `/Browsers/`.
* [ ] `BaseTest` único en `/Tests/` y heredado por todas las suites.
* [ ] Configuración vía `Config/appsettings.tests.json` o env vars.
* [ ] Artefactos con timestamp adjuntados automáticamente.
* [ ] Métricas publicadas (`test-metrics.jsonl`, `run-metrics.json`) y accesibles en CI.

---

### B) Portabilidad multi-sitio

**Solución**: `ISiteProfile`, POM por sitio (`SiteA_*/SiteB_*`), **Flows** neutrales; selección por `SiteId` en config/DI.
**Instrucciones**:

* Tests dependen de **interfaces** POM; cambios de sitio no tocan tests.
* Normalizar precio/fecha vía `ISiteProfile`.
  **Criterios**: cambiar `SiteId` sin editar tests; ≤ 10 cambios fuera de `Profiles/`/`Pages/SiteB_*`; diferencia de duración ≤ 20%.

### C) MVP técnico & backlog

**Orden**: Infra ▶ Paralelización ▶ Reporting ▶ POM/Flows ▶ Test data ▶ Linters ▶ Docker/CI ▶ Retries/Flaky.
**Criterios (DoD base)**: `dotnet test -m:4` verde en 3 browsers; Allure+metrics publicados; 1 test por tipo; 0 `Thread.Sleep` y 0 selectores en tests.

---

## 8) CI/CD (GitHub Actions + Docker)

* **Matriz**: `browser ∈ {chromium, firefox, webkit}` × `siteId ∈ {A, B}`.
* **Cachés**: NuGet (y opcional Playwright). **Nightly** programado.
* **GateCheck** decide el estado final; artefactos retenidos ≥ 7 días.
* **Dockerfile** y `docker-compose.yml` para ejecución reproducible.

---

## 9) SLI/SLO (KPIs)

* **Pass rate** PR ≥ 90% (main/nightly ≥ 95%).
* **p95** ≤ 12 min (PR) · ≤ 10 min (main/nightly).
* **Flaky ratio** ≤ 5% (si activado).
* **Portabilidad**: suite E2E corre en A y B cambiando solo `SiteId`.

---

## 10) Políticas (Flaky, Retries, Seguridad)

* **Flaky & Quarantine**: `[Category("Flaky")]` + issue (owner, TTL 7 días); salida tras **10 verdes seguidos sin retry**.
* **Retries**: `[Retry(1)]` solo en `Regression/Edge/Flaky`; prohibido en `Smoke/A11y/Visual`.
* **Seguridad**: secretos por env/GA; `secrets.local.json` no versionado; `dotnet list package --vulnerable` en CI; secret scanning.

---

## 11) Quality Gates (enforce)

* Falla si: `passRate < 0.90` o `p95 > 12min` (PR), y/o **flakyRatio > 5%** (si activo).
* Verificador: `tools/GateCheck/Program.cs` leyendo `artifacts/run-metrics.json`.

---

## 12) Artefactos & Ejemplos

* **Métricas & Allure**:
  `docs/examples/RunMetricsCollectorExample.cs`, `docs/examples/AllureIntegrationExample.cs`
* **Portabilidad & POM**:
  `docs/examples/PortabilityPatternExample.cs`, `docs/examples/pom-guidelines.md`
* **Infra & CI**:
  `docs/examples/BrowserFactoryExample.cs`, `docs/examples/CIDockerWorkflowExample.yml`, `Dockerfile`, `docker-compose.yml`
* **Estilo & Ops**:
  `docs/examples/editorconfig-dotnet8.txt`, `docs/examples/flaky-issue-template.md`, `docs/runbook.md`
* **Tools**:
  `tools/GateCheck/` (csproj + Program.cs)

---

## 13) Definition of Done

* CI verde en **3 navegadores × 2 SiteId** con SLOs cumplidos.
* Cambio de sitio por config **sin editar tests**.
* Allure + `run-metrics.json` + traces publicados.
* 0 `Thread.Sleep` y 0 selectores en tests; POM sin asserts.
* Política Flaky operativa; secrets OK; linters sin warnings en CI.

---

## 14) Roadmap (Hitos)

* **M0 Base** → Infra, paralelización, 1 E2E "Add to cart".
* **M1 CI+Métricas** → GA matriz + Allure + GateCheck.
* **M2 Portabilidad** → Perfiles A/B, suite verde en ambos.
* **M3 Calidad+Ops** → Categorías (Smoke/Regression/…), Nightly, Runbook.

---

## 15) Unit Tests (NUnit, .NET 8)

> **Nota de alcance:** Este repo también contiene Unit Tests de componentes de negocio (sin UI).

### 15.1 Objetivo
Validar unidades de lógica y utilidades **sin dependencias externas** para **fast feedback** y como **gate previo** a E2E (bloquea merges si fallan o si la cobertura cae por debajo del umbral).

### 15.2 Estructura & rutas
Ejemplo;
/src/ShopWeb/ # código de producción
/tests/ShopWeb.UnitTests/ # NUnit (unit)

### 15.3 Framework & paquetes
- **NUnit 4**, **Microsoft.NET.Test.Sdk**, **FluentAssertions**, **Moq**.
- **Cobertura**: `coverlet.msbuild` con reports `cobertura`.

### 15.4 Reglas de diseño (unit)
- **Aislamiento total**: sin UI/Playwright, sin red, sin disco, sin BD.
- Dobles vía **interfaces/DI** (Moq/stubs); tests deterministas y rápidos (<100 ms).
- Estilo **AAA**; un comportamiento por test; nombres `Metodo_Escenario_Resultado`.
- Categoría: `[Category("Unit")]` para filtrar en CI y local.

### 15.5 Ejecución local
```bash
# Todo el proyecto de unit
dotnet test tests/ShopWeb.UnitTests/ -c Release

# Filtrado por categoría
dotnet test tests/ShopWeb.UnitTests/ -c Release --filter "TestCategory=Unit"


¡Voy! He revisado la **sección 15 – Unit Tests (NUnit, .NET 8)** de tu `PROJECT.md` y te dejo un diagnóstico + una versión mejorada lista para sustituir.

---

## Diagnóstico rápido

**Fortalezas**

* Declara objetivo (unidad, fast feedback) y reglas base (sin I/O, mocks, AAA).&#x20;

**Gaps / incoherencias**

1. **Contradicción con la sección 4 (Alcance):** allí afirmas “repo exclusivamente E2E; no unit ni API”, pero en la 15 introduces Unit Tests. Hay que unificar criterio.&#x20;
2. **Rutas y naming inconsistentes:** en 15 indicas `tests/UnitTests/`, mientras en el resto del doc/ejemplos se usa `tests/ShopWeb.UnitTests/`.&#x20;
3. **Paquetes/herramientas:** en 15 se menciona `coverlet.collector`, pero en otros ejemplos propones `coverlet.msbuild` y gates por cobertura en CI. Alinear.&#x20;
4. **CI/gates ausentes:** no define umbral (p. ej. 80%), ni dependencia **Unit → E2E** en Actions, ni cómo bloquear merges.&#x20;
5. **Texto a pulir:** mezcla ES/EN y alguna frase incompleta (“Run pre-commit…”, “Some scenarios such be…”).&#x20;

---

## Propuesta de sustitución para la sección 15

Pega este bloque **en lugar** de la sección 15 actual:

```markdown
## 15) Unit Tests (NUnit, .NET 8)

> **Nota de alcance:** Si la sección 4 declara “solo E2E”, actualízala para reflejar que **este repo también contiene Unit Tests** de componentes de negocio (sin UI). Alternativamente, mueve esta sección a un repo separado. Elige **una** política y mantenla consistente en README/Runbook/CI.

### 15.1 Objetivo
Validar unidades de lógica y utilidades **sin dependencias externas** para **fast feedback** y como **gate previo** a E2E (bloquea merges si fallan o si la cobertura cae por debajo del umbral).

### 15.2 Estructura & rutas
```

/src/ShopWeb/                       # código de producción
/tests/ShopWeb.UnitTests/           # NUnit (unit)

````

### 15.3 Framework & paquetes
- **NUnit 4**, **Microsoft.NET.Test.Sdk**, **FluentAssertions**, **Moq**.
- **Cobertura**: `coverlet.msbuild` con reports `cobertura`.

### 15.4 Reglas de diseño (unit)
- **Aislamiento total**: sin UI/Playwright, sin red, sin disco, sin BD.
- Dobles vía **interfaces/DI** (Moq/stubs); tests deterministas y rápidos (<100 ms).
- Estilo **AAA**; un comportamiento por test; nombres `Metodo_Escenario_Resultado`.
- Categoría: `[Category("Unit")]` para filtrar en CI y local.

### 15.5 Ejecución local
```bash
# Todo el proyecto de unit
dotnet test tests/ShopWeb.UnitTests/ -c Release

# Filtrado por categoría
dotnet test tests/ShopWeb.UnitTests/ -c Release --filter "TestCategory=Unit"
````

### 15.6 Cobertura & umbral

* Umbral mínimo inicial: **80% líneas (total)**.
* Sube el listón progresivamente (85%/90%) cuando el código crezca/estabilice.

### 15.7 CI (GitHub Actions)

* **Workflow**: job `unit-tests` que:

  * Restaura/compila y ejecuta **solo** `tests/ShopWeb.UnitTests/`.
  * Genera cobertura `cobertura` y falla si `< 80%` (MSBuild props).
  * Publica artifacts (XML + HTML summary).
* **Orquestación**: job E2E hace `needs: unit-tests` → **E2E no corre si fallan los unit**.
* **Branch protection**: requerir los checks `unit-tests` + `e2e-tests` antes de merge.

**Ejemplo de paso de test con cobertura (MSBuild):**

```bash
dotnet test tests/ShopWeb.UnitTests/ \
  -c Release \
  /p:CollectCoverage=true \
  /p:CoverletOutput=./TestResults/Coverage/ \
  /p:CoverletOutputFormat=cobertura \
  /p:Threshold=80 /p:ThresholdType=line /p:ThresholdStat=total
```

### 15.8 Anti-patrones (no permitido)

* Tests unit que toquen UI/Playwright o dependencias externas.
* Lógica de negocio dentro de POM o Flows “testeada” como unit.
* Usar retries en unit: si flakea, **arreglarlo o eliminarlo**.

### 15.9 Definition of Done (unit)

* ✅ Suite verde en CI (`unit-tests`) con cobertura ≥ **80%** (subirá en roadmap).
* ✅ Categorías aplicadas (`Unit`) y tests deterministas (<100 ms).
* ✅ Acoplamiento mínimo: mocks/DI cubren dependencias.
* ✅ Pipeline E2E depende de Unit (`needs`), y ambos checks son **obligatorios** para merge.

```

> Nota: consulta también `tests/README.md.txt` incluido en el repo para detalles de cobertura y ejecución.
