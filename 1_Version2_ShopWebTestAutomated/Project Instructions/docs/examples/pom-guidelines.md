```md
# Guía POM (Page Object Model) — Do’s & Don’ts

> Objetivo: POM **delgado**, predecible y portable. Los tests **no** contienen selectores ni waits; solo orquestan **flows**.

## 1) Principios
- **Responsabilidad única**: el POM modela una página o componente, con **acciones** y **consultas**.
- **Sin asserts**: las aserciones viven en los tests o en helpers de assertions.
- **Sin sleeps**: usar `Locator.WaitForAsync` o utilidades centralizadas (`Utilities/Waits.cs`).
- **Interfaces primero**: tests dependen de **interfaces** (`IProductPage`), no de implementaciones concretas.
- **Component Objects**: secciones repetibles (header, modal) como clases reutilizables.

## 2) Estructura sugerida
```

Pages/
Components/
BaseComponent.cs
Header.cs
Modal.cs
Home/
IHomePage.cs
SiteA\_HomePage.cs
SiteB\_HomePage.cs
Product/
IProductPage.cs
SiteA\_ProductPage.cs
SiteB\_ProductPage.cs

````

## 3) Selectores y waits
- Define **locators privados** y expresivos:
  ```csharp
  private ILocator BtnAddToCart => _page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" });
  private ILocator Price => _page.Locator(".price");
````

* **Esperas**:

  * Antes de interactuar: `await BtnAddToCart.WaitForAsync(...)`.
  * Tras acciones que cambian la UI: esperar a un **estado observable** (ej. contador del carrito).
  * Centraliza esperas no triviales en `Utilities/Waits.cs`.

## 4) Iframes y componentes

* **FrameComponent**: resuelve el frame **cada vez** (evita stale references).

  ```csharp
  public class FrameComponent : BaseComponent {
    private readonly string _frameName;
    public FrameComponent(IPage page, string frameName) : base(page, page.Locator($"iframe[name='{frameName}']")) { _frameName = frameName; }
    private IFrame Frame => Page.Frame(_frameName)!;
    public Task ClickOkAsync() => Frame.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
  }
  ```

## 5) Patrón de retorno

* Métodos **async** (`Task`, `Task<T>`). Evita **fluent chaining** salvo en componentes puros.
* Los POM devuelven **valores** (precio, texto, flags) o realizan acciones; no mezclan decisiones de negocio.

## 6) Anti-patrones (NO hacer)

* ❌ Lógica de negocio/decisiones en POM.
* ❌ `Thread.Sleep`.
* ❌ Aserciones dentro del POM.
* ❌ Acceso a servicios externos desde el POM.
* ❌ Selectores en tests.

## 7) Ejemplo breve

```csharp
public interface IProductPage {
  Task<decimal> GetPriceAsync();
  Task AddToCartAsync();
}

public class SiteA_ProductPage : IProductPage {
  private readonly IPage _page;
  private ILocator BtnAddToCart => _page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" });
  private ILocator Price => _page.Locator(".price");

  public SiteA_ProductPage(IPage page) { _page = page; }

  public async Task<decimal> GetPriceAsync() {
    await Price.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    var text = await Price.InnerTextAsync();
    return PriceParser.ToDecimal(text);
  }

  public async Task AddToCartAsync() {
    await BtnAddToCart.WaitForAsync();
    await BtnAddToCart.ClickAsync();
  }
}
```

## 8) Checklist de revisión (PR)

* [ ] Sin selectores en tests.
* [ ] POM sin asserts/negocio.
* [ ] Waits centralizados o `Locator.WaitForAsync`.
* [ ] Interfaces implementadas por sitio (A/B).
* [ ] Componentes reutilizables para secciones repetidas.

````