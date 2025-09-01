---
proyecto: ShopWebTestAutomated
ultima_actualizacion: 2025-01-28
---

# Deuda técnica (viva)

| ID    | Descripción técnica                                           | Impacto | Prioridad | Complejidad | Detectada en | Propuesta / Siguiente paso                                    | Enlace Roadmap |
|-------|---------------------------------------------------------------|---------|-----------|-------------|--------------|--------------------------------------------------------------|----------------|
| TD-01 | Site B implementation missing - only demoblaze (Site A) impl | Alta    | Media     | Media       | it01         | Crear SiteB_* implementations + profile abstraction          | T-020          |
| TD-02 | Flaky detection básico - falta análisis histórico avanzado  | Media   | Baja      | Alta        | it01         | Implementar sliding window detection + quarantine automático | T-025          |
| TD-03 | Console logs capture no implementado en BaseTest             | Baja    | Baja      | Baja        | it01         | Añadir Page.Console event handler en BaseTest setup          | T-021          |
| TD-04 | Docker Playwright setup hardcoded - falta parametrización   | Media   | Media     | Baja        | it01         | Parametrizar versiones y opciones de instalación             | T-022          |
| TD-05 | Secret management básico - falta Azure KeyVault/AWS Secrets | Baja    | Baja      | Media       | it01         | Integrar con cloud secret managers para CI/CD                | T-030          |
| TD-06 | Error handling en Flows demasiado genérico                   | Media   | Media     | Baja        | it01         | Crear custom exceptions con retry policies específicos       | T-023          |
| TD-07 | Allure categories/labels hardcoded - falta configuración     | Baja    | Baja      | Baja        | it01         | Mover labels a appsettings con override per ambiente         | T-024          |
| TD-08 | Bibliography download process missing - solo estructura setup | Baja    | Baja      | Media       | it01         | Implementar descarga automática de docs oficiales            | T-028          |

## Nueva deuda técnica (it03)

| ID    | Descripción técnica                                           | Impacto | Prioridad | Complejidad | Detectada en | Propuesta / Siguiente paso                                    | Enlace Roadmap |
|-------|---------------------------------------------------------------|---------|-----------|-------------|--------------|--------------------------------------------------------------|----------------|
| TD-14 | Allure runtime context "No test context is active" error    | CRÍTICA | ALTA      | Media       | PR#2-CI/CD   | URGENTE: CI/CD failing all browsers due to Allure context  | T-027          |
| TD-18 | CI/CD pipeline failing all browser tests in GitHub Actions  | CRÍTICA | ALTA      | Alta        | PR#2-CI/CD   | All browser jobs exit code 1 - runtime test execution fails | T-027          |
| TD-15 | Flaky detection básico - falta sliding window histórico     | Media   | Alta      | Alta        | it03-start   | Implementar sliding window con storage histórico             | T-027          |
| TD-16 | Performance trending no implementado - falta dashboard       | Media   | Media     | Media       | it03-start   | Dashboard P95, regression detection, alerting                | T-028          |
| TD-17 | Auto-quarantine workflow missing - tests manuales           | Alta    | Alta      | Media       | it03-start   | Workflow automático quarantine/recovery para flaky tests    | T-027          |

## Deuda resuelta (histórico)

- TD-01 → ✅ Resuelta en it02 (Site B implementation completa)
- TD-03 → ✅ Resuelta en it02 (Console logs capture implementado)  
- TD-04 → ✅ Resuelta en it02 (Docker parametrization completa)
- TD-06 → ✅ Resuelta en it02 (Custom exceptions + retry policies)
- TD-09 → ✅ Resuelta en it02 (IBrowserFactory.Dispose implemented)
- TD-10 → ✅ Resuelta en it02 (PageFactory created with multi-site support)
- TD-11 → ✅ Resuelta en it02 (TestSettings.GetCurrentSite method added)
- TD-12 → ✅ Resuelta en it02 (Microsoft.Extensions.Configuration.Binder reference added)
- TD-13 → ✅ Resuelta en it02 (All async/await warnings fixed)

---

## Detalles de deuda técnica

### TD-01: Site B Implementation Missing
**Contexto**: Actualmente solo Site A (demoblaze.com) está implementado. El framework promete portabilidad multi-sitio pero falta Site B.

**Impacto técnico**: 
- Tests no pueden ejecutarse contra sitios alternativos
- Patrón de portabilidad no validado en producción
- Incumple requisito de PROJECT.md sobre config-driven site switching

**Plan de resolución**:
1. Definir Site B target (ej: another e-commerce demo site)
2. Crear ISiteProfile abstraction
3. Implementar SiteB_HomePage, SiteB_ProductListPage, etc.
4. Validar que tests pasan en ambos sitios solo cambiando SiteId config

**Esfuerzo estimado**: 5-8 horas

---

### TD-02: Flaky Detection Básico
**Contexto**: Solo detectamos flaky ratio básico (tests que fallan luego pasan en retry). Falta análisis histórico y quarantine automático.

**Impacto técnico**:
- Tests flaky no se detectan hasta acumular suficientes runs
- No hay quarantine automático de tests inestables
- Métricas de estabilidad incompletas

**Plan de resolución**:
1. Implementar sliding window de últimos N runs
2. Auto-quarantine tras X fallos en Y runs  
3. Auto-recovery tras Z éxitos consecutivos
4. Historical trend analysis

**Esfuerzo estimado**: 8-12 horas

---

### TD-03: Console Logs Capture
**Contexto**: BaseTest tiene placeholder para console logs pero no captura real.

**Impacto técnico**: Debugging más difícil cuando tests fallan sin logs de JS

**Plan de resolución**:
```csharp
_page.Console += async (_, e) => {
    _consoleLogs.Add($"{e.Type}: {e.Text}");
};
```

**Esfuerzo estimado**: 1-2 horas

---

### TD-04: Docker Setup Hardcoded
**Contexto**: Dockerfile y docker-compose tienen versiones y configuraciones hardcoded.

**Impacto técnico**: Dificultad para actualizar versiones, personalizar configuraciones

**Plan de resolución**:
1. Parametrizar versiones via ENV vars o ARGs
2. Template docker-compose con .env support
3. Documentar customization options

**Esfuerzo estimado**: 2-3 horas

---

### TD-05: Secret Management Básico
**Contexto**: Solo env vars y GitHub Secrets. No integración con cloud secret managers.

**Impacto técnico**: Limitado para deployments enterprise o multi-cloud

**Plan de resolución**: No prioritario para MVP, evaluar en it03+

---

### TD-06: Error Handling Genérico
**Contexto**: Flows usan Exception genérica. Falta typed exceptions y retry policies.

**Impacto técnico**: Debugging más difícil, retry logic subóptima

**Plan de resolución**:
1. Crear ShopWebException, NavigationException, etc.
2. Retry policies por tipo de error
3. Exponential backoff configurable

**Esfuerzo estimado**: 3-4 horas

---

### TD-07: Allure Configuration Hardcoded
**Contexto**: Labels y categories hardcoded en BaseTest.

**Impacto técnico**: Difícil personalizar reporting por ambiente

**Plan de resolución**: Mover a appsettings.tests.json con ambiente-specific overrides

**Esfuerzo estimado**: 1-2 horas

---

> **Guía**: Debt = **detalle técnico y plan**; Memoria = **narrativa y contexto**; Roadmap = **tarea accionable**. (Evita duplicar el texto; **enlaza** por ID.)