# Build context is the repository root:
#   docker compose up --build            (runs the API)
#   docker build -f infrastructure/docker/api/Dockerfile --target test .   (runs the tests)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props Prodify.sln ./
COPY src/Prodify.Api/Prodify.Api.csproj src/Prodify.Api/
COPY src/Prodify.Application/Prodify.Application.csproj src/Prodify.Application/
COPY src/Prodify.Contracts/Prodify.Contracts.csproj src/Prodify.Contracts/
COPY src/Prodify.Domain/Prodify.Domain.csproj src/Prodify.Domain/
COPY src/Prodify.Infrastructure/Prodify.Infrastructure.csproj src/Prodify.Infrastructure/
COPY tests/Prodify.ArchitectureTests/Prodify.ArchitectureTests.csproj tests/Prodify.ArchitectureTests/
COPY tests/Prodify.ConcurrencyTests/Prodify.ConcurrencyTests.csproj tests/Prodify.ConcurrencyTests/
COPY tests/Prodify.IntegrationTests/Prodify.IntegrationTests.csproj tests/Prodify.IntegrationTests/
COPY tests/Prodify.UnitTests/Prodify.UnitTests.csproj tests/Prodify.UnitTests/
RUN dotnet restore Prodify.sln

COPY . .
RUN dotnet build Prodify.sln -c Release --no-restore

FROM build AS test
RUN dotnet test Prodify.sln -c Release --no-build

FROM build AS publish
RUN dotnet publish src/Prodify.Api/Prodify.Api.csproj -c Release --no-build -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Prodify.Api.dll"]
