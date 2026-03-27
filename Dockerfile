# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug config)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SaaS.API/SaaS.API.csproj", "src/SaaS.API/"]
COPY ["src/SaaS.Application/SaaS.Application.csproj", "src/SaaS.Application/"]
COPY ["src/SaaS.Domain/SaaS.Domain.csproj", "src/SaaS.Domain/"]
COPY ["src/SaaS.Infrastructure/SaaS.Infrastructure.csproj", "src/SaaS.Infrastructure/"]
RUN dotnet restore "./src/SaaS.API/SaaS.API.csproj"
COPY . .
WORKDIR "/src/src/SaaS.API"
RUN dotnet build "./SaaS.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SaaS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in normal mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SaaS.API.dll"]
