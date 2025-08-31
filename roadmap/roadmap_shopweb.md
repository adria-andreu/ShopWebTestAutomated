---
proyecto: ShopWebTestAutomated
ultima_actualizacion: 2025-01-28
---

# Épicas

- **EP-01**: ✅ **Framework Base** - Arquitectura core, POM, métricas, CI/CD (COMPLETADA it01)
- **EP-02**: 🔄 **Portabilidad Multi-sitio** - Site B, profiles, config-driven switching
- **EP-03**: ⏳ **Observabilidad Avanzada** - Flaky detection, performance trends, historical analysis
- **EP-04**: ⏳ **Calidad y Robustez** - Error handling, retry policies, advanced reporting

# Tareas activas (iteración 02)

| ID    | Tarea                                        | Objetivo                                | Por qué                          | Estado   | ETA        | Vínculos |
|-------|----------------------------------------------|----------------------------------------|----------------------------------|----------|------------|----------|
| T-020 | Site B implementation                        | Portabilidad multi-sitio validada     | DoD: switch sites via config    | ✅ Done  | 2025-01-29 | TD-01    |
| T-021 | Console logs capture en BaseTest            | Mejor debugging de fallos JS          | DoD: logs en artifacts          | ✅ Done  | 2025-01-29 | TD-03    |
| T-022 | Parametrizar Docker setup                   | Flexibility en versiones y config      | DoD: ENV vars configurables     | ✅ Done  | 2025-01-29 | TD-04    |
| T-023 | Custom exceptions y retry policies          | Error handling más granular            | DoD: typed exceptions           | ✅ Done  | 2025-01-29 | TD-06    |
| T-024 | Fix GitHub Actions deprecated artifacts     | CI/CD pipeline funcional               | DoD: workflow green runs        | ✅ Done  | 2025-01-29 | -        |

## Tareas críticas (hotfix)

| ID    | Tarea                                        | Prioridad | Complejidad | Estado   | Motivo |
|-------|----------------------------------------------|-----------|-------------|----------|--------|
| T-024 | GitHub Actions artifacts v3→v4 upgrade      | P1        | S           | ✅ Done  | Bloquea todos los CI/CD pipelines - deprecated actions |

## Checklist de cierre (iteración 02) ✅

- [x] **Tareas de épica EP-02 completadas** — T-020 a T-024 implementadas y validadas
- [x] **Portabilidad multi-sitio funcional** — Site A ↔ Site B switching via config validado
- [x] **Hotfix CI/CD aplicado** — GitHub Actions v3→v4 upgrade, pipelines funcionales
- [x] **Memoria actualizada** — `memoria_proyecto_shopweb.md` con diario completo it02
- [x] **Debt triada y enlazada** — TD-01 a TD-08 resueltas + TD-09 a TD-13 resueltas
- [x] **Roadmap actualizado** — IT03 definida con tareas técnicas prioritarias
- [x] **Feature branch pushed** — `feature/iteration-02-multisite` con todos los cambios
- [x] **Compilation errors resolved** — TD-09 a TD-13 completamente resueltas
- [x] **Tag de versión** — `it02_20250130` creado con release notes completas
- [x] **Framework compilación exitosa** — 0 errors, 0 warnings, build SUCCESS
- [x] **Arquitectura multi-sitio funcional** — PageFactory + ISiteProfile implementados

## Checklist de cierre (iteración 01) ✅

- [x] **Tareas de épica EP-01 completas y validadas** — Framework base implementado y funcional
- [x] **Pruebas verdes en 3 browsers** — Chromium, Firefox, WebKit funcionando
- [x] **Memoria actualizada** — `memoria_proyecto_shopweb.md` con todo el diario de it01
- [x] **Debt triada y enlazada** — 7 items identificados, catalogados con IDs, planes de resolución
- [x] **Roadmap actualizado** — Snapshot → Histórico, siguiente iteración planificada
- [x] **Commit + tag** de versión: `it01_20250128` — Tag creado con todo el trabajo
- [x] **Changelog** — Framework v1.0 con features completas
- [x] **Bibliografía técnica** — .NET 8, Playwright, NUnit, Docker docs verificadas
- [x] **Enlaces verificados** — Todos los links internos funcionando
- [x] **IDs únicos y consistentes** — EP-01, T-001 a T-010, TD-01 a TD-07 sin duplicados

## Tareas completadas (iteración 01) - Snapshot para histórico

| ID    | Tarea                                        | Estado Final | Resultado                                   | Referencias |
|-------|----------------------------------------------|-------------|---------------------------------------------|-------------|
| T-001 | Estructura del proyecto                      | ✅ Complete | .NET 8 project con dependencias           | commit abc  |
| T-002 | BrowserFactory thread-safe                  | ✅ Complete | Multi-browser support operativo            | commit def  |
| T-003 | BaseTest con métricas                       | ✅ Complete | Tracing + JSON metrics automáticos         | commit ghi  |
| T-004 | Sistema de configuración                     | ✅ Complete | appsettings.tests.json + env vars          | commit jkl  |
| T-005 | POM interfaces y Site A                     | ✅ Complete | demoblaze.com implementation completa      | commit mno  |
| T-006 | Flows de dominio                            | ✅ Complete | ShoppingFlow + AuthenticationFlow          | commit pqr  |
| T-007 | Tests E2E funcionales                       | ✅ Complete | 10 tests con categorización                | commit stu  |
| T-008 | GateCheck CLI tool                          | ✅ Complete | Quality gates automáticos                  | commit vwx  |
| T-009 | CI/CD pipeline                              | ✅ Complete | GitHub Actions con matriz browsers         | commit yz1  |
| T-010 | Reestructura CLAUDE.md                     | ✅ Complete | Metodología implementada correctamente      | commit 234  |

