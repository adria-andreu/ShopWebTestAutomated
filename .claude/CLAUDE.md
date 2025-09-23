# CLAUDE.md — Índice maestro y reglas operativas del proyecto

> **Propósito**: `CLAUDE.md` es el punto de entrada que Claude carga automáticamente para entender **qué leer primero**, **cómo comportarse** y **cómo evolucionar el proyecto por iteraciones**. Aquí definimos el **orden de lectura**, el **flujo de iteraciones**, las **reglas de comportamiento**, la ubicación de la documentación de referencia técnica prioritaria y los **enlaces** (imports) al resto de documentación del proyecto. 

---

## Nota global sobre placeholders y ejemplos

En este documento se incluyen nombres de archivo, rutas, identificadores e IDs (por ejemplo:  
`memoria_proyecto_[slug].md`, `{topic}`, `itNN_YYYYMMDD`, `feature/<slug-corto>`, `EP-01`, `T-001`, `TD-01`).

**Todos ellos son ejemplos o placeholders** que deben adaptarse al contexto real del proyecto.

- **[slug]**, **{topic}**, **<slug-corto>** → Sustituir por el nombre, tema o identificador reales.
- **itNN_YYYYMMDD** → Reemplazar `NN` por el número de iteración y `YYYYMMDD` por la fecha.
- **IDs (EP, T, TD)** → Usar la numeración interna de cada equipo.
- **Rutas de carpetas/archivos** → Ajustar a la estructura concreta del repositorio, manteniendo la lógica.

**Regla general:** Mantener el formato y propósito de cada elemento, pero no usar literalmente los nombres aquí mostrados salvo que así se indique expresamente.

---

## 1) Orden de lectura (imports)

> Carpetas:
Por cada proyecto habrá creada una copia de esta estructura de carpetas:

/project_memory
/historical_project_memory
/technical_debt
/roadmap
/downloads
/templates


> Usa imports `@ruta/al/archivo` para asegurar el **orden de lectura** y evitar duplicidades. Claude carga estos ficheros como memoria del proyecto al iniciar.

```md
# Documentos de referencia (orden estricto)
1. @docs/claude.dm                         <!-- Manual maestro de comportamiento -->
2. @docs/memoria_proyecto_[slug].md        <!-- Sólo iteración vigente -->
3. @docs/memoria_proyecto_[slug]_hist.md   <!-- Iteraciones pasadas (histórico) -->
4. @docs/technical_debt_[slug].md          <!-- Deuda técnica viva -->
5. @docs/roadmap_[slug].md                 <!-- Roadmap vivo + histórico -->
6. @docs/directrices_versionado.md         <!-- Git, tags, ramas y mensajes -->
7. @downloads/BIBLIOGRAPHY.md              <!-- Bibliografía técnica por topic -->
8. @templates/memoria_proyecto.md
9. @templates/memoria_proyecto_hist.md
10. @templates/technical_debt.md
11. @templates/roadmap.md
12. @templates/directrices_versionado.md

Perfecto.
Aquí tienes tu **`CLAUDE.md` final** con todo lo que hemos trabajado integrado:

---

```md
# CLAUDE.md — Índice maestro y reglas operativas del proyecto

> **Propósito**: `CLAUDE.md` es el punto de entrada que Claude carga automáticamente para entender **qué leer primero**, **cómo comportarse** y **cómo evolucionar el proyecto por iteraciones**. Aquí definimos el **orden de lectura**, el **flujo de iteraciones**, las **reglas de comportamiento**, la ubicación de la documentación de referencia técnica prioritaria y los **enlaces** (imports) al resto de documentación del proyecto. 

---

## Nota global sobre placeholders y ejemplos

En este documento se incluyen nombres de archivo, rutas, identificadores e IDs (por ejemplo:  
`memoria_proyecto_[slug].md`, `{topic}`, `itNN_YYYYMMDD`, `feature/<slug-corto>`, `EP-01`, `T-001`, `TD-01`).

**Todos ellos son ejemplos o placeholders** que deben adaptarse al contexto real del proyecto.

- **[slug]**, **{topic}**, **<slug-corto>** → Sustituir por el nombre, tema o identificador reales.
- **itNN_YYYYMMDD** → Reemplazar `NN` por el número de iteración y `YYYYMMDD` por la fecha.
- **IDs (EP, T, TD)** → Usar la numeración interna de cada equipo.
- **Rutas de carpetas/archivos** → Ajustar a la estructura concreta del repositorio, manteniendo la lógica.

**Regla general:** Mantener el formato y propósito de cada elemento, pero no usar literalmente los nombres aquí mostrados salvo que así se indique expresamente.

---

## 1) Orden de lectura (imports)

