# Directrices de versionado (reutilizable)

## Rama principal
- Estrategia: trunk-based con ramas cortas por feature/fix.
- Branch naming: feature/<slug-corto> | fix/<slug-corto>

## Tags
- Al cerrar iteración: `itNN_YYYYMMDD`
- Releases (si aplica): SemVer `vX.Y.Z` con CHANGELOG

## Commits
- Mensaje: tipo(scope)!: resumen breve
  - cuerpo opcional con contexto, refs T-/TD-
  - footer: Closes #ISSUE, Ref TD-XX

## PRs
- Descripción con: objetivo, enfoque, pruebas, riesgos, verificación.