FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj", "CO.CDP.OrganisationApp/"]
RUN dotnet restore "CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj"

WORKDIR "/src/Frontend/CO.CDP.OrganisationApp"
COPY Frontend/ /src/Frontend
COPY Libraries/ /src/Libraries
COPY Services/ /src/Services
RUN dotnet build "CO.CDP.OrganisationApp.csproj" -c $BUILD_CONFIGURATION -o /app/build
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CO.CDP.OrganisationApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.OrganisationApp.dll"]
