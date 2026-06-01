# syntax=docker/dockerfile:1
# Image de l'hôte web Atlas (Blazor Web App, .NET 10). Publier l'hôte bundle aussi les fichiers
# statiques du client WASM (Atlas.Web.Client est référencé). CONTEXTE DE BUILD = racine du repo.
#   docker build -f src/Atlas.Web/Dockerfile -t atlas-web .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Atlas.Web/Atlas.Web.csproj
RUN dotnet publish src/Atlas.Web/Atlas.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Atlas.Web.dll"]
