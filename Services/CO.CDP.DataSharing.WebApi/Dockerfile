FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/CO.CDP.DataSharing.WebApi/CO.CDP.DataSharing.WebApi.csproj", "CO.CDP.DataSharing.WebApi/"]
RUN dotnet restore "CO.CDP.DataSharing.WebApi/CO.CDP.DataSharing.WebApi.csproj"
WORKDIR "/src/Services/CO.CDP.DataSharing.WebApi"
COPY Services/ /src/Services
RUN dotnet build "CO.CDP.DataSharing.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CO.CDP.DataSharing.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CO.CDP.DataSharing.WebApi.dll"]
