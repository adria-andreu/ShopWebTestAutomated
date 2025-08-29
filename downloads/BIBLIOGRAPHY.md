# Bibliografía Técnica — ShopWebTestAutomated

> **Propósito:** Gestión centralizada de documentación técnica oficial por topic, siguiendo directrices de CLAUDE.md para priorizar manuales oficiales.

## Índice por Topic

- [.NET 8](#net-8)
- [Playwright](#playwright)
- [NUnit](#nunit)
- [Docker](#docker)
- [Allure](#allure)

---

## .NET 8

### Documentación Principal
- **Título**: .NET 8 Official Documentation
- **Autor**: Microsoft
- **Versión**: 8.0 (Current)
- **Fecha**: 2023-11-14
- **Enlace**: https://docs.microsoft.com/en-us/dotnet/
- **Local**: `downloads/dotnet/dotnet8-docs.md` (pendiente descarga)
- **Resumen**: Documentación completa del runtime .NET 8, APIs, herramientas de desarrollo y deployment.

### Testing en .NET 8
- **Título**: Unit testing in .NET using NUnit
- **Autor**: Microsoft Docs
- **Fecha**: 2023-10-15
- **Enlace**: https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
- **Local**: `downloads/dotnet/nunit-testing-dotnet.md` (pendiente descarga)
- **Resumen**: Guía oficial para testing unitario con NUnit en .NET 8.

---

## Playwright

### Documentación Principal
- **Título**: Playwright for .NET Documentation
- **Autor**: Microsoft Playwright Team
- **Versión**: 1.40.0
- **Fecha**: 2023-11-20
- **Enlace**: https://playwright.dev/dotnet/
- **Local**: `downloads/playwright/playwright-dotnet-v1.40.md` (pendiente descarga)
- **Resumen**: Documentación completa de Playwright para .NET, incluyendo APIs, best practices y ejemplos.

### Testing Patterns
- **Título**: Playwright Testing Best Practices
- **Autor**: Playwright Team
- **Fecha**: 2023-11-01
- **Enlace**: https://playwright.dev/dotnet/test-best-practices
- **Local**: `downloads/playwright/best-practices-v1.40.md` (pendiente descarga)
- **Resumen**: Patrones recomendados para testing con Playwright, incluyendo POM y paralelización.

---

## NUnit

### Documentación Principal
- **Título**: NUnit 4.0 Documentation
- **Autor**: NUnit Team
- **Versión**: 4.0.1
- **Fecha**: 2023-12-01
- **Enlace**: https://docs.nunit.org/
- **Local**: `downloads/nunit/nunit-4.0-docs.md` (pendiente descarga)
- **Resumen**: Documentación completa de NUnit 4.0, assertions, attributes, y paralelización.

### Parallel Execution
- **Título**: Parallelizable Attribute
- **Autor**: NUnit Team
- **Fecha**: 2023-11-15
- **Enlace**: https://docs.nunit.org/articles/nunit/writing-tests/attributes/parallelizable.html
- **Local**: `downloads/nunit/parallel-execution.md` (pendiente descarga)
- **Resumen**: Configuración y mejores prácticas para ejecución paralela de tests.

---

## Docker

### Documentación Principal
- **Título**: Docker Official Documentation
- **Autor**: Docker Inc.
- **Versión**: 24.0
- **Fecha**: 2023-11-30
- **Enlace**: https://docs.docker.com/
- **Local**: `downloads/docker/docker-docs-v24.md` (pendiente descarga)
- **Resumen**: Documentación completa de Docker, incluyendo Dockerfile, compose, y networking.

### .NET en Docker
- **Título**: .NET in Docker Best Practices
- **Autor**: Microsoft
- **Fecha**: 2023-10-20
- **Enlace**: https://docs.microsoft.com/en-us/dotnet/core/docker/
- **Local**: `downloads/docker/dotnet-docker-practices.md` (pendiente descarga)
- **Resumen**: Mejores prácticas para containerizar aplicaciones .NET.

---

## Allure

### Documentación Principal
- **Título**: Allure Framework Documentation
- **Autor**: Allure Team (Qameta)
- **Versión**: 2.24.0
- **Fecha**: 2023-11-10
- **Enlace**: https://docs.qameta.io/allure/
- **Local**: `downloads/allure/allure-docs-v2.24.md` (pendiente descarga)
- **Resumen**: Documentación completa de Allure Framework para reporting de tests.

### Allure NUnit Integration
- **Título**: Allure NUnit Adapter
- **Autor**: Allure Team
- **Versión**: 2.12.1
- **Fecha**: 2023-10-25
- **Enlace**: https://github.com/allure-framework/allure-csharp
- **Local**: `downloads/allure/allure-nunit-integration.md` (pendiente descarga)
- **Resumen**: Integración específica de Allure con NUnit para reporting avanzado.

---

## Estado de Descarga y Mantenimiento

| Topic | Documentos | Estado | Última Verificación | Próxima Revisión |
|-------|------------|--------|--------------------|--------------------|
| .NET 8 | 2 | ⏳ Pendiente | - | 2025-02-15 |
| Playwright | 2 | ⏳ Pendiente | - | 2025-02-15 |
| NUnit | 2 | ⏳ Pendiente | - | 2025-02-15 |
| Docker | 2 | ⏳ Pendiente | - | 2025-02-15 |
| Allure | 2 | ⏳ Pendiente | - | 2025-02-15 |

## Proceso de Descarga (TD-08)

> **Nota:** La descarga efectiva de documentación se ha identificado como TD-08 para iteración futura, ya que requiere:
> 1. Herramientas de scraping/download automatizado
> 2. Conversión de HTML/PDF a Markdown  
> 3. Versionado y actualización automática
> 4. Validación de enlaces y freshness

## Notas de Uso

- **Prioridad de consulta**: Siempre consultar documentación local antes de buscar online
- **Actualizaciones**: Verificar versiones nuevas mensualmente
- **Archivo histórico**: Mantener versiones anteriores en subcarpeta `archive/`
- **Convenciones de naming**: `{topic}-{feature}-v{version}.md`