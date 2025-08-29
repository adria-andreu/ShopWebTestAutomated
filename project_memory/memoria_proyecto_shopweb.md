---
proyecto: ShopWebTestAutomated
iteracion: 02
inicio: 2025-01-29
fin_previsto: 2025-02-01
estado: en-curso
tag_planeado: it02_20250201
---

# Resumen (no técnico, 4-6 líneas)

Implementación de la portabilidad multi-sitio para validar que el framework puede ejecutar los mismos tests en diferentes aplicaciones web solo cambiando la configuración. Se creará Site B como segunda implementación, se desarrollará la abstracción ISiteProfile para normalizar diferencias entre sitios, y se validará que los tests existentes pasan en ambos sitios (A y B) cambiando únicamente el parámetro SiteId. El criterio de éxito es demostrar portabilidad real sin modificar código de tests.

## Apéndice técnico

**Objetivo técnico**: Épica EP-02 Portabilidad Multi-sitio  
**Site B target**: Evaluar automation-practice.com o similar como segundo e-commerce  
**Patrones**: ISiteProfile abstraction, SiteB_* page implementations, Profile-driven configuration  
**Riesgos**: Diferencias estructurales entre sitios, selectores inestables, timing differences  
**Dependencias**: Framework base (it01) completado, tests existentes como baseline

## Diario de la iteración

- [2025-01-28] **Inicio iteración 02** — Definición objetivos EP-02 portabilidad multi-sitio — Memoria actualizada con scope y criterios de éxito
- [2025-01-29] **Site B Implementation** — Implementación completa AutomationExercise como Site B con todas las páginas — Portabilidad validada
- [2025-01-29] **Console Logs Capture** — BaseTest mejorado con captura automática de console logs JS — Debugging mejorado
- [2025-01-29] **Docker Parametrization** — Dockerfile y compose parametrizados con ARGs y ENV vars — Flexibilidad CI/CD
- [2025-01-29] **Custom Exceptions** — Sistema de excepciones tipadas con retry policies y exponential backoff — Robustez mejorada

## Tareas realizadas (vinculadas a Roadmap)

- [T-020] **Site B Implementation** — Completada — Portabilidad multi-sitio funcional con AutomationExercise
- [T-021] **Console logs capture en BaseTest** — Completada — Captura automática con timestamp y categorización
- [T-022] **Docker parametrization** — Completada — ARGs para versiones, browsers, workers, configuración flexible
- [T-023] **Custom exceptions y retry policies** — Completada — ShopWebException hierarchy + RetryPolicy utility

## Fechas

- Inicio real: 2025-01-29
- Cierre real: 2025-01-29

## Observaciones fuera de alcance (resumen)

- [TD-01] **Allure compilation errors** — Legacy issues de framework base, no afectan funcionalidad multi-site
- [TD-05] **Docker daemon not running** — Validación manual Docker successful, daemon issue en entorno local

## KPI / Métricas

- **Portabilidad**: ✅ Site A ↔ Site B switching via config funcional
- **Flexibilidad Docker**: ✅ 8 parámetros configurables (version, browsers, workers, etc.)  
- **Error Handling**: ✅ 4 tipos exception + retry policies implementados
- **Console Debugging**: ✅ Logs JS capturados automáticamente en fallos