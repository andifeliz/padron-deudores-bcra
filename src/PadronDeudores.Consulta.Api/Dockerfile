# Etapa 1: compilar con el SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/PadronDeudores.Dominio/ src/PadronDeudores.Dominio/
COPY src/PadronDeudores.Infraestructura/ src/PadronDeudores.Infraestructura/
COPY src/PadronDeudores.Consulta.Api/ src/PadronDeudores.Consulta.Api/
RUN dotnet publish src/PadronDeudores.Consulta.Api -c Release -o /app

# Etapa 2: ejecutar con el runtime de ASP.NET.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
# curl solo para el healthcheck de Docker.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "PadronDeudores.Consulta.Api.dll"]
