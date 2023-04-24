#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["WASMWithAuth/Server/WASMWithAuth.Server.csproj", "WASMWithAuth/Server/"]
COPY ["WASMWithAuth/Client/WASMWithAuth.Client.csproj", "WASMWithAuth/Client/"]
COPY ["WASMWithAuth/Shared/WASMWithAuth.Shared.csproj", "WASMWithAuth/Shared/"]
RUN dotnet restore "WASMWithAuth/Server/WASMWithAuth.Server.csproj"
COPY . .
WORKDIR "/src/WASMWithAuth/Server"
RUN dotnet build "WASMWithAuth.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WASMWithAuth.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WASMWithAuth.Server.dll"]