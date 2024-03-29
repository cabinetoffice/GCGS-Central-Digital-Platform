﻿ARG ASPNET_VERSION=8.0
ARG BUILD_CONFIGURATION=Release
ARG NUGET_PACKAGES=/nuget/packages

FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS base
ARG NUGET_PACKAGES
ENV NUGET_PACKAGES="${NUGET_PACKAGES}"
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:${ASPNET_VERSION} AS solution-dependencies
ARG NUGET_PACKAGES
ENV NUGET_PACKAGES="${NUGET_PACKAGES}"
WORKDIR /src
COPY Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj Frontend/CO.CDP.OrganisationApp/
COPY Frontend/CO.CDP.OrganisationApp.Tests/CO.CDP.OrganisationApp.Tests.csproj Frontend/CO.CDP.OrganisationApp.Tests/
COPY Libraries/CO.CDP.Tenant.WebApiClient/CO.CDP.Tenant.WebApiClient.csproj Libraries/CO.CDP.Tenant.WebApiClient/
COPY Libraries/CO.CDP.Tenant.WebApiClient.Tests/CO.CDP.Tenant.WebApiClient.Tests.csproj Libraries/CO.CDP.Tenant.WebApiClient.Tests/
COPY TestKit/CO.CDP.Testcontainers.PostgreSql/CO.CDP.Testcontainers.PostgreSql.csproj TestKit/CO.CDP.Testcontainers.PostgreSql/
COPY TestKit/CO.CDP.Testcontainers.PostgreSql.Tests/CO.CDP.Testcontainers.PostgreSql.Tests.csproj TestKit/CO.CDP.Testcontainers.PostgreSql.Tests/
COPY Services/CO.CDP.Tenant.Persistence/CO.CDP.Tenant.Persistence.csproj Services/CO.CDP.Tenant.Persistence/
COPY Services/CO.CDP.Tenant.Persistence.Tests/CO.CDP.Tenant.Persistence.Tests.csproj Services/CO.CDP.Tenant.Persistence.Tests/
COPY Services/CO.CDP.Tenant.WebApi/CO.CDP.Tenant.WebApi.csproj Services/CO.CDP.Tenant.WebApi/
COPY Services/CO.CDP.Tenant.WebApi.Tests/CO.CDP.Tenant.WebApi.Tests.csproj Services/CO.CDP.Tenant.WebApi.Tests/
COPY Services/CO.CDP.DataSharing.WebApi/CO.CDP.DataSharing.WebApi.csproj Services/CO.CDP.DataSharing.WebApi/
COPY Services/CO.CDP.DataSharing.WebApi.Tests/CO.CDP.DataSharing.WebApi.Tests.csproj Services/CO.CDP.DataSharing.WebApi.Tests/
COPY Services/CO.CDP.Organisation.WebApi.Tests/CO.CDP.Organisation.WebApi.Tests.csproj Services/CO.CDP.Organisation.WebApi.Tests/
COPY Services/CO.CDP.Organisation.WebApi/CO.CDP.Organisation.WebApi.csproj Services/CO.CDP.Organisation.WebApi/
COPY Services/CO.CDP.Person.WebApi/CO.CDP.Person.WebApi.csproj Services/CO.CDP.Person.WebApi/
COPY Services/CO.CDP.Person.WebApi.Tests/CO.CDP.Person.WebApi.Tests.csproj Services/CO.CDP.Person.WebApi.Tests/
COPY Services/CO.CDP.Forms.WebApi/CO.CDP.Forms.WebApi.csproj Services/CO.CDP.Forms.WebApi/
COPY Services/CO.CDP.Forms.WebApi.Tests/CO.CDP.Forms.WebApi.Tests.csproj Services/CO.CDP.Forms.WebApi.Tests/
COPY GCGS-Central-Digital-Platform.sln .
RUN dotnet restore "GCGS-Central-Digital-Platform.sln"

FROM solution-dependencies AS source
COPY TestKit TestKit
COPY Libraries Libraries
COPY Services Services
COPY Frontend Frontend

FROM source AS build
ARG BUILD_CONFIGURATION
WORKDIR /src
RUN dotnet build -c $BUILD_CONFIGURATION

FROM build AS build-tenant
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.Tenant.WebApi
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-organisation
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.Organisation.WebApi
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-person
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.Person.WebApi
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-forms
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.Forms.WebApi
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-data-sharing
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.DataSharing.WebApi
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-organisation-app
ARG BUILD_CONFIGURATION
WORKDIR /src/Frontend/CO.CDP.OrganisationApp
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build-tenant AS publish-tenant
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.Tenant.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-organisation AS publish-organisation
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.Organisation.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-person AS publish-person
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.Person.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-forms AS publish-forms
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.Forms.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-data-sharing AS publish-data-sharing
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.DataSharing.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-organisation-app AS publish-organisation-app
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.OrganisationApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final-tenant
WORKDIR /app
COPY --from=publish-tenant /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Tenant.WebApi.dll"]

FROM base AS final-organisation
WORKDIR /app
COPY --from=publish-organisation /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Organisation.WebApi.dll"]

FROM base AS final-person
WORKDIR /app
COPY --from=publish-person /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Person.WebApi.dll"]

FROM base AS final-forms
WORKDIR /app
COPY --from=publish-person /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Forms.WebApi.dll"]

FROM base AS final-data-sharing
WORKDIR /app
COPY --from=publish-data-sharing /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.DataSharing.WebApi.dll"]

FROM base AS final-organisation-app
WORKDIR /app
COPY --from=publish-organisation-app /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.OrganisationApp.dll"]
