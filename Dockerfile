# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY CampCost.sln .
COPY src/CampCost.Core/CampCost.Core.csproj           src/CampCost.Core/
COPY src/CampCost.Infrastructure/CampCost.Infrastructure.csproj src/CampCost.Infrastructure/
COPY src/CampCost.Api/CampCost.Api.csproj              src/CampCost.Api/
COPY tests/CampCost.Tests/CampCost.Tests.csproj        tests/CampCost.Tests/

RUN dotnet restore

COPY . .
RUN dotnet publish src/CampCost.Api/CampCost.Api.csproj -c Release -o /publish --no-restore

# Stage 2: runtime (smaller image)
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CampCost.Api.dll"]
