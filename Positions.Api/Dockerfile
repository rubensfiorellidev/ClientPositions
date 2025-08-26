FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Positions.Api/Positions.Api.csproj", "Positions.Api/"]
COPY ["Positions.ConsoleApp/Positions.ConsoleApp.csproj", "Positions.ConsoleApp/"]
RUN dotnet restore "./Positions.Api/Positions.Api.csproj"
COPY . .
WORKDIR "/src/Positions.Api"
RUN dotnet build "./Positions.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Positions.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Positions.Api.dll"]