> Carpetas:
Por cada proyecto habrá creada una copia de esta estructura de carpetas:
```

/project\_memory
/historical\_project\_memory
/technical\_debt
/roadmap
/downloads
/templates

````

> Usa imports `@ruta/al/archivo` para asegurar el **orden de lectura** y evitar duplicidades. Claude carga estos ficheros como memoria del proyecto al iniciar.

```md
# Documentos de referencia (orden estricto)
1. @docs/claude.dm                         <!-- Manual maestro de comportamiento -->
2. @docs/memoria_proyecto_[slug].md        <!-- Sólo iteración vigente -->
3. @docs/memoria_proyecto_[slug]_hist.md   <!-- Iteraciones pasadas (histórico) -->
4. @docs/technical_debt_[slug].md          <!-- Deuda técnica viva -->
5. @docs/roadmap_[slug].md                 <!-- Roadmap vivo + histórico -->
6. @docs/directrices_versionado.md         <!-- Git, tags, ramas y mensajes -->
7. @downloads/BIBLIOGRAPHY.md              <!-- Bibliografía técnica por topic -->
8. @templates/memoria_proyecto.md
9. @templates/memoria_proyecto_hist.md
10. @templates/technical_debt.md
11. @templates/roadmap.md
12. @templates/directrices_versionado.md
````

---

## 2) Flujo de iteraciones en espiral (definición y checklist)

**Objetivo**: crecer en “espirales” donde cada iteración **suma valor funcional**, deja **documentación clara** y **no rompe** lo anterior.

**Cierre de iteración (Definition of Done, DoD):**

* [ ] Todas las **tareas** de la **épica** activa completadas y verificadas.
* [ ] **Pruebas** (unitarias/integración/E2E) verdes y reproducibles.
* [ ] **Memoria vigente** actualizada (resumen, diario, fechas, tareas).
* [ ] **Deuda técnica** triada (impacto, prioridad, complejidad) y enlazada al Roadmap.
* [ ] **Roadmap** actualizado: snapshot al histórico y preparación de la siguiente espiral.
* [ ] **Commit + tag** de versión de iteración (`itNN_YYYYMMDD`) en `main`.
* [ ] **Changelog/Release notes** mínimas si aplica.
* [ ] **Bibliografía técnica** revisada y actualizada (nuevas versiones, cambios de enlace).
* [ ] **Enlaces internos/externos** verificados (sin 404).
* [ ] **IDs EP/T/TD** únicos y consistentes.

**Acciones al cerrar:**

1. Mover contenido de la memoria vigente → histórico.
2. Vaciar/plantillar nueva memoria.
3. Actualizar Roadmap (cerradas → Histórico; backlog priorizado).
4. Trazar deuda técnica → candidatas de próxima iteración.
5. Crear tag Git y anotar referencias.

---

## 3) Reglas de comportamiento para Claude

### 3.1. Lectura y carga de contexto

1. Claude debe leer los documentos referenciados en `CLAUDE.md` en el orden especificado.
2. Si un documento no existe o está desactualizado:

   * Registrar en la Memoria vigente (fecha, nota).
   * Crear tarea en Roadmap para crearlo/actualizarlo.
3. Claude debe fusionar información evitando duplicar contenido; contradicciones → registrar en Technical Debt como “Conflicto de documentación”.
4. Precedencia en conflictos: (1) `docs/claude.dm`, (2) `CLAUDE.md`, (3) resto de documentos.
   Severidad: Alta (impacto en diseño), Media (terminología), Baja (formato).
5. Si el trabajo involucra un *topic* técnico, verificar que el manual oficial esté en `/downloads/{topic}/` y referenciarlo antes de buscar en fuentes externas.

### 3.2. Interpretación de instrucciones

* Verificar si está en alcance de la iteración actual; si no, documentar en Memoria + Debt + Roadmap.
* Si hay varias soluciones: presentar pros/contras/impacto y recomendar la más alineada con el plan.
* Si contradice directrices del proyecto: notificar y sugerir alternativa válida.

### 3.3. Estilo de comunicación

* Siempre 2 capas: breve/no técnica + apéndice técnico.
* Evitar jerga innecesaria salvo en la parte técnica.
* Usar IDs y enlaces cruzados para trazabilidad.
* Señalar explícitamente supuestos vs. hechos documentados.

### 3.4. Documentación generada por Claude

* Cada tarea completada → registrar fecha, acción, motivo en Memoria vigente.
* Referenciar tarea en Roadmap (T-xxx).
* Si hay deuda técnica, crear TD-xxx y enlazar.
* Evitar duplicar texto: Memoria = contexto/decisiones; Debt = plan técnico; Roadmap = tarea accionable.

### 3.5. Control de calidad y autocorrección

* Validar cumplimiento de directrices antes de entregar respuesta/código.
* Detectar y proponer mejoras de bajo coste.
* Corregir errores previos y documentar su impacto.

### 3.6. Gestión de la iteración

* Ayudar a cumplir el checklist de cierre.
* Bloquear cierre si faltan puntos críticos.
* Proponer tareas futuras basadas en Debt, hallazgos y cambios externos.

---

## 4) Estructura recomendada del repo

```
/docs/                  # Documentos de trabajo
/downloads/              # Manuales y bibliografía técnica
/templates/              # Plantillas reutilizables
CLAUDE.md
.claude/
  commands/              # Slash commands reutilizables
  agents/                # Subagentes (opcional)
  settings.json          # Preferencias compartidas
```

---

