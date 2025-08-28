## `docs/runbook.md`

## Scope operativo
Este runbook aplica a **E2E UI**. El repo **no** contiene pruebas unitarias.
Cualquier referencia a “unit” o “coverage” se considera fuera de alcance aquí.


# Runbook — Operación y diagnóstico (Playwright + NUnit)
> 🔙 Volver: [README](../README.md) · [PROJECT.md](../PROJECT.md)

> Propósito: guiar **diagnóstico, reproducción y corrección** de fallos; aplicar **quarantine** y cumplir **quality gates** con evidencias (métricas + artefactos).

---

## 0) Dónde están las cosas
- **Allure results**: `allure-results/` (artefacto CI; HTML si se publica).
- **Métricas**: `artifacts/test-metrics.jsonl` (por test), `artifacts/run-metrics.json` (por suite).
- **Traces/Screenshots**: `artifacts/<TestName>_YYYYMMDD_HHmmssfff/trace.zip|screenshot.png`.
- **Histórico**: `artifacts/history/run-YYYYMMDD-HHMM.json` (retención CI ≥ 30 días).
- **Config**: `Config/appsettings.tests.json` (o env vars); `secrets.local.json` *(no versionado)*.

---

## 1) Diagnóstico rápido (≤ 10 min)
1. Abre el último run en **Allure** y localiza el test fallido.
2. Descarga el **trace** y ábrelo:
   - `npx playwright show-trace artifacts/.../trace.zip` (Node CLI), o
   - `~/.dotnet/tools/playwright show-trace artifacts/.../trace.zip` (si tienes Playwright .NET CLI enlazado).
3. Revisa los **logs** del job (pasos `Test`, `Validate run-metrics.json`, `Quality Gates`).
4. Consulta `artifacts/run-metrics.json`:
   - `passRate`, `p95DurationMs`, `flakyRatio`.
   - (Opcional) `flakyTests[]` si está habilitado.

---

## 2) Reproducir localmente

> Estándar: **variables de entorno** + filtro de VSTest `--filter`.

```bash
# Chromium headless (por defecto)
dotnet test --configuration Release

# Ejecutar en modo headed
HEADED=1 dotnet test --configuration Release

# Cambiar navegador y forzar tracing
BROWSER=firefox TRACE_MODE=On dotnet test --configuration Release

# Filtrar por categoría (NUnit -> TestCategory)
dotnet test --filter "TestCategory=Smoke"

# Filtrar por nombre (contains)
dotnet test --filter "FullyQualifiedName~When_Adding_To_Cart"

# Cambiar de sitio (perfiles A/B)
SITE_ID=B dotnet test --filter "TestCategory=Regression"
````

> Evitar `--where` y `TestRunParameters.Parameter(...)` aquí; mantener un único estándar de ejecución.

---

## 3) Causas típicas y acciones

* **Waits insuficientes** → usa auto-waiting de Playwright + `Locator.WaitForAsync` y helpers (`Utilities/Waits`).
* **Datos no únicos** → `TestDataId.New(prefix)` + cleanup en flows.
* **Selectores frágiles** → preferir roles/`data-testid` y selectores estables.
* **Dependencias de tiempo/animaciones** → esperar por estado observable (visibilidad, network idle) o congelar reloj si es viable.
* **Inestabilidad externa (red/servicios)** → reintentos delimitados en flows (no en POM ni en tests).

---

## 4) Política de Flaky & Quarantine

* Marcar con `[Category("Flaky")]` y crear issue con plantilla `docs/examples/flaky-issue-template.md` (owner + TTL 7 días).
* **Salida**: 10 ejecuciones **verdes** consecutivas **sin retry**.
* Mantener etiquetas/links en Allure para trazabilidad.

---

## 5) Quality Gates (CI)

* **PR**: `passRate ≥ 0.90`, `p95DurationMs ≤ 12 min`, `flakyRatio ≤ 0.05`.
* **main/nightly**: `passRate ≥ 0.95`, `p95DurationMs ≤ 10 min`, `flakyRatio ≤ 0.05`.
* **Hard-fail** si `artifacts/run-metrics.json` falta o es inválido (el pipeline termina en error).

---

## 6) CI (GitHub Actions) — operaciones útiles

* **Re-run failed jobs** desde la UI.
* **Descarga de artefactos**: `artifacts/**` y `allure-results/**`.
* **Nightly**: ejecuta **matriz completa** `browser × siteId`, `TRACE_MODE=On`.
* Ver workflow: `docs/examples/CIDockerWorkflowExample.yml`.

---

## 7) Seguridad de artefactos

* **Nunca** imprimir secretos en logs; usar **masking** de CI.
* `secrets.local.json` fuera de control de versiones.
* Sanitizar `page.html` y `console.log.txt` antes de adjuntar si pueden contener PII/secretos.
* Revisar vulnerabilidades: `dotnet list package --vulnerable`.

---

## 8) Checklist de cierre de incidente

* [ ] Causa raíz identificada/documentada.
* [ ] Corrección aplicada (selector robusto / espera por estado / datos únicos).
* [ ] Artefactos presentes: `trace.zip`, `screenshot.png` (si fallo), métricas actualizadas.
* [ ] Issue de flaky/quarantine actualizado o cerrado (cuando aplique).
* [ ] Run en `main` verde y **Quality Gates** OK.
---

**Navegación:** [README](../README.md) · [PROJECT.md](../PROJECT.md) · [Volver arriba](#runbook--operación-y-diagnóstico-playwright--nunit)
```
