FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Bootstraper/App.Server/App.Server.csproj", "src/Bootstraper/App.Server/"]
COPY ["src/Modules/Auth/App.Modules.Auth.Api/App.Modules.Auth.Api.csproj", "src/Modules/Auth/App.Modules.Auth.Api/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["src/Modules/Auth/App.Modules.Auth.Shared/App.Modules.Auth.Shared.csproj", "src/Modules/Auth/App.Modules.Auth.Shared/"]
COPY ["src/Modules/Auth/App.Modules.Auth.Core/App.Modules.Auth.Core.csproj", "src/Modules/Auth/App.Modules.Auth.Core/"]
COPY ["src/Modules/Auth/App.Modules.Auth.Application/App.Modules.Auth.Application.csproj", "src/Modules/Auth/App.Modules.Auth.Application/"]
COPY ["src/Modules/Auth/App.Modules.Auth.Infrastructure/App.Modules.Auth.Infrastructure.csproj", "src/Modules/Auth/App.Modules.Auth.Infrastructure/"]
COPY ["src/Modules/Weather/App.Models.Weather.Shared/App.Models.Weather.Shared.csproj", "src/Modules/Weather/App.Models.Weather.Shared/"]
COPY ["src/Bootstraper/App.Client/App.Client.csproj", "src/Bootstraper/App.Client/"]
RUN dotnet restore "src/Bootstraper/App.Server/App.Server.csproj"
COPY . .
WORKDIR "/src/src/Bootstraper/App.Server"
RUN dotnet build "App.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "App.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.Server.dll"]