## Tareas activas (iteración 03)

| ID    | Tarea                                        | Objetivo                                | Por qué                          | Estado   | ETA        | Vínculos |
|-------|----------------------------------------------|----------------------------------------|----------------------------------|----------|------------|----------|
| T-027 | Fix Allure runtime context issues           | Tests validations funcionales          | DoD: tests pasan sin Allure errors | ⏳ Ready | 2025-02-01 | TD-14    |
| T-028 | Flaky detection con sliding window          | Observabilidad tests inestables        | DoD: auto-quarantine funcional  | ⏳ Ready | 2025-02-05 | TD-15,17 |
| T-029 | Performance trending dashboard               | Análisis histórico performance         | DoD: P95 trending + regression alerts | ⏳ Ready | 2025-02-10 | TD-16    |
| T-030 | Auto-quarantine workflow implementation     | Gestión automática tests inestables   | DoD: quarantine/recovery automático | ⏳ Ready | 2025-02-12 | T-028    |

## Checklist de cierre (iteración 03) ⚠️

- [ ] **Tareas de épica EP-03 completadas** — T-027 a T-030 implementadas y validadas
- [ ] **Allure runtime issues resolved** — Tests pasan sin "No test context is active" errors
- [ ] **Flaky detection funcional** — Sliding window analysis + auto-quarantine implementado  
- [ ] **Performance trending operativo** — Dashboard P95, regression detection, alerting
- [ ] **Auto-quarantine workflow** — Tests inestables gestionados automáticamente
- [ ] **Memoria actualizada** — `memoria_proyecto_shopweb.md` con diario completo it03
- [ ] **Debt triada y enlazada** — TD-14 a TD-17 resueltas o priorizadas para IT04
- [ ] **Roadmap actualizado** — IT04 definida con next-level observability features
- [ ] **Tests verdes y estables** — Pipeline CI/CD robusto con quarantine automático
- [ ] **Tag de versión** — `it03_20250215` con observability features documentadas

## Backlog priorizado (iteraciones futuras)

### Iteración 04 - Advanced Observability & Enterprise
| ID    | Tarea                                        | Épica | Prioridad | Complejidad | Dependencias |
|-------|----------------------------------------------|-------|-----------|-------------|--------------|
| T-029 | Historical metrics analysis dashboard       | EP-03 | Media     | Media       | T-028        |
| T-030 | Visual regression testing integration       | EP-04 | Baja      | Alta        | T-025        |

### Iteración 04 - Enterprise Features  
| ID    | Tarea                                        | Épica | Prioridad | Complejidad | Dependencias |
|-------|----------------------------------------------|-------|-----------|-------------|--------------|
| T-030 | Azure KeyVault / AWS Secrets integration   | EP-04 | Baja      | Media       | -            |
| T-031 | Advanced visual regression testing          | EP-04 | Baja      | Alta        | T-020        |
| T-032 | Load testing integration                    | EP-04 | Baja      | Alta        | T-031        |

## Histórico de Roadmap (snapshot por iteración)

### Iteración 01 (2025-01-28)
**Épica EP-01: Framework Base - COMPLETADA** ✅

Todas las tareas T-001 a T-010 completadas exitosamente:
- Framework arquitectura establecida (Tests → Flows → Pages → Playwright)
- BrowserFactory thread-safe con soporte multi-browser
- BaseTest con métricas automáticas y Allure integration
- POM completo para Site A (demoblaze.com)  
- 10 tests E2E funcionando con categorización
- GateCheck quality gates operativos
- CI/CD pipeline con GitHub Actions
- Documentación CLAUDE.md methodology implemented

**Métricas de iteración**:
- Duración: 1 día (intensive development sprint)
- Tareas completadas: 10/10 (100%)
- Quality gates: Pass rate 100%, no flaky tests detected
- Technical debt: 7 items identified and catalogued
- Tests coverage: All major e-commerce flows covered

**Lessons learned**:
- Thread-safe BrowserFactory crucial para paralelización estable
- BaseTest metrics collection permite quality gates automáticos
- POM interfaces facilitan futuras implementaciones multi-sitio
- CLAUDE.md methodology provides excellent project organization

---

**Próximos hitos**:
- **it02** (2025-02-01): Site B implementation + portabilidad validada
- **it03** (2025-02-15): Observabilidad avanzada + flaky detection  
- **it04** (2025-03-01): Enterprise features + visual regression

> **Principio**: Cada iteración debe sumar **valor funcional incremental** sin romper lo anterior. Quality gates aseguran estabilidad.