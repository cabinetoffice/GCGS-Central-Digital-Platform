# syntax=docker/dockerfile:1.7

ARG ASPNET_VERSION=8.0
ARG BUILD_CONFIGURATION=Release

# Distroless image used for apps has no package manager so we install these packages here
FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS packages
RUN apt-get update && apt-get install -y --no-install-recommends \
    netcat-openbsd \
    fontconfig \
    fonts-dejavu-core \
    && fc-cache -fv \
    && rm -rf /var/lib/apt/lists/*
RUN set -eux; \
    arch="$(uname -m)"; \
    case "$arch" in \
        x86_64) libarch="x86_64-linux-gnu" ;; \
        aarch64|arm64) libarch="aarch64-linux-gnu" ;; \
        *) echo "Unsupported architecture: $arch"; exit 1 ;; \
    esac; \
    libdir="/lib/${libarch}"; \
    if [ ! -e "${libdir}/libbsd.so.0" ]; then libdir="/usr/lib/${libarch}"; fi; \
    mkdir -p "/opt/extra-libs/usr/lib/${libarch}"; \
    cp "${libdir}/libbsd.so.0" "${libdir}/libmd.so.0" "/opt/extra-libs/usr/lib/${libarch}/"

# Distroless "chiseled" image from MS with absolutely minimal packages, no shell, no package manager
# Packages we require can be copied from the packages image above
FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION}-noble-chiseled-extra AS base
COPY --from=packages /usr/bin/nc /usr/bin/nc
COPY --from=packages /opt/extra-libs/usr/lib/ /usr/lib/
COPY --from=packages /usr/bin/fc-cache /usr/bin/fc-cache
COPY --from=packages /usr/share/fonts/truetype/dejavu /usr/share/fonts/truetype/dejavu
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:${ASPNET_VERSION} AS solution-dependencies
WORKDIR /src
COPY --link Libraries/CO.CDP.WebApi.Foundation/CO.CDP.WebApi.Foundation.csproj Libraries/CO.CDP.WebApi.Foundation/
COPY --link Libraries/CO.CDP.GovUKNotify/CO.CDP.GovUKNotify.csproj Libraries/CO.CDP.GovUKNotify/
COPY --link Libraries/CO.CDP.AwsServices/CO.CDP.AwsServices.csproj Libraries/CO.CDP.AwsServices/
COPY --link Libraries/CO.CDP.Authentication/CO.CDP.Authentication.csproj Libraries/CO.CDP.Authentication/
COPY --link Libraries/CO.CDP.Configuration/CO.CDP.Configuration.csproj Libraries/CO.CDP.Configuration/
COPY --link Libraries/CO.CDP.EntityFrameworkCore/CO.CDP.EntityFrameworkCore.csproj Libraries/CO.CDP.EntityFrameworkCore/
COPY --link Libraries/CO.CDP.Functional/CO.CDP.Functional.csproj Libraries/CO.CDP.Functional/
COPY --link Libraries/CO.CDP.MQ/CO.CDP.MQ.csproj Libraries/CO.CDP.MQ/
COPY --link Libraries/CO.CDP.Mvc.Validation/CO.CDP.Mvc.Validation.csproj Libraries/CO.CDP.Mvc.Validation/
COPY --link Libraries/CO.CDP.Swashbuckle/CO.CDP.Swashbuckle.csproj Libraries/CO.CDP.Swashbuckle/
COPY --link Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj Frontend/CO.CDP.OrganisationApp/
COPY --link Frontend/CO.CDP.RegisterOfCommercialTools.App/CO.CDP.RegisterOfCommercialTools.App.csproj Frontend/CO.CDP.RegisterOfCommercialTools.App/
COPY --link Libraries/CO.CDP.Tenant.WebApiClient/CO.CDP.Tenant.WebApiClient.csproj Libraries/CO.CDP.Tenant.WebApiClient/
COPY --link Libraries/CO.CDP.Organisation.WebApiClient/CO.CDP.Organisation.WebApiClient.csproj Libraries/CO.CDP.Organisation.WebApiClient/
COPY --link Libraries/CO.CDP.Person.WebApiClient/CO.CDP.Person.WebApiClient.csproj Libraries/CO.CDP.Person.WebApiClient/
COPY --link Libraries/CO.CDP.Forms.WebApiClient/CO.CDP.Forms.WebApiClient.csproj Libraries/CO.CDP.Forms.WebApiClient/
COPY --link Libraries/CO.CDP.DataSharing.WebApiClient/CO.CDP.DataSharing.WebApiClient.csproj Libraries/CO.CDP.DataSharing.WebApiClient/
COPY --link Libraries/CO.CDP.EntityVerificationClient/CO.CDP.EntityVerificationClient.csproj Libraries/CO.CDP.EntityVerificationClient/
COPY --link Libraries/CO.CDP.RegisterOfCommercialTools.WebApiClient/CO.CDP.RegisterOfCommercialTools.WebApiClient.csproj Libraries/CO.CDP.RegisterOfCommercialTools.WebApiClient/
COPY --link TestKit/CO.CDP.TestKit.Mvc/CO.CDP.TestKit.Mvc.csproj TestKit/CO.CDP.TestKit.Mvc/
COPY --link TestKit/CO.CDP.Testcontainers.PostgreSql/CO.CDP.Testcontainers.PostgreSql.csproj TestKit/CO.CDP.Testcontainers.PostgreSql/
COPY --link Services/CO.CDP.OrganisationInformation/CO.CDP.OrganisationInformation.csproj Services/CO.CDP.OrganisationInformation/
COPY --link Services/CO.CDP.OrganisationInformation.Persistence/CO.CDP.OrganisationInformation.Persistence.csproj Services/CO.CDP.OrganisationInformation.Persistence/
COPY --link Services/CO.CDP.ApplicationRegistry.Persistence/CO.CDP.ApplicationRegistry.Persistence.csproj Services/CO.CDP.ApplicationRegistry.Persistence/
COPY --link Services/CO.CDP.Tenant.WebApi/CO.CDP.Tenant.WebApi.csproj Services/CO.CDP.Tenant.WebApi/
COPY --link Services/CO.CDP.DataSharing.WebApi/CO.CDP.DataSharing.WebApi.csproj Services/CO.CDP.DataSharing.WebApi/
COPY --link Services/CO.CDP.Organisation.WebApi/CO.CDP.Organisation.WebApi.csproj Services/CO.CDP.Organisation.WebApi/
COPY --link Services/CO.CDP.Person.WebApi/CO.CDP.Person.WebApi.csproj Services/CO.CDP.Person.WebApi/
COPY --link Services/CO.CDP.Forms.WebApi/CO.CDP.Forms.WebApi.csproj Services/CO.CDP.Forms.WebApi/
COPY --link Services/CO.CDP.Organisation.Authority/CO.CDP.Organisation.Authority.csproj Services/CO.CDP.Organisation.Authority/
COPY --link Services/CO.CDP.EntityVerification.Persistence/CO.CDP.EntityVerification.Persistence.csproj Services/CO.CDP.EntityVerification.Persistence/
COPY --link Services/CO.CDP.EntityVerification/CO.CDP.EntityVerification.csproj Services/CO.CDP.EntityVerification/
COPY --link Services/CO.CDP.Localization/CO.CDP.Localization.csproj Services/CO.CDP.Localization/
COPY --link Services/CO.CDP.AntiVirusScanner/CO.CDP.AntiVirusScanner.csproj Services/CO.CDP.AntiVirusScanner/
COPY --link Services/CO.CDP.OutboxProcessor/CO.CDP.OutboxProcessor.csproj Services/CO.CDP.OutboxProcessor/
COPY --link Services/CO.CDP.ScheduledWorker/CO.CDP.ScheduledWorker.csproj Services/CO.CDP.ScheduledWorker/
COPY --link Libraries/CO.CDP.UI.Foundation/CO.CDP.UI.Foundation.csproj Libraries/CO.CDP.UI.Foundation/
COPY --link Libraries/CO.CDP.Logging/CO.CDP.Logging.csproj Libraries/CO.CDP.Logging/
COPY --link Services/CO.CDP.RegisterOfCommercialTools.WebApi/CO.CDP.RegisterOfCommercialTools.WebApi.csproj Services/CO.CDP.RegisterOfCommercialTools.WebApi/
COPY --link Services/CO.CDP.RegisterOfCommercialTools.Persistence/CO.CDP.RegisterOfCommercialTools.Persistence.csproj Services/CO.CDP.RegisterOfCommercialTools.Persistence/

COPY --link GCGS-Central-Digital-Platform.sln .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "Services/CO.CDP.Organisation.Authority/CO.CDP.Organisation.Authority.csproj" && \
    dotnet restore "Services/CO.CDP.Tenant.WebApi/CO.CDP.Tenant.WebApi.csproj" && \
    dotnet restore "Services/CO.CDP.Organisation.WebApi/CO.CDP.Organisation.WebApi.csproj" && \
    dotnet restore "Services/CO.CDP.Person.WebApi/CO.CDP.Person.WebApi.csproj" && \
    dotnet restore "Services/CO.CDP.Forms.WebApi/CO.CDP.Forms.WebApi.csproj" && \
    dotnet restore "Services/CO.CDP.DataSharing.WebApi/CO.CDP.DataSharing.WebApi.csproj" && \
    dotnet restore "Services/CO.CDP.EntityVerification/CO.CDP.EntityVerification.csproj" && \
    dotnet restore "Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj" && \
    dotnet restore "Frontend/CO.CDP.RegisterOfCommercialTools.App/CO.CDP.RegisterOfCommercialTools.App.csproj" && \
    dotnet restore "Services/CO.CDP.RegisterOfCommercialTools.WebApi/CO.CDP.RegisterOfCommercialTools.WebApi.csproj" && \
    dotnet restore "Services/CO.CDP.AntiVirusScanner/CO.CDP.AntiVirusScanner.csproj" && \
    dotnet restore "Services/CO.CDP.OutboxProcessor/CO.CDP.OutboxProcessor.csproj" && \
    dotnet restore "Services/CO.CDP.ScheduledWorker/CO.CDP.ScheduledWorker.csproj" && \
    dotnet restore "Services/CO.CDP.OrganisationInformation.Persistence/CO.CDP.OrganisationInformation.Persistence.csproj" && \
    dotnet restore "Services/CO.CDP.EntityVerification.Persistence/CO.CDP.EntityVerification.Persistence.csproj"

FROM solution-dependencies AS source
COPY --link TestKit TestKit
COPY --link Libraries Libraries
COPY --link Services Services
COPY --link Frontend Frontend

FROM source AS publish
ARG BUILD_CONFIGURATION
WORKDIR /src
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "Services/CO.CDP.Organisation.Authority/CO.CDP.Organisation.Authority.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/authority /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.Tenant.WebApi/CO.CDP.Tenant.WebApi.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/tenant /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.Organisation.WebApi/CO.CDP.Organisation.WebApi.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/organisation /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.Person.WebApi/CO.CDP.Person.WebApi.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/person /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.Forms.WebApi/CO.CDP.Forms.WebApi.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/forms /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.DataSharing.WebApi/CO.CDP.DataSharing.WebApi.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/data-sharing /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.EntityVerification/CO.CDP.EntityVerification.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/entity-verification /p:UseAppHost=false && \
    dotnet publish "Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/organisation-app /p:UseAppHost=false && \
    dotnet publish "Frontend/CO.CDP.RegisterOfCommercialTools.App/CO.CDP.RegisterOfCommercialTools.App.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/commercial-tools-app /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.RegisterOfCommercialTools.WebApi/CO.CDP.RegisterOfCommercialTools.WebApi.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/commercial-tools-api /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.AntiVirusScanner/CO.CDP.AntiVirusScanner.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/antivirus-app /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.OutboxProcessor/CO.CDP.OutboxProcessor.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/outbox-processor /p:UseAppHost=false && \
    dotnet publish "Services/CO.CDP.ScheduledWorker/CO.CDP.ScheduledWorker.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish/scheduled-worker /p:UseAppHost=false

FROM source AS build-migrations-organisation-information
WORKDIR /src
COPY .config/dotnet-tools.json .config/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet tool restore && \
    dotnet ef migrations bundle -p /src/Services/CO.CDP.OrganisationInformation.Persistence -s /src/Services/CO.CDP.OrganisationInformation.Persistence --self-contained -o /app/migrations/efbundle

FROM source AS build-migrations-entity-verification
WORKDIR /src
COPY .config/dotnet-tools.json .config/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet tool restore && \
    dotnet ef migrations bundle -p /src/Services/CO.CDP.EntityVerification.Persistence -s /src/Services/CO.CDP.EntityVerification.Persistence --self-contained -o /app/migrations/efbundle


FROM base AS migrations-organisation-information
COPY --from=busybox:uclibc /bin/busybox /bin/busybox
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=build-migrations-organisation-information /src/Services/CO.CDP.OrganisationInformation.Persistence/OrganisationInformationDatabaseMigrationConfig /app/OrganisationInformationDatabaseMigrationConfig
COPY --from=build-migrations-organisation-information /src/Services/CO.CDP.OrganisationInformation.Persistence/StoredProcedures /app/StoredProcedures
COPY --from=build-migrations-organisation-information /app/migrations/efbundle .
ENTRYPOINT ["/bin/busybox", "sh", "-c", "/app/efbundle --connection \"Host=$OrganisationInformationDatabase__Host;Database=$OrganisationInformationDatabase__Database;Username=$OrganisationInformationDatabase__Username;Password=$OrganisationInformationDatabase__Password;\""]

FROM base AS migrations-entity-verification
COPY --from=busybox:uclibc /bin/busybox /bin/busybox
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=build-migrations-entity-verification /src/Services/CO.CDP.EntityVerification.Persistence/EntityVerificationDatabaseMigrationConfig /app/EntityVerificationDatabaseMigrationConfig
COPY --from=build-migrations-entity-verification /app/migrations/efbundle .
ENTRYPOINT ["/bin/busybox", "sh", "-c", "/app/efbundle --connection \"Host=$EntityVerificationDatabase__Host;Database=$EntityVerificationDatabase__Database;Username=$EntityVerificationDatabase__Username;Password=$EntityVerificationDatabase__Password;\""]


FROM base AS final-authority
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/authority .
ENTRYPOINT ["dotnet", "CO.CDP.Organisation.Authority.dll"]

FROM base AS final-tenant
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/tenant .
ENTRYPOINT ["dotnet", "CO.CDP.Tenant.WebApi.dll"]

FROM base AS final-organisation
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/organisation .
ENTRYPOINT ["dotnet", "CO.CDP.Organisation.WebApi.dll"]

FROM base AS final-person
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/person .
ENTRYPOINT ["dotnet", "CO.CDP.Person.WebApi.dll"]

FROM base AS final-forms
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/forms .
ENTRYPOINT ["dotnet", "CO.CDP.Forms.WebApi.dll"]

FROM base AS final-data-sharing
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/data-sharing .
ENTRYPOINT ["dotnet", "CO.CDP.DataSharing.WebApi.dll"]

FROM base AS final-entity-verification
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/entity-verification .
ENTRYPOINT ["dotnet", "CO.CDP.EntityVerification.dll"]

FROM base AS final-organisation-app
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/organisation-app .
ENTRYPOINT ["dotnet", "CO.CDP.OrganisationApp.dll"]

FROM base AS final-commercial-tools-app
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/commercial-tools-app .
ENTRYPOINT ["dotnet", "CO.CDP.RegisterOfCommercialTools.App.dll"]

FROM base AS final-commercial-tools-api
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/commercial-tools-api .
ENTRYPOINT ["dotnet", "CO.CDP.RegisterOfCommercialTools.WebApi.dll"]

FROM base AS final-antivirus-app
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/antivirus-app .
ENTRYPOINT ["dotnet", "CO.CDP.AntiVirusScanner.dll"]

FROM base AS final-outbox-processor-organisation
ARG VERSION
ENV VERSION=${VERSION}
ENV DbContext=OrganisationInformationContext
ENV Channel=organisation_information_outbox
WORKDIR /app
COPY --from=publish /app/publish/outbox-processor .
ENTRYPOINT ["dotnet", "CO.CDP.OutboxProcessor.dll"]

FROM base AS final-outbox-processor-entity-verification
ARG VERSION
ENV VERSION=${VERSION}
ENV DbContext=EntityVerificationContext
ENV Channel=entity_verification_outbox
WORKDIR /app
COPY --from=publish /app/publish/outbox-processor .
ENTRYPOINT ["dotnet", "CO.CDP.OutboxProcessor.dll"]

FROM base AS final-scheduled-worker
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish /app/publish/scheduled-worker .
ENTRYPOINT ["dotnet", "CO.CDP.ScheduledWorker.dll"]
