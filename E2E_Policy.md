
````markdown
# E2E_Policy.md — End-to-End Governance

## 1. Objetivo
- Alinear con el Roadmap (Iteración 7).
- Garantizar gobernanza clara de los E2E tests.
- Asegurar uso de test data **determinista y reproducible**.
- Evitar duplicación con Unit/Integration.
- Generar el código para automatizar (claude task) los tests a partir de los pasos definidos en **Setup** y **Steps** que voy a escribir yo como QA siguiendo las [Guidelines Writing Test](guidelines-writing-test-automation.md).

---

## 2. Alcance y Límites de los E2E

### Ejemplos concretos
✅ Entra:
- Login básico con credenciales válidas.
- Checkout completo con un producto en stock.
- Seleccionar un producto en stock y añadirlo al carrito.

❌ No entra:
- Validar todos los errores de formulario de login (Unit/Integration).
- Validar edge cases de expiración de token (Integration).
- Testear cálculos matemáticos internos (Unit).

### Reglas adicionales
- Todo lo que pueda probarse en Unit o Integration no debe duplicarse en E2E.
- Los E2E deben centrarse en flujo cross-component y experiencia de usuario.
- Relación con otras capas: pirámide Unit > Integration > E2E (referencia a CLAUDE.md).

---

## 3. Categorías de E2E
- **Smoke** → validación mínima en PR (`AutoStatus.Automated`).
- **Regression** → flujos completos en nightly (`AutoStatus.Automated`).
- **Critical Path** → checkout, login, payment (`AutoStatus.Automated`).
- **UI-only** → validaciones de interfaz cuando aplique (`AutoStatus.Automated`).
- **Manual** → solo para documentar procesos no automatizables (`AutoStatus.Manual`).
- **Manual_Assist** → pasos semiautomatizados que requieren ayuda manual (`AutoStatus.Manual_Assist`).
- **Automated** → cualquier test ejecutable en CI/CD.

### Tabla de ejemplos

| Categoría          | Ejemplo                                                      | Criterio de uso                                    |
|--------------------|--------------------------------------------------------------|----------------------------------------------------|
| **Smoke**          | Login básico, abrir dashboard                                | PR rápido, máximo 2-3 min                          |
| **Regression**     | Checkout con descuento, cambio de idioma, flujos secundarios | Nightly, cobertura más amplia                      |
| **Critical Path**  | Checkout, alta de usuario, recuperación de contraseña        | Negocio vital, siempre en PR y nightly             |
| **UI-only**        | Validar visibilidad de un icono, layout básico               | Validaciones visuales simples                      |
| **Manual**         | Validar integración con sistema externo de pagos             | No automatizable en CI                             |
| **Manual_Assist**  | Configuración de parámetros en UI con apoyo de script        | Requiere pasos manuales pero con soporte           |
| **Automated**      | Cualquier caso ejecutable en CI                              | Todas las categorías salvo Manual / Manual_Assist  |

### Promoción/democión
- Un test pasa de **Smoke a Regression** si:
  - Incrementa duración o dependencia externa.
  - No es crítico para merges diarios.
- Un test se degrada a **UI-only** si solo valida presentación sin lógica de negocio.

---

## 4. Test Data determinista
- Uso de **fixtures JSON versionados** (`/Config/TestData/`).
- Prohibido `random` o `DateTime.Now` directo.
- Patrón `DataFactory` con builders deterministas.
- Mecanismo `Seed/Reset` reproducible en CI.

