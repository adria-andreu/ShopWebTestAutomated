¡De una! Aquí tienes el archivo **listo para pegar** en tu repo como:

`/docs/examples/PULL_REQUEST_TEMPLATE_EXAMPLE.md`

> Nota: Este es el **modelo obligatorio** para PRs. Para activarlo en GitHub, copia este mismo contenido a **`.github/PULL_REQUEST_TEMPLATE.md`** (misma estructura). Mantiene las frases exactas que usa el *PolicyCheck* para validar cumplimiento.

---

````markdown
# Pull Request — Template Obligatorio

> **Propósito del cambio (1–2 líneas):**  
> _Describe brevemente el objetivo: bugfix/refactor/nueva capacidad infra/portabilidad, etc._

---

## ✅ Cumplimiento de políticas

- [ ] He leído y cumplo **CLAUDE.md** primario  
- [ ] He leído y cumplo **PROJECT.md**  
- [ ] Añadí **evidencias** (Allure, `artifacts/run-metrics.json`, traces)

<!-- Las líneas anteriores contienen las frases clave que valida el PolicyCheck:
"He leído y cumplo CLAUDE.md" y "He leído y cumplo PROJECT.md" -->

### Reglas tocadas (marca las que aplican)
- [ ] POM **sin asserts** y **sin sleeps** (esperas centralizadas)
- [ ] **Sin selectores en tests** (tests orquestan Flows)
- [ ] Paralelización segura (sin estado compartido)
- [ ] Integración Allure (adjuntos en fallo)
- [ ] Portabilidad A/B vía `SiteId` (sin tocar tests)
- [ ] Seguridad: **sin secretos** en repo/logs
- [ ] Linters/Analyzers **sin warnings**

---

## 🔍 Resumen del cambio
**Qué se cambió:**  
- …

**Por qué:**  
- …

**Scope:**  
- Módulos impactados: `Pages/*`, `Flows/*`, `Utilities/*`, `Browsers/*`, `Config/*`  
- Breaking changes: [ ] No · [ ] Sí (explica abajo)

---

## 🧪 Plan de pruebas
**Comandos usados localmente**
```bash
dotnet test -m:4 \
  -- TestRunParameters.Parameter(name=Browser,value=chromium) \
  TestRunParameters.Parameter(name=SiteId,value=A) \
  --where "cat == Smoke"
````

**Cobertura de categorías ejecutadas**

* [ ] Smoke  ·  \[ ] Regression  ·  \[ ] Negative  ·  \[ ] Edge  ·  \[ ] A11y  ·  \[ ] Visual

---

## 📎 Evidencias (obligatorio)

* **Allure run (enlace/artefacto):** …
* **`artifacts/run-metrics.json`:** adjunto/subido como artefacto
* **Traces/Screenshots (en fallo):** rutas o artefactos

**Snapshot de métricas** (extrae los valores relevantes):

```json
{
  "passRate": 0.94,
  "p95": 540000,
  "total": 128,
  "flakyCandidates": ["Tests.Cart.AddItem_ShouldAppearInCart"]
}
```

---

## 🚦 Quality Gates (esperado vs actual)

| Gate                   |           Umbral | Valor actual | Resultado |
| ---------------------- | ---------------: | -----------: | --------- |
| Pass rate              |      ≥ 0.90 (PR) |         0.94 | ✅         |
| p95 (ms)               | ≤ 720000 (12min) |       540000 | ✅         |
| Flaky ratio (opcional) |           ≤ 0.05 |         0.02 | ✅         |

> Si algún gate falla, explica la mitigación o justifica la excepción (revisión CODEOWNERS requerida).

---

## 🏷️ Flaky & Quarantine (si aplica)

* ¿Se movió algún test a **Quarantine**? \[ ] No · \[ ] Sí → Issue: #… · **Owner**: @… · **TTL**: `YYYY-MM-DD`
* Criterio de salida: **10 ejecuciones verdes** sin retry

---

## 🔒 Seguridad

* Secretos: \[ ] No se añadieron secretos · \[ ] N/A
* Logs: \[ ] Validado que no imprimen secretos
* Dependencias: salida de `dotnet list package --vulnerable` revisada

---

## ⚠️ Riesgos & mitigación

* Riesgos principales: …
* Plan de rollback: …
* Impacto en portabilidad (A/B): …
* Consideraciones de datos de prueba (unicidad/cleanup): …

---

## 👀 Review

* **Impacto en capas**: Tests → Flows → Pages solamente (sin inversiones)

## Checklist
- [ ] Tests añadidos/actualizados
- [ ] Docs/Runbook actualizados
- [ ] Etiquetas correctas
- [ ] Reviewers/CODEOWNERS asignados
- [ ] CI/Gates en verde
- [ ] Closes #id (si aplica)

* **Checklist técnico**

  * [ ] CI **verde** (incluye GateCheck)
  * [ ] **0** `Thread.Sleep`, **0** selectores en tests
  * [ ] Allure + `run-metrics.json` subidos
  * [ ] Linters/Analyzers **sin warnings**
  * [ ] Docker/compose (si aplica) verificados

**Revisores sugeridos**: @… (CODEOWNERS se solicitarán automáticamente)

```