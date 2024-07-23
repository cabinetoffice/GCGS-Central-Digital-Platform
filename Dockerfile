ARG ASPNET_VERSION=8.0
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
COPY --link Libraries/CO.CDP.AwsServices/CO.CDP.AwsServices.csproj Libraries/CO.CDP.AwsServices/
COPY --link Libraries/CO.CDP.AwsServices.Tests/CO.CDP.AwsServices.Tests.csproj Libraries/CO.CDP.AwsServices.Tests/
COPY --link Libraries/CO.CDP.Authentication/CO.CDP.Authentication.csproj Libraries/CO.CDP.Authentication/
COPY --link Libraries/CO.CDP.Authentication.Tests/CO.CDP.Authentication.Tests.csproj Libraries/CO.CDP.Authentication.Tests/
COPY --link Libraries/CO.CDP.Configuration/CO.CDP.Configuration.csproj Libraries/CO.CDP.Configuration/
COPY --link Libraries/CO.CDP.Configuration.Tests/CO.CDP.Configuration.Tests.csproj Libraries/CO.CDP.Configuration.Tests/
COPY --link Libraries/CO.CDP.EntityFrameworkCore/CO.CDP.EntityFrameworkCore.csproj Libraries/CO.CDP.EntityFrameworkCore/
COPY --link Libraries/CO.CDP.EntityFrameworkCore.Tests/CO.CDP.EntityFrameworkCore.Tests.csproj Libraries/CO.CDP.EntityFrameworkCore.Tests/
COPY --link Libraries/CO.CDP.Functional/CO.CDP.Functional.csproj Libraries/CO.CDP.Functional/
COPY --link Libraries/CO.CDP.Functional.Tests/CO.CDP.Functional.Tests.csproj Libraries/CO.CDP.Functional.Tests/
COPY --link Libraries/CO.CDP.MQ/CO.CDP.MQ.csproj Libraries/CO.CDP.MQ/
COPY --link Libraries/CO.CDP.MQ.Tests/CO.CDP.MQ.Tests.csproj Libraries/CO.CDP.MQ.Tests/
COPY --link Libraries/CO.CDP.Mvc.Validation/CO.CDP.Mvc.Validation.csproj Libraries/CO.CDP.Mvc.Validation/
COPY --link Libraries/CO.CDP.Mvc.Validation.Tests/CO.CDP.Mvc.Validation.Tests.csproj Libraries/CO.CDP.Mvc.Validation.Tests/
COPY --link Libraries/CO.CDP.Swashbuckle/CO.CDP.Swashbuckle.csproj Libraries/CO.CDP.Swashbuckle/
COPY --link Libraries/CO.CDP.Swashbuckle.Tests/CO.CDP.Swashbuckle.Tests.csproj Libraries/CO.CDP.Swashbuckle.Tests/
COPY --link Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj Frontend/CO.CDP.OrganisationApp/
COPY --link Frontend/CO.CDP.OrganisationApp.Tests/CO.CDP.OrganisationApp.Tests.csproj Frontend/CO.CDP.OrganisationApp.Tests/
COPY --link Libraries/CO.CDP.Tenant.WebApiClient/CO.CDP.Tenant.WebApiClient.csproj Libraries/CO.CDP.Tenant.WebApiClient/
COPY --link Libraries/CO.CDP.Tenant.WebApiClient.Tests/CO.CDP.Tenant.WebApiClient.Tests.csproj Libraries/CO.CDP.Tenant.WebApiClient.Tests/
COPY --link Libraries/CO.CDP.Organisation.WebApiClient/CO.CDP.Organisation.WebApiClient.csproj Libraries/CO.CDP.Organisation.WebApiClient/
COPY --link Libraries/CO.CDP.Organisation.WebApiClient.Tests/CO.CDP.Organisation.WebApiClient.Tests.csproj Libraries/CO.CDP.Organisation.WebApiClient.Tests/
COPY --link Libraries/CO.CDP.Person.WebApiClient/CO.CDP.Person.WebApiClient.csproj Libraries/CO.CDP.Person.WebApiClient/
COPY --link Libraries/CO.CDP.Person.WebApiClient.Tests/CO.CDP.Person.WebApiClient.Tests.csproj Libraries/CO.CDP.Person.WebApiClient.Tests/
COPY --link Libraries/CO.CDP.Forms.WebApiClient/CO.CDP.Forms.WebApiClient.csproj Libraries/CO.CDP.Forms.WebApiClient/
COPY --link Libraries/CO.CDP.Forms.WebApiClient.Tests/CO.CDP.Forms.WebApiClient.Tests.csproj Libraries/CO.CDP.Forms.WebApiClient.Tests/
COPY --link TestKit/CO.CDP.TestKit.Mvc/CO.CDP.TestKit.Mvc.csproj TestKit/CO.CDP.TestKit.Mvc/
COPY --link TestKit/CO.CDP.TestKit.Mvc.Tests/CO.CDP.TestKit.Mvc.Tests.csproj TestKit/CO.CDP.TestKit.Mvc.Tests/
COPY --link TestKit/CO.CDP.Testcontainers.PostgreSql/CO.CDP.Testcontainers.PostgreSql.csproj TestKit/CO.CDP.Testcontainers.PostgreSql/
COPY --link TestKit/CO.CDP.Testcontainers.PostgreSql.Tests/CO.CDP.Testcontainers.PostgreSql.Tests.csproj TestKit/CO.CDP.Testcontainers.PostgreSql.Tests/
COPY --link Services/CO.CDP.OrganisationInformation/CO.CDP.OrganisationInformation.csproj Services/CO.CDP.OrganisationInformation/
COPY --link Services/CO.CDP.OrganisationInformation.Persistence/CO.CDP.OrganisationInformation.Persistence.csproj Services/CO.CDP.OrganisationInformation.Persistence/
COPY --link Services/CO.CDP.OrganisationInformation.Persistence.Tests/CO.CDP.OrganisationInformation.Persistence.Tests.csproj Services/CO.CDP.OrganisationInformation.Persistence.Tests/
COPY --link Services/CO.CDP.Tenant.WebApi/CO.CDP.Tenant.WebApi.csproj Services/CO.CDP.Tenant.WebApi/
COPY --link Services/CO.CDP.Tenant.WebApi.Tests/CO.CDP.Tenant.WebApi.Tests.csproj Services/CO.CDP.Tenant.WebApi.Tests/
COPY --link Services/CO.CDP.DataSharing.WebApi/CO.CDP.DataSharing.WebApi.csproj Services/CO.CDP.DataSharing.WebApi/
COPY --link Services/CO.CDP.DataSharing.WebApi.Tests/CO.CDP.DataSharing.WebApi.Tests.csproj Services/CO.CDP.DataSharing.WebApi.Tests/
COPY --link Services/CO.CDP.Organisation.WebApi.Tests/CO.CDP.Organisation.WebApi.Tests.csproj Services/CO.CDP.Organisation.WebApi.Tests/
COPY --link Services/CO.CDP.Organisation.WebApi/CO.CDP.Organisation.WebApi.csproj Services/CO.CDP.Organisation.WebApi/
COPY --link Services/CO.CDP.Person.WebApi/CO.CDP.Person.WebApi.csproj Services/CO.CDP.Person.WebApi/
COPY --link Services/CO.CDP.Person.WebApi.Tests/CO.CDP.Person.WebApi.Tests.csproj Services/CO.CDP.Person.WebApi.Tests/
COPY --link Services/CO.CDP.Forms.WebApi/CO.CDP.Forms.WebApi.csproj Services/CO.CDP.Forms.WebApi/
COPY --link Services/CO.CDP.Forms.WebApi.Tests/CO.CDP.Forms.WebApi.Tests.csproj Services/CO.CDP.Forms.WebApi.Tests/
COPY --link Services/CO.CDP.Organisation.Authority/CO.CDP.Organisation.Authority.csproj Services/CO.CDP.Organisation.Authority/
COPY --link Services/CO.CDP.Organisation.Authority.Tests/CO.CDP.Organisation.Authority.Tests.csproj Services/CO.CDP.Organisation.Authority.Tests/
COPY --link Services/CO.CDP.EntityVerification/CO.CDP.EntityVerification.csproj Services/CO.CDP.EntityVerification/
COPY --link Services/CO.CDP.EntityVerification.Tests/CO.CDP.EntityVerification.Tests.csproj Services/CO.CDP.EntityVerification.Tests/
COPY --link GCGS-Central-Digital-Platform.sln .
RUN dotnet restore "GCGS-Central-Digital-Platform.sln"

