# Dockerfile — entorno reproducible para .NET 8 + Playwright + tests
# Uso:
#   docker build -t shop-tests .
#   docker run --rm -e BROWSER=chromium -v "$PWD/artifacts:/app/artifacts" shop-tests
#   (BROWSER: chromium | firefox | webkit)

FROM mcr.microsoft.com/playwright/dotnet:<version>
WORKDIR /app

# Instala Playwright CLI y navegadores con dependencias del sistema
RUN dotnet tool install --global Microsoft.Playwright.CLI && \
/root/.dotnet/tools/playwright install --with-deps

# Copia el código (ajusta los patrones si tu solución usa múltiples .sln)
COPY . /app

# Restaura y compila
RUN dotnet restore && dotnet build -c Release

# Variables de entorno
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV BROWSER=chromium

# Ejecuta tests con paralelización y pasa el navegador por TestRunParameters
# Monta artifacts/ desde fuera para recoger reportes (ver ejemplo de docker run arriba)
CMD ["dotnet","test","-c","Release","-m:4","--no-build","--",
     "TestRunParameters.Parameter(name=Browser,value=chromium)"]


4) Checklist rápido para integrarlo (DoD de esta iteración)

 Copiar Dockerfile en la raíz y probar:

docker build -t shop-tests .

docker run --rm -e BROWSER=firefox -v "$PWD/artifacts:/app/artifacts" shop-tests

 Añadir proyecto tools/GateCheck (csproj + Program.cs).

 Confirmar que tu suite genera artifacts/run-metrics.json (vía RunMetricsCollector.WriteSummary() en [OneTimeTearDown] del assembly).

 Crear .github/workflows/tests.yml (el de arriba) y verificar:

Se ejecutan los 3 navegadores en paralelo.

Se suben artefactos de Allure y artifacts/.

GateCheck decide el estado final del job (fallará si <90% pass rate o p95 > 12 min).