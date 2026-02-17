ARG ASPNET_VERSION=8.0
ARG BUILD_CONFIGURATION=Release
ARG NUGET_PACKAGES=/nuget/packages

# Distroless image used for apps has no package manager so we install these packages here
FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS packages
RUN apt-get update && apt-get install -y --no-install-recommends \
    netcat-openbsd \
    fontconfig \
    fonts-dejavu-core \
    && fc-cache -fv \
    && rm -rf /var/lib/apt/lists/*

# Distroless "chiseled" image from MS with absolutely minimal packages, no shell, no package manager
# Packages we require can be copied from the packages image above
FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION}-noble-chiseled-extra AS base
ARG NUGET_PACKAGES
ENV NUGET_PACKAGES="${NUGET_PACKAGES}"
COPY --from=packages /usr/bin/nc /usr/bin/nc
COPY --from=packages /lib/x86_64-linux-gnu/libbsd.so.0 /lib/x86_64-linux-gnu/libbsd.so.0
COPY --from=packages /lib/x86_64-linux-gnu/libmd.so.0 /lib/x86_64-linux-gnu/libmd.so.0
COPY --from=packages /usr/bin/fc-cache /usr/bin/fc-cache
COPY --from=packages /usr/share/fonts/truetype/dejavu /usr/share/fonts/truetype/dejavu
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:${ASPNET_VERSION} AS solution-dependencies
ARG NUGET_PACKAGES
ENV NUGET_PACKAGES="${NUGET_PACKAGES}"
WORKDIR /src
COPY --link Libraries/CO.CDP.WebApi.Foundation/CO.CDP.WebApi.Foundation.csproj Libraries/CO.CDP.WebApi.Foundation/
COPY --link Libraries/CO.CDP.WebApi.Foundation.Tests/CO.CDP.WebApi.Foundation.Tests.csproj Libraries/CO.CDP.WebApi.Foundation.Tests/
COPY --link Libraries/CO.CDP.GovUKNotify/CO.CDP.GovUKNotify.csproj Libraries/CO.CDP.GovUKNotify/
COPY --link Libraries/CO.CDP.GovUKNotify.Test/CO.CDP.GovUKNotify.Test.csproj Libraries/CO.CDP.GovUKNotify.Test/
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
COPY --link Frontend/CO.CDP.UserManagement.App/CO.CDP.UserManagement.App.csproj Frontend/CO.CDP.UserManagement.App/
COPY --link Frontend/CO.CDP.RegisterOfCommercialTools.App/CO.CDP.RegisterOfCommercialTools.App.csproj Frontend/CO.CDP.RegisterOfCommercialTools.App/
COPY --link Frontend/CO.CDP.RegisterOfCommercialTools.App.Tests/CO.CDP.RegisterOfCommercialTools.App.Tests.csproj Frontend/CO.CDP.RegisterOfCommercialTools.App.Tests/
COPY --link Libraries/CO.CDP.Tenant.WebApiClient/CO.CDP.Tenant.WebApiClient.csproj Libraries/CO.CDP.Tenant.WebApiClient/
COPY --link Libraries/CO.CDP.Tenant.WebApiClient.Tests/CO.CDP.Tenant.WebApiClient.Tests.csproj Libraries/CO.CDP.Tenant.WebApiClient.Tests/
COPY --link Libraries/CO.CDP.Organisation.WebApiClient/CO.CDP.Organisation.WebApiClient.csproj Libraries/CO.CDP.Organisation.WebApiClient/
COPY --link Libraries/CO.CDP.Organisation.WebApiClient.Tests/CO.CDP.Organisation.WebApiClient.Tests.csproj Libraries/CO.CDP.Organisation.WebApiClient.Tests/
COPY --link Libraries/CO.CDP.Person.WebApiClient/CO.CDP.Person.WebApiClient.csproj Libraries/CO.CDP.Person.WebApiClient/
COPY --link Libraries/CO.CDP.Person.WebApiClient.Tests/CO.CDP.Person.WebApiClient.Tests.csproj Libraries/CO.CDP.Person.WebApiClient.Tests/
COPY --link Libraries/CO.CDP.Forms.WebApiClient/CO.CDP.Forms.WebApiClient.csproj Libraries/CO.CDP.Forms.WebApiClient/
COPY --link Libraries/CO.CDP.Forms.WebApiClient.Tests/CO.CDP.Forms.WebApiClient.Tests.csproj Libraries/CO.CDP.Forms.WebApiClient.Tests/
COPY --link Libraries/CO.CDP.DataSharing.WebApiClient/CO.CDP.DataSharing.WebApiClient.csproj Libraries/CO.CDP.DataSharing.WebApiClient/
COPY --link Libraries/CO.CDP.DataSharing.WebApiClient.Tests/CO.CDP.DataSharing.WebApiClient.Tests.csproj Libraries/CO.CDP.DataSharing.WebApiClient.Tests/
COPY --link Libraries/CO.CDP.EntityVerificationClient/CO.CDP.EntityVerificationClient.csproj Libraries/CO.CDP.EntityVerificationClient/
COPY --link Libraries/CO.CDP.RegisterOfCommercialTools.WebApiClient/CO.CDP.RegisterOfCommercialTools.WebApiClient.csproj Libraries/CO.CDP.RegisterOfCommercialTools.WebApiClient/
COPY --link Libraries/CO.CDP.RegisterOfCommercialTools.WebApiClient.Tests/CO.CDP.RegisterOfCommercialTools.WebApiClient.Tests.csproj Libraries/CO.CDP.RegisterOfCommercialTools.WebApiClient.Tests/
COPY --link TestKit/CO.CDP.TestKit.Mvc/CO.CDP.TestKit.Mvc.csproj TestKit/CO.CDP.TestKit.Mvc/
COPY --link TestKit/CO.CDP.TestKit.Mvc.Tests/CO.CDP.TestKit.Mvc.Tests.csproj TestKit/CO.CDP.TestKit.Mvc.Tests/
COPY --link TestKit/CO.CDP.Testcontainers.PostgreSql/CO.CDP.Testcontainers.PostgreSql.csproj TestKit/CO.CDP.Testcontainers.PostgreSql/
COPY --link TestKit/CO.CDP.Testcontainers.PostgreSql.Tests/CO.CDP.Testcontainers.PostgreSql.Tests.csproj TestKit/CO.CDP.Testcontainers.PostgreSql.Tests/
COPY --link Services/CO.CDP.OrganisationInformation/CO.CDP.OrganisationInformation.csproj Services/CO.CDP.OrganisationInformation/
COPY --link Services/CO.CDP.OrganisationInformation.Persistence/CO.CDP.OrganisationInformation.Persistence.csproj Services/CO.CDP.OrganisationInformation.Persistence/
COPY --link Services/CO.CDP.OrganisationInformation.Persistence.Tests/CO.CDP.OrganisationInformation.Persistence.Tests.csproj Services/CO.CDP.OrganisationInformation.Persistence.Tests/
COPY --link Services/CO.CDP.OrganisationInformation.Tests/CO.CDP.OrganisationInformation.Tests.csproj Services/CO.CDP.OrganisationInformation.Tests/
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
COPY --link Services/CO.CDP.EntityVerification.Persistence/CO.CDP.EntityVerification.Persistence.csproj Services/CO.CDP.EntityVerification.Persistence/
COPY --link Services/CO.CDP.EntityVerification.Persistence.Tests/CO.CDP.EntityVerification.Persistence.Tests.csproj Services/CO.CDP.EntityVerification.Persistence.Tests/
COPY --link Services/CO.CDP.EntityVerification/CO.CDP.EntityVerification.csproj Services/CO.CDP.EntityVerification/
COPY --link Services/CO.CDP.EntityVerification.Tests/CO.CDP.EntityVerification.Tests.csproj Services/CO.CDP.EntityVerification.Tests/
COPY --link Services/CO.CDP.Localization/CO.CDP.Localization.csproj Services/CO.CDP.Localization/
COPY --link Services/CO.CDP.AntiVirusScanner/CO.CDP.AntiVirusScanner.csproj Services/CO.CDP.AntiVirusScanner/
COPY --link Services/CO.CDP.AntiVirusScanner.Tests/CO.CDP.AntiVirusScanner.Tests.csproj Services/CO.CDP.AntiVirusScanner.Tests/
COPY --link Services/CO.CDP.OutboxProcessor/CO.CDP.OutboxProcessor.csproj Services/CO.CDP.OutboxProcessor/
COPY --link Services/CO.CDP.ScheduledWorker/CO.CDP.ScheduledWorker.csproj Services/CO.CDP.ScheduledWorker/
COPY --link Services/CO.CDP.ScheduledWorker.Tests/CO.CDP.ScheduledWorker.Tests.csproj Services/CO.CDP.ScheduledWorker.Tests/
COPY --link Libraries/CO.CDP.UI.Foundation/CO.CDP.UI.Foundation.csproj Libraries/CO.CDP.UI.Foundation/
COPY --link Libraries/CO.CDP.UI.Foundation.Tests/CO.CDP.UI.Foundation.Tests.csproj Libraries/CO.CDP.UI.Foundation.Tests/
COPY --link Libraries/CO.CDP.Logging/CO.CDP.Logging.csproj Libraries/CO.CDP.Logging/
COPY --link Libraries/CO.CDP.Logging.Tests/CO.CDP.Logging.Tests.csproj Libraries/CO.CDP.Logging.Tests/
COPY --link Services/CO.CDP.RegisterOfCommercialTools.WebApi/CO.CDP.RegisterOfCommercialTools.WebApi.csproj Services/CO.CDP.RegisterOfCommercialTools.WebApi/
COPY --link Services/CO.CDP.RegisterOfCommercialTools.WebApi.Tests/CO.CDP.RegisterOfCommercialTools.WebApi.Tests.csproj Services/CO.CDP.RegisterOfCommercialTools.WebApi.Tests/
COPY --link Services/CO.CDP.RegisterOfCommercialTools.Persistence/CO.CDP.RegisterOfCommercialTools.Persistence.csproj Services/CO.CDP.RegisterOfCommercialTools.Persistence/
COPY --link Services/CO.CDP.RegisterOfCommercialTools.Persistence.Tests/CO.CDP.RegisterOfCommercialTools.Persistence.Tests.csproj Services/CO.CDP.RegisterOfCommercialTools.Persistence.Tests/
COPY --link Libraries/CO.CDP.UserManagement.Core/CO.CDP.UserManagement.Core.csproj Libraries/CO.CDP.UserManagement.Core/
COPY --link Libraries/CO.CDP.UserManagement.Shared/CO.CDP.UserManagement.Shared.csproj Libraries/CO.CDP.UserManagement.Shared/
COPY --link Libraries/CO.CDP.UserManagement.WebApiClient/CO.CDP.UserManagement.WebApiClient.csproj Libraries/CO.CDP.UserManagement.WebApiClient/
COPY --link Services/CO.CDP.UserManagement.Infrastructure/CO.CDP.UserManagement.Infrastructure.csproj Services/CO.CDP.UserManagement.Infrastructure/
COPY --link Services/CO.CDP.UserManagement.Api/CO.CDP.UserManagement.Api.csproj Services/CO.CDP.UserManagement.Api/
COPY --link Services/CO.CDP.UserManagement.UnitTests/CO.CDP.UserManagement.UnitTests.csproj Services/CO.CDP.UserManagement.UnitTests/

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

FROM build AS build-antivirus-app
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.AntiVirusScanner
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-outbox-processor
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.OutboxProcessor
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-scheduled-worker
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.ScheduledWorker
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-organisation-app
ARG BUILD_CONFIGURATION
WORKDIR /src/Frontend/CO.CDP.OrganisationApp
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-commercial-tools-app
ARG BUILD_CONFIGURATION
WORKDIR /src/Frontend/CO.CDP.RegisterOfCommercialTools.App
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-commercial-tools-api
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.RegisterOfCommercialTools.WebApi
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-user-management-api
ARG BUILD_CONFIGURATION
WORKDIR /src/Services/CO.CDP.UserManagement.Api
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-user-management-app
ARG BUILD_CONFIGURATION
WORKDIR /src/Frontend/CO.CDP.UserManagement.App
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS build-migrations-user-management
WORKDIR /src
COPY .config/dotnet-tools.json .config/
RUN dotnet tool restore
RUN dotnet ef migrations bundle -p /src/Services/CO.CDP.UserManagement.Infrastructure -s /src/Services/CO.CDP.UserManagement.Infrastructure --context UserManagementDbContext --self-contained -o /app/migrations/efbundle

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

FROM build-commercial-tools-app AS publish-commercial-tools-app
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.RegisterOfCommercialTools.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-commercial-tools-api AS publish-commercial-tools-api
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.RegisterOfCommercialTools.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-user-management-api AS publish-user-management-api
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.UserManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-user-management-app AS publish-user-management-app
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.UserManagement.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-antivirus-app AS publish-antivirus-app
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.AntiVirusScanner.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-outbox-processor AS publish-outbox-processor
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.OutboxProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-scheduled-worker AS publish-scheduled-worker
ARG BUILD_CONFIGURATION
RUN dotnet publish "CO.CDP.ScheduledWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build-tenant AS build-migrations-organisation-information
WORKDIR /src
COPY .config/dotnet-tools.json .config/
RUN dotnet tool restore
RUN dotnet ef migrations bundle -p /src/Services/CO.CDP.OrganisationInformation.Persistence -s /src/Services/CO.CDP.OrganisationInformation.Persistence --self-contained -o /app/migrations/efbundle

FROM build-entity-verification AS build-migrations-entity-verification
WORKDIR /src
COPY .config/dotnet-tools.json .config/
RUN dotnet tool restore
RUN dotnet ef migrations bundle -p /src/Services/CO.CDP.EntityVerification.Persistence -s /src/Services/CO.CDP.EntityVerification.Persistence --self-contained -o /app/migrations/efbundle

FROM build-commercial-tools-api AS build-migrations-commercial-tools
WORKDIR /src
COPY .config/dotnet-tools.json .config/
RUN dotnet tool restore
RUN dotnet ef migrations bundle -p /src/Services/CO.CDP.RegisterOfCommercialTools.Persistence -s /src/Services/CO.CDP.RegisterOfCommercialTools.Persistence --self-contained -o /app/migrations/efbundle

FROM base AS migrations-organisation-information
COPY --from=busybox:uclibc /bin/busybox /bin/busybox
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=build-migrations-organisation-information /src/Services/CO.CDP.OrganisationInformation.Persistence/OrganisationInformationDatabaseMigrationConfig /app/OrganisationInformationDatabaseMigrationConfig
COPY --from=build-migrations-organisation-information /src/Services/CO.CDP.OrganisationInformation.Persistence/StoredProcedures /app/StoredProcedures
COPY --from=build-migrations-organisation-information /app/migrations/efbundle .
ENTRYPOINT ["/bin/busybox", "sh", "-c", "/app/efbundle", "--connection", "Host=$OrganisationInformationDatabase__Host;Database=$OrganisationInformationDatabase__Database;Username=$OrganisationInformationDatabase__Username;Password=$OrganisationInformationDatabase__Password;"]

FROM base AS migrations-entity-verification
COPY --from=busybox:uclibc /bin/busybox /bin/busybox
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=build-migrations-entity-verification /src/Services/CO.CDP.EntityVerification.Persistence/EntityVerificationDatabaseMigrationConfig /app/EntityVerificationDatabaseMigrationConfig
COPY --from=build-migrations-entity-verification /app/migrations/efbundle .
ENTRYPOINT ["/bin/busybox", "sh", "-c", "/app/efbundle", "--connection", "Host=$EntityVerificationDatabase__Host;Database=$EntityVerificationDatabase__Database;Username=$EntityVerificationDatabase__Username;Password=$EntityVerificationDatabase__Password;"]

FROM base AS migrations-commercial-tools
COPY --from=busybox:uclibc /bin/busybox /bin/busybox
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=build-migrations-commercial-tools /src/Services/CO.CDP.RegisterOfCommercialTools.Persistence/RegisterOfCommercialToolsDatabaseMigrationConfig /app/RegisterOfCommercialToolsDatabaseMigrationConfig
COPY --from=build-migrations-commercial-tools /app/migrations/efbundle .
ENTRYPOINT ["/bin/busybox", "sh", "-c", "/app/efbundle", "--connection", "Host=$OrganisationInformationDatabase__Host;Database=$OrganisationInformationDatabase__Database;Username=$OrganisationInformationDatabase__Username;Password=$OrganisationInformationDatabase__Password;"]

FROM base AS final-authority
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-authority /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Organisation.Authority.dll"]

FROM base AS final-tenant
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-tenant /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Tenant.WebApi.dll"]

FROM base AS final-organisation
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-organisation /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Organisation.WebApi.dll"]

FROM base AS final-person
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-person /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Person.WebApi.dll"]

FROM base AS final-forms
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-forms /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.Forms.WebApi.dll"]

FROM base AS final-data-sharing
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-data-sharing /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.DataSharing.WebApi.dll"]

FROM base AS final-entity-verification
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-entity-verification /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.EntityVerification.dll"]

FROM base AS final-organisation-app
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-organisation-app /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.OrganisationApp.dll"]

FROM base AS final-commercial-tools-app
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-commercial-tools-app /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.RegisterOfCommercialTools.App.dll"]

FROM base AS final-commercial-tools-api
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-commercial-tools-api /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.RegisterOfCommercialTools.WebApi.dll"]

FROM base AS final-antivirus-app
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-antivirus-app /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.AntiVirusScanner.dll"]

FROM base AS final-outbox-processor-organisation
ARG VERSION
ENV VERSION=${VERSION}
ENV DbContext=OrganisationInformationContext
ENV Channel=organisation_information_outbox
WORKDIR /app
COPY --from=publish-outbox-processor /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.OutboxProcessor.dll"]

FROM base AS final-outbox-processor-entity-verification
ARG VERSION
ENV VERSION=${VERSION}
ENV DbContext=EntityVerificationContext
ENV Channel=entity_verification_outbox
WORKDIR /app
COPY --from=publish-outbox-processor /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.OutboxProcessor.dll"]

FROM base AS final-scheduled-worker
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-scheduled-worker /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.ScheduledWorker.dll"]

FROM base AS migrations-user-management
COPY --from=busybox:uclibc /bin/busybox /bin/busybox
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=build-migrations-user-management /src/Services/CO.CDP.UserManagement.Infrastructure/UserManagementDatabaseMigrationConfig /app/UserManagementDatabaseMigrationConfig
COPY --from=build-migrations-user-management /app/migrations/efbundle .
ENTRYPOINT ["/bin/busybox", "sh", "-c", "/app/efbundle --connection \"Host=$UserManagementDatabase__Server;Database=$UserManagementDatabase__Database;Username=$UserManagementDatabase__Username;Password=$UserManagementDatabase__Password;\""]

FROM base AS final-user-management-api
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-user-management-api /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.UserManagement.Api.dll"]

FROM base AS final-user-management-app
ARG VERSION
ENV VERSION=${VERSION}
WORKDIR /app
COPY --from=publish-user-management-app /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.UserManagement.App.dll"]