FROM solution-dependencies AS source
COPY --link TestKit TestKit
COPY --link Libraries Libraries
COPY --link Services Services
COPY --link Frontend Frontend

FROM source AS build
ARG BUILD_CONFIGURATION
WORKDIR /src
RUN dotnet build -c $BUILD_CONFIGURATION

FROM build AS build-authority
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.Organisation.Authority
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

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

FROM build AS build-entity-verification
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.EntityVerification
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-organisation-app
ARG BUILD_CONFIGURATION
WORKDIR /src/Frontend/CO.CDP.OrganisationApp
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build-authority AS publish-authority
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.Organisation.Authority.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

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

FROM build-entity-verification AS publish-entity-verification
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.EntityVerification.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-organisation-app AS publish-organisation-app
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.OrganisationApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-tenant AS build-migrations-organisation-information
WORKDIR /src
COPY .config/dotnet-tools.json .config/
RUN dotnet tool restore
RUN dotnet ef migrations bundle -p /src/Services/CO.CDP.OrganisationInformation.Persistence -s /src/Services/CO.CDP.Tenant.WebApi --self-contained -o /app/migrations/efbundle

FROM build-entity-verification AS build-migrations-entity-verification
WORKDIR /src
COPY .config/dotnet-tools.json .config/
RUN dotnet tool restore
RUN dotnet ef migrations bundle -p /src/Services/CO.CDP.EntityVerification --self-contained -o /app/migrations/efbundle

FROM base AS migrations-organisation-information
ENV MIGRATIONS_CONNECTION_STRING=""
WORKDIR /app
COPY --from=build-migrations-organisation-information /app/migrations/efbundle .
ENTRYPOINT /app/efbundle --connection "$MIGRATIONS_CONNECTION_STRING"

FROM base AS migrations-entity-verification
ENV MIGRATIONS_CONNECTION_STRING=""
WORKDIR /app
COPY --from=build-migrations-entity-verification /app/migrations/efbundle .
ENTRYPOINT /app/efbundle --connection "$MIGRATIONS_CONNECTION_STRING"

FROM base AS final-authority
WORKDIR /app
COPY --from=publish-authority /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Organisation.Authority.dll"]

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
COPY --from=publish-forms /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Forms.WebApi.dll"]

FROM base AS final-data-sharing
WORKDIR /app
COPY --from=publish-data-sharing /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.DataSharing.WebApi.dll"]

FROM base AS final-entity-verification
WORKDIR /app
COPY --from=publish-entity-verification /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.EntityVerification.dll"]

FROM base AS final-organisation-app
WORKDIR /app
COPY --from=publish-organisation-app /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.OrganisationApp.dll"]
