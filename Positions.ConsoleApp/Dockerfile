FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Positions.ConsoleApp/Positions.ConsoleApp.csproj Positions.ConsoleApp/
RUN dotnet restore Positions.ConsoleApp/Positions.ConsoleApp.csproj

COPY . .
WORKDIR /src/Positions.ConsoleApp
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENV DOTNET_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "Positions.ConsoleApp.dll"]
