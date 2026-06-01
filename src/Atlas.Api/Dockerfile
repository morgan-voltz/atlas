# syntax=docker/dockerfile:1
# Image de l'API Atlas (.NET 10). Build multi-étapes ; CONTEXTE DE BUILD = racine du repo
# (nécessaire pour Directory.Packages.props / Directory.Build.props — gestion centralisée des versions).
#   docker build -f src/Atlas.Api/Dockerfile -t atlas-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Atlas.Api/Atlas.Api.csproj
RUN dotnet publish src/Atlas.Api/Atlas.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# Utilisateur non-root fourni par l'image (.NET 8+).
USER $APP_UID
# Les images aspnet .NET 8+ écoutent par défaut sur le port 8080 (ASPNETCORE_HTTP_PORTS=8080).
EXPOSE 8080
ENTRYPOINT ["dotnet", "Atlas.Api.dll"]
