# UnitTests (NUnit, .NET 8)

**Ámbito:** pruebas unitarias puras sobre `src/*`.  
**Reglas de aislamiento:** sin red, sin disco, sin UI, sin BD y sin reloj real; abstraer dependencias mediante interfaces e inyección.  
**Estilo:** AAA (Arrange-Act-Assert), un comportamiento por test, nombres `Metodo_Escenario_Resultado`.

## Ejecutar local con cobertura

```bash
dotnet test tests/ShopWeb.UnitTests/ShopWeb.UnitTests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory artifacts/TestResults
