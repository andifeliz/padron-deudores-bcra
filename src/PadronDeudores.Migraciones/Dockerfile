# Etapa 1: compilar con el SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/PadronDeudores.Migraciones/ src/PadronDeudores.Migraciones/
RUN dotnet publish src/PadronDeudores.Migraciones -c Release -o /app

# Etapa 2: ejecutar solo con el runtime.
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "PadronDeudores.Migraciones.dll"]