## 5) Gestión de Bibliografía Técnica

> **Propósito:** Garantizar que cada *topic* (framework, librería, API, etc.) tenga documentación técnica fiable, preferentemente oficial, organizada en `/downloads`.

### 1. Criterios de selección

* Prioridad máxima: manuales oficiales y guías del fabricante.
* Secundarias: artículos técnicos y documentación comunitaria solo si:

  * No hay manual oficial, o
  * No cubre el caso concreto.
* Evitar contenido sin autor o de origen dudoso.

### 2. Procedimiento

1. Identificar el *topic* y su alcance.
2. Localizar y descargar el manual oficial (PDF, HTML, etc.).
3. Guardar en `/downloads/{topic}/` con nombre que incluya versión y fecha
   (ej.: `manual_v1.2_2024-10.pdf`).
4. Crear `BIBLIOGRAPHY.md` en esa carpeta con:

   * Título, autor/es, fecha, enlace original, resumen, versión.
5. Referenciar en la Memoria del proyecto con enlace relativo.

### 3. Uso y mantenimiento

* Revisión mínima cada 6 meses o antes de iniciar nuevo trabajo sobre el *topic*.
* Consultar `/downloads` antes de buscar en internet.
* Mantener versiones antiguas en `/downloads/{topic}/archive/`.

---

## 6) Convenciones y estilo de documentación

* Audiencia dual: resumen para todos + apéndice técnico.
* IDs y enlaces cruzados: EP-NN, T-NNN, TD-NN.
* Fechas ISO (YYYY-MM-DD) y zona horaria Europe/Madrid.
* Lenguaje claro y activo.
* Tablas para Roadmap/Debt.
* No duplicar: una idea vive en un sitio, el resto enlaza.
* Glosario opcional si el dominio lo requiere.

---

## 7) Importación de plantillas

Las plantillas de Memoria, Roadmap, Debt y Versionado viven en `/templates/` para permitir reutilización y mantenimiento independiente.
Importar en `CLAUDE.md` según el orden de lectura (ver §1).


## 8) Política de Pruebas (Unit, Integration, E2E)

**Objetivo**: asegurar pirámide de test saludable (**Unit > Integration > E2E**), aislando responsabilidades y habilitando *fast feedback*.

### 8.1) Reglas para Unit Tests (obligatorias)
- **Ubicación**: `tests/ShopWeb.UnitTests/` con proyecto `.csproj` dedicado (p. ej. `ShopWeb.UnitTests.csproj`) que referencia sólo las librerías bajo prueba en `src/*`.
- **Aislamiento estricto**: sin red, sin disco, sin UI/Playwright, sin BD. Usa **interfaces** y **dependency injection** para aislar dependencias; si hace falta, usa *stubs* o *mocks* ligeros.
- **Framework**: NUnit 4.x + `Microsoft.NET.Test.Sdk` + `coverlet.collector`.
- **Estilo**: AAA (Arrange–Act–Assert); un comportamiento por test; nombres `Metodo_Escenario_Resultado`.
- **Velocidad**: <100ms por test; evita *Thread.Sleep*; tests deterministas.
- **Ejecución obligatoria**: en **cada PR** y en **main/nightly** **antes** de E2E.
- **Reporting**: TRX y cobertura (Cobertura/LCOV). Publicar artefactos en CI.
- **Quality Gate mínimo**: fallar la PR si hay **cualquier test unitario rojo**. (Cobertura opcional, ver Opción B).

### 8.2) Estructura técnica base (recomendación .NET)
/\repo personal excercise/
ShopWeb/ # Código bajo prueba
/tests/
UnitTests/
ShopWeb.UnitTests.csproj # NUnit + coverlet.collector
<*.cs> # Tests AAA
.gitHub/workflows/
unit-tests.yml # Pipeline Unit
tests.yml # Pipeline E2E
tools/
GateCheck/ # Quality gate (p95, pass rate, etc.)


### 8.3) Integración en el ciclo (A/B)

**Opción A — Básica (recomendada para adopción rápida)**
1) Añadir/ubicar `tests/ShopWeb.UnitTests/ShopWeb.UnitTests.csproj`.  
2) Ejecutar en CI `dotnet test` + `--collect:"XPlat Code Coverage"`.  
3) Subir TRX y cobertura como artefactos.  
4) Encadenar E2E sólo si Unit Tests pasan.

**Opción B — Avanzada (gates + métricas unificadas)**
1) Además de A, convertir cobertura a HTML y publicar (p. ej., ReportGenerator).  
2) Enforce cobertura mínima (p. ej., 60% inicial, +5pp por iteración):  
   `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=60`.  
3) (Opcional) Añadir *Allure.NUnit* para report unified si el proyecto ya usa Allure en E2E.  
4) Registrar métricas mínimas por suite en `artifacts/test-metrics.jsonl` para GateCheck.

**Relación Cloud ↔ Project**  
- **CLAUDE** define estas reglas universales.  
- **PROJECT** (ShopWebTestAutomated/PROJECT.md)** especifica *cómo* se ejecutan (rutas, comandos locales/CI, YAML, thresholds).
