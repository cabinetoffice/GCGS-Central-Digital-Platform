FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Libraries/CO.CDP.Tenant.WebApiClient/CO.CDP.Tenant.WebApiClient.csproj", "Libraries/CO.CDP.Tenant.WebApiClient/"]
COPY ["Services/CO.CDP.Tenant.WebApi/CO.CDP.Tenant.WebApi.csproj", "Services/CO.CDP.Tenant.WebApi/"]
COPY ["Services/CO.CDP.Tenant.Persistence/CO.CDP.Tenant.Persistence.csproj", "Services/CO.CDP.Tenant.Persistence/"]
RUN dotnet restore "Libraries/CO.CDP.Tenant.WebApiClient/CO.CDP.Tenant.WebApiClient.csproj"
COPY ["Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj", "CO.CDP.OrganisationApp/"]
RUN dotnet restore "CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj"

COPY Libraries/ /src/Libraries
COPY Services/ /src/Services
COPY Frontend/ /src/Frontend

WORKDIR "/src/Services/CO.CDP.Tenant.WebApi"
RUN dotnet build "CO.CDP.Tenant.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR "/src/Libraries/CO.CDP.Tenant.WebApiClient"
RUN dotnet build "CO.CDP.Tenant.WebApiClient.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR "/src/Frontend/CO.CDP.OrganisationApp"
RUN dotnet build "CO.CDP.OrganisationApp.csproj" -c $BUILD_CONFIGURATION -o /app/build
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CO.CDP.OrganisationApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.OrganisationApp.dll"]
