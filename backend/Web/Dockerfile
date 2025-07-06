# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8086
EXPOSE 8087


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["backend/Web/Web.csproj", "backend/Web/"]
COPY ["backend/Application/Application.csproj", "backend/Application/"]
COPY ["backend/Domain/Domain.csproj", "backend/Domain/"]
COPY ["backend/Infrastructure/Infrastructure.csproj", "backend/Infrastructure/"]
COPY ["backend/Infrastructure/Data/Migrations", "backend/Infrastructure/Data/Migrations"]
RUN dotnet restore "./backend/Web/Web.csproj"
COPY . .
WORKDIR "/src/backend/Web"
RUN dotnet build "./Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
COPY backend/Infrastructure/Data/Migrations /app/publish/Migrations

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanArch.Web.dll"]