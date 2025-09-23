---
proyecto: [slug]
ultima_actualizacion: YYYY-MM-DD
---

# Épicas
- EP-01: [Descripción épica principal]
- EP-02: [Segunda épica]

# Tareas activas (iteración NN)
| ID    | Tarea                                    | Objetivo                         | Por qué | Estado       | ETA        | Vínculos |
|-------|------------------------------------------|----------------------------------|--------|-------------|------------|---------|
| T-001 | [Descripción tarea]                      | [Objetivo específico]            | [Justificación] | En curso | 2025-MM-DD | TD-01 |

## Checklist de cierre (iteración NN)
- [ ] Tareas de épica actual completas y validadas
- [ ] Memoria actualizada y movida a histórico
- [ ] Debt triada y enlazada
- [ ] Snapshot de tareas → Histórico
- [ ] Tag Git creado: itNN_YYYYMMDD

## Histórico de Roadmap (snapshot por iteración)
### Iteración NN-1 (YYYY-MM-DD)
- T-00X: [...estado final, referencias, PRs...]
### Iteración 05 - Quick Wins 

| ID    | Tarea                                                               | Épica | Prioridad | Complejidad | Dependencias |
|-------|---------------------------------------------------------------------|-------|-----------|-------------|--------------|
| T-033 | Renombrar "PolicyCheck" → "GateCheck" en PROJECT.md + pipelines      | EP-04 | ✅ COMPLETED | Baja        | -            |
| T-034 | Unificar rutas Unit Tests a `tests/ShopWeb.UnitTests/` (+nota migr.) | EP-04 | Alta      | Baja        | T-042        |
| T-035 | Cobertura ≥80% en CLAUDE (Política de Pruebas, Opción B)             | EP-04 | Alta      | Media       | PROJECT §15.6/15.7 |
| T-036 | Limpiar CLAUDE.md (bloque “Perfecto…”, fences, imports reales)       | EP-04 | Media     | Baja        | -            |
| T-037 | Consistencia config: TraceMode clave única (doc + ejemplo config)    | EP-04 | Media     | Media       | T-040        |
| T-038 | Añadir bloque “Terminología & IDs” en PROJECT.md                     | EP-04 | Media     | Baja        | -            |

### Iteración 05 - Cambios estructurales (1–2 semanas)

| ID    | Tarea                                                               | Épica | Prioridad | Complejidad | Dependencias |
|-------|---------------------------------------------------------------------|-------|-----------|-------------|--------------|
| T-039 | Imports canónicos: crear `docs/memoria_proyecto_shopweb.md` + hist. | EP-04 | Media     | Media       | memoria_proyecto_shopweb.md |
| T-040 | Runbook + ejemplos: crear `docs/runbook.md` y docs/examples          | EP-04 | Alta      | Media       | -            |
| T-041 | Portabilidad A/B: ejemplo mínimo ISiteProfile + normalizador precio/fecha | EP-02 | Media | Media | T-020, T-025 |
