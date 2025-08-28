## `/docs/examples/flaky-issue-template.md`

```md
---
name: "Flaky Test"
about: "Seguimiento de test inestable (Quarantine Policy)"
title: "[FLAKY] <FullyQualifiedTestName>"
labels: ["flaky", "tests"]
assignees: []
---

## Identificación
- **Test**: `<Namespace.Class.Method>`
- **Categorías**: `<Smoke|Regression|...>`
- **Owner**: `@github-user`
- **Fecha detección**: `YYYY-MM-DD`
- **TTL (máx 7 días)**: `YYYY-MM-DD`

## Evidencias
- **Allure Run**: enlace
- **Trace**: enlace/artefacto
- **Logs CI**: enlace

## Patrón observado
- [ ] Falla intermitente
- [ ] Depende de tiempo
- [ ] Depende de datos
- [ ] Medio/bajo aislamiento
- **Descripción**:
  - …

## Hipótesis y pruebas
1. …
2. …

## Plan de corrección
- [ ] Causa raíz identificada
- [ ] Fix aplicado en rama `...`
- [ ] Validado en 10 ejecuciones consecutivas sin retry

## Criterios de salida de cuarentena
- 10 ejecuciones consecutivas **OK** (sin reintento)
- Pass rate del test ≥ 99%
- PR de reactivación mergeado
```