### Snippet de DataFactory
```csharp
public static class DataFactory
{
    public static User CreateDefaultUser()
    {
        return new UserBuilder()
            .WithUsername("User_E2E1")
            .WithEmail("user.e2e1@example.com")
            .WithPassword("Password123!")
            .Build();
    }

    public static Product CreateDefaultProduct()
    {
        return new ProductBuilder()
            .WithName("Product_E2E1")
            .WithPrice(50.00m)
            .WithStock(5)
            .Build();
    }
}
````

### Política fechas/horas

* Prohibido usar `DateTime.Now`.
* Usar `DateProvider.Fixed("2025-09-21")` o `DateProvider.Today()` con valor inyectado en CI.
* Siempre documentar la fecha de referencia en el test.

### Regla de cleanup

Cada flow que cree datos persistentes debe exponer:

```csharp
public async Task CleanupAsync()
{
    await DeleteUserAsync("User_E2E1");
    await DeleteProductAsync("Product_E2E1");
}
```

---

## 5. Gobernanza & Enforcement

### Tabla de validación

| Criterio                         | Cumple | Comentario                               |
| -------------------------------- | ------ | ---------------------------------------- |
| Naming y categorías correctas    | ✅/❌    | Ejemplo: `[Category(Tag.CriticalPath)]`  |
| Test data determinista           | ✅/❌    | Uso de `DataFactory`, no random          |
| Sin duplicación Unit/Integration | ✅/❌    | Validar que no repite lógica ya cubierta |
| Cleanup documentado en flows     | ✅/❌    | Flow implementa `CleanupAsync`           |
| Verify usado en lugar de asserts | ✅/❌    | Buscar `Verify.*`                        |

### Checklist PR

* [ ] Naming y categorías correctas.
* [ ] Test data determinista.
* [ ] Sin duplicar lógica de Unit/Integration.
* [ ] Cleanup documentado en flows.

### Quality Gates

* Se aplican desde `PROJECT.md` (passRate, p95, flakyRatio).

### CLAUDE enforcement

* Validación automática del checklist en PR (si aplica).

### Validadores automáticos

* Claude enforcement (parse de Setup/Steps).
* Regex CI: fallar si detecta `Assert.` en tests.
* GateCheck: validar que se generan métricas + artifacts en cada run.

---

## 6. Referencias Cruzadas

## 6. Referencias Cruzadas
- [CS-Test Cases Writing Guidelines (Top Reference)](CS-Test-Cases-Writing-Guidelines.pdf) → **Documento maestro para estilo, naming, Setup/Steps/Confirms**.  
- [Guidelines Writing Test](guidelines-writing-test-automation.md) → adaptación práctica al proyecto actual.  
- [PROJECT.md](PROJECT.md) → arquitectura, quality gates, roadmap.  
- [CLAUDE.md](CLAUDE.md) → precedencia de reglas y enforcement global.

---

## 7. Verify Methods

### Propósito

Estandarizar las aserciones en los tests mediante métodos `Verify.*` que envuelven a los asserts del framework (NUnit/FluentAssertions).
Esto aporta:

* Legibilidad homogénea (alineado con los *Confirms* del [Guidelines Writing Test](guidelines-writing-test-automation.md)).
* Mejor trazabilidad (los `Verify.*` pueden loguear mensajes o adjuntar artefactos).
* Evitar duplicación de lógica de validación en cada test.

### Reglas

* Usar siempre `Verify.*` en lugar de asserts directos (`Assert.*`, `.Should()`, etc.). El uso directo de `Assert` o `FluentAssertions` queda **prohibido**.
* Sintaxis alineada con los *Confirms* → voz pasiva + presente.
* Mensaje de error siempre obligatorio como tercer parámetro.

### Ejemplos
// 🔹 Pseudo-code example of Verify helpers for test policies
// (clean, generic, and not tied to any framework)

// Verify equality
Verify.Equals(actualValue, expectedValue, "Values should be equal");

// Verify inequality
Verify.NotEquals(actualValue, unexpectedValue, "Values should not be equal");

// Verify true/false conditions
Verify.True(condition, "Condition should be true");
Verify.False(condition, "Condition should be false");

// Verify nullability
Verify.NotNull(objectValue, "Object should not be null");
Verify.Null(objectValue, "Object should be null");

// Verify collections
Verify.Empty(listValue, "List should be empty");
Verify.Equals(listValue, expectedList, "Lists should match");

// Verify string contains
Verify.Contains(actualString, expectedSubstring, "String should contain substring");
Verify.DoesNotContain(actualString, forbiddenSubstring, "String should not contain substring");

// Verify ranges and ordering
Verify.GreaterThan(actualNumber, minValue, "Value should be greater than min");
Verify.LessThan(actualNumber, maxValue, "Value should be less than max");
Verify.Ordered(collection, "Collection should be ordered");

// Force fail/pass
Verify.Fail("Force test failure");
Verify.Pass("Force test success");

```

### Extensibilidad

Los métodos `Verify.*` deben centralizarse en `Utilities/Verify.cs` e incluir:

* `VerifyEquals(expected, actual, message)`
* `VerifyTrue(condition, message)`
* `VerifyFalse(condition, message)`
* `VerifyNotNull(object, message)`
* `VerifyContains(collection, item, message)`
* `VerifyCount(collection, expectedCount, message)`
* `VerifyGreaterThan(value, threshold, message)`

### Política de mensajes

* Siempre en inglés.
* Voz pasiva + presente.
* Una línea clara, sin abreviaturas.
* Ejemplo:

  ```csharp
  Verify.VerifyEquals(cart.Items.Count, 2, "Cart item count is displayed correctly");
  ```

### Integración con reporting

Los `Verify.*` deben:

* Loguear en consola y adjuntar al `test-metrics.jsonl`.
* En fallo: adjuntar screenshot y trace al report Allure.

---

## 8. Ejemplo completo de E2E test

### Caso: Checkout con producto simple

#### Categorías

```csharp
[TestName("Checkout_SimpleProduct_Success")]
[Category(AutoStatus.Automated), Category(Tag.CriticalPath), Category(Tag.Regression)]
[Describe("Ensure that a user can add a product to the cart and complete checkout successfully")]
```

#### Setup

```csharp
// Precondiciones — construidas con DataFactory
Setup("Log in as test user 'CheckoutUser1'");
Setup("Ensure product 'Product1' exists with stock > 0");
Setup("Clean cart for 'CheckoutUser1' to start from empty");
```

#### Steps

```csharp
Step("Add 'Product1' to the shopping cart");
Step("Go to Checkout page");
Step("Enter valid shipping address");
Step("Select default payment method 'CreditCard'");
Step("Confirm the order");
```

#### Verify

```csharp
Verify.VerifyTrue(orderConfirmationPage.IsVisible("OrderSuccessMessage"),
    "'Order confirmation message' is displayed");

Verify.VerifyEquals(order.TotalAmount, expectedAmount,
    "Order total amount is displayed correctly");

Verify.VerifyNotNull(order.Id,
    "Order Id is generated and not null");
```

#### Ejemplo de DataFactory

```csharp
public static class DataFactory
{
    public static User CreateCheckoutUser()
    {
        return new UserBuilder()
            .WithUsername("CheckoutUser1")
            .WithEmail("checkout.user1@example.com")
            .WithPassword("Password123!")
            .Build();
    }

    public static Product CreateProduct()
    {
        return new ProductBuilder()
            .WithName("Product1")
            .WithPrice(100.00m)
            .WithStock(10)
            .Build();
    }
}
```