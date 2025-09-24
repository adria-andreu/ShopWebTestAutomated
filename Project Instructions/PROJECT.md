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

**Artefactos**: `.github/PULL_REQUEST_TEMPLATE.md`, `.github/CODEOWNERS`, chequeos "GateCheck" en CI.

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
Este repositorio cubre **exclusivamente pruebas E2E de UI** con Playwright + NUnit.
No se incluyen **pruebas unitarias** ni **pruebas de API** en este proyecto.

- Terminología: cuando aquí se hable de “tests”, se entiende **E2E UI**.
- Métricas y gates aplican solo a E2E (passRate, p95, flakyRatio).
- Cualquier KPI de “coverage” de código no aplica en este repo.


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

**Objetivo**: validar unidades de lógica y utilidades **sin dependencias externas**, para *fast feedback* y como **gate previo** a las E2E.

### Alcance y reglas
- **Ubicación**: `tests/ShopWeb.UnitTests/` (`ShopWeb.UnitTests.csproj`).
- **Aislamiento**: sin red, disco, UI, BD. Dependencias aisladas con interfaces/DI; si procede, *stubs/mocks*.
- **Framework**: NUnit 4.x · `Microsoft.NET.Test.Sdk` · `coverlet.collector`.
- **Estilo**: AAA, un comportamiento por test, nombres `Metodo_Escenario_Resultado`.
- **Velocidad**: tests deterministas y rápidos (<100ms).

> Nota: consulta también `tests/README.md.txt` incluido en el repo para detalles de cobertura y ejecución.
