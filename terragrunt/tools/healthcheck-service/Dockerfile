# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3030

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["healthCheck/healthCheck.csproj", "healthCheck/"]
RUN dotnet restore "healthCheck/healthCheck.csproj"
COPY . .
WORKDIR "/src/healthCheck"
RUN dotnet build "healthCheck.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "healthCheck.csproj" -c Release -o /app/publish

# Use the base image to run the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:3030
ENV ASPNETCORE_PORT=3030
ENTRYPOINT ["dotnet", "healthCheck.dll"]
