# Etapa 1: compilar con el SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/PadronDeudores.Dominio/ src/PadronDeudores.Dominio/
COPY src/PadronDeudores.Infraestructura/ src/PadronDeudores.Infraestructura/
COPY src/PadronDeudores.Importador.Worker/ src/PadronDeudores.Importador.Worker/
RUN dotnet publish src/PadronDeudores.Importador.Worker -c Release -o /app

# Etapa 2: ejecutar solo con el runtime (el worker no es una API web).
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "PadronDeudores.Importador.Worker.dll"]